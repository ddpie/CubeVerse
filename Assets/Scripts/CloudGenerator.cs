using UnityEngine;
using System.Collections.Generic;

public class CloudGenerator : MonoBehaviour
{
    [Header("云朵设置")]
    public GameObject cubePrefab;
    public int cloudCount = 10;
    public float minCloudHeight = 30f;
    public float maxCloudHeight = 50f;
    public float cloudSpawnRadius = 100f;
    
    [Header("云朵形状")]
    public int minCubesPerCloud = 15;
    public int maxCubesPerCloud = 40;
    public float cloudDensity = 0.7f;
    
    [Header("颜色设置")]
    public Color cloudColor = Color.white;
    
    private Transform player;
    private List<GameObject> clouds = new List<GameObject>();
    private Vector3 lastPlayerPosition;
    private float updateDistance = 20f;
    
    void Start()
    {
        // 延迟初始化，等待玩家生成
        Invoke("InitializeClouds", 1.0f);
    }
    
    void InitializeClouds()
    {
        // 首先尝试从GameManager获取玩家引用
        if (GameManager.Instance != null && GameManager.Instance.playerTransform != null)
        {
            player = GameManager.Instance.playerTransform;
            Debug.Log("云朵系统从GameManager获取玩家引用");
            lastPlayerPosition = player.position;
            
            // 生成初始云朵
            GenerateInitialClouds();
        }
        // 如果GameManager不可用，则使用传统方法
        else
        {
            // 查找玩家
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("云朵系统找到玩家: " + player.position);
                lastPlayerPosition = player.position;
                
                // 生成初始云朵
                GenerateInitialClouds();
            }
            else if (Camera.main != null)
            {
                player = Camera.main.transform;
                Debug.Log("云朵系统找到相机: " + player.position);
                lastPlayerPosition = player.position;
                
                // 生成初始云朵
                GenerateInitialClouds();
            }
            else
            {
                // 如果还没找到相机，继续尝试
                Invoke("InitializeClouds", 0.5f);
                Debug.Log("云朵系统等待玩家初始化...");
            }
        }
    }
    
    void Update()
    {
        // 确保有玩家
        if (player == null)
        {
            // 首先尝试从GameManager获取玩家引用
            if (GameManager.Instance != null && GameManager.Instance.playerTransform != null)
            {
                player = GameManager.Instance.playerTransform;
                lastPlayerPosition = player.position;
            }
            else
            {
                GameObject playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null)
                {
                    player = playerObj.transform;
                    lastPlayerPosition = player.position;
                }
                else if (Camera.main != null)
                {
                    player = Camera.main.transform;
                    lastPlayerPosition = player.position;
                }
                else
                {
                    return; // 如果没有玩家，不执行更新
                }
            }
        }
        
        // 如果玩家移动了足够远的距离，更新云朵
        if (Vector3.Distance(player.position, lastPlayerPosition) > updateDistance)
        {
            lastPlayerPosition = player.position;
            UpdateClouds();
        }
        
        // 缓慢移动云朵，模拟风的效果
        MoveClouds();
    }
    
    void GenerateInitialClouds()
    {
        for (int i = 0; i < cloudCount; i++)
        {
            CreateCloud();
        }
        Debug.Log($"初始化生成了 {cloudCount} 朵云");
    }
    
    void UpdateClouds()
    {
        // 移除远处的云朵，添加新的云朵
        List<GameObject> cloudsToRemove = new List<GameObject>();
        
        foreach (GameObject cloud in clouds)
        {
            if (Vector3.Distance(player.position, cloud.transform.position) > cloudSpawnRadius * 1.5f)
            {
                cloudsToRemove.Add(cloud);
            }
        }
        
        foreach (GameObject cloud in cloudsToRemove)
        {
            clouds.Remove(cloud);
            Destroy(cloud);
            CreateCloud(); // 为每个移除的云朵创建一个新的
        }
        
        Debug.Log($"更新了 {cloudsToRemove.Count} 朵云");
    }
    
    void CreateCloud()
    {
        // 在玩家周围随机位置创建云朵
        float angle = Random.Range(0f, 360f);
        float distance = Random.Range(cloudSpawnRadius * 0.3f, cloudSpawnRadius);
        float height = Random.Range(minCloudHeight, maxCloudHeight);
        
        Vector3 spawnPosition = player.position + new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad) * distance,
            height - player.position.y,
            Mathf.Sin(angle * Mathf.Deg2Rad) * distance
        );
        
        // 创建云朵容器
        GameObject cloud = new GameObject($"Cloud_{clouds.Count}");
        cloud.transform.position = spawnPosition;
        cloud.transform.parent = transform;
        clouds.Add(cloud);
        
        // 生成云朵形状
        GenerateCloudShape(cloud.transform);
    }
    
    void GenerateCloudShape(Transform cloudParent)
    {
        // 确定这朵云的大小
        int cubeCount = Random.Range(minCubesPerCloud, maxCubesPerCloud);
        
        // 云朵的基本形状参数
        float cloudWidth = Random.Range(5f, 10f);
        float cloudHeight = Random.Range(2f, 4f);
        float cloudDepth = Random.Range(5f, 10f);
        
        // 创建云朵中心
        Vector3 cloudCenter = cloudParent.position;
        
        // 生成云朵形状
        for (int i = 0; i < cubeCount; i++)
        {
            // 使用高斯分布创建更自然的云朵形状
            float x = Random.Range(-cloudWidth, cloudWidth) * 0.5f;
            float y = Random.Range(-cloudHeight, cloudHeight) * 0.5f;
            float z = Random.Range(-cloudDepth, cloudDepth) * 0.5f;
            
            // 使用二次函数创建更像云的形状（底部平，顶部圆润）
            float heightFactor = 1 - (y / cloudHeight) * (y / cloudHeight);
            x *= heightFactor;
            z *= heightFactor;
            
            // 随机决定是否放置方块（创建更蓬松的外观）
            if (Random.value < cloudDensity)
            {
                Vector3 position = cloudCenter + new Vector3(x, y, z);
                CreateCloudCube(position, cloudParent);
            }
        }
    }
    
    // 缓存云朵材质
    private Material cloudMaterial;
    
    void CreateCloudCube(Vector3 position, Transform parent)
    {
        GameObject cube = Instantiate(cubePrefab, position, Quaternion.identity, parent);
        
        // 设置方块颜色为白色（云朵颜色）
        Renderer renderer = cube.GetComponent<Renderer>();
        if (renderer != null)
        {
            // 使用共享材质而不是每次创建新材质
            if (cloudMaterial == null)
            {
                cloudMaterial = new Material(renderer.material);
                cloudMaterial.color = cloudColor;
            }
            renderer.sharedMaterial = cloudMaterial;
        }
        
        // 移除碰撞器，让玩家可以穿过云朵
        Collider collider = cube.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }
    }
    
    void MoveClouds()
    {
        // 缓慢移动所有云朵，模拟风的效果
        float windSpeed = 0.5f;
        Vector3 windDirection = new Vector3(1, 0, 0); // 风向从西到东
        
        foreach (GameObject cloud in clouds)
        {
            cloud.transform.position += windDirection * windSpeed * Time.deltaTime;
        }
    }
}
