using UnityEngine;
using System.Collections.Generic;

public class SnowSystem : MonoBehaviour
{
    [Header("雪花设置")]
    public GameObject snowflakePrefab; // 雪花预制体（会使用与云朵相同的方块）
    public int maxSnowflakes = 2000; // 最大雪花数量
    public float snowIntensity = 0.7f; // 雪的强度 (0-1)
    public float snowSpawnRadius = 60f; // 雪花生成半径
    public float snowHeight = 35f; // 雪花生成高度
    
    [Header("雪花外观")]
    public Color snowColor = new Color(1.0f, 1.0f, 1.0f, 0.8f); // 雪花颜色（白色半透明）
    public Vector3 snowflakeScale = new Vector3(0.15f, 0.15f, 0.15f); // 雪花形状（小立方体）
    
    [Header("雪花行为")]
    public float snowFallSpeed = 5f; // 雪花下落速度（比雨慢）
    public float snowDriftAmount = 1.5f; // 雪花漂移量
    public float snowRotationSpeed = 50f; // 雪花旋转速度
    public bool isSnowing = false; // 是否正在下雪
    
    private Transform player; // 玩家位置引用
    private List<GameObject> snowflakes = new List<GameObject>(); // 雪花对象池
    private List<GameObject> activeSnowflakes = new List<GameObject>(); // 活动的雪花
    private List<Vector3> snowflakeRotations = new List<Vector3>(); // 每个雪花的旋转速度
    private List<Vector3> snowflakeDrifts = new List<Vector3>(); // 每个雪花的漂移方向
    private Vector3 lastPlayerPosition; // 上次更新雪花位置时的玩家位置
    private float updateDistance = 5f; // 玩家移动多远后更新雪花位置
    private float timeSinceLastDriftChange = 0f;
    private float driftChangeInterval = 2f; // 每隔多少秒改变一次漂移方向
    
    void Start()
    {
        // 延迟初始化，等待玩家生成
        Invoke("Initialize", 1.0f);
    }
    
    void Initialize()
    {
        // 首先尝试从GameManager获取玩家引用
        if (GameManager.Instance != null && GameManager.Instance.playerTransform != null)
        {
            player = GameManager.Instance.playerTransform;
            lastPlayerPosition = player.position;
            
            // 初始化雪花预制体
            InitializeSnowflakePrefab();
            
            // 创建雪花对象池
            CreateSnowflakePool();
            
            // 如果默认下雪，开始下雪
            if (isSnowing)
            {
                StartSnow();
            }
        }
        else
        {
            // 查找玩家
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                lastPlayerPosition = player.position;
                
                // 初始化雪花预制体
                InitializeSnowflakePrefab();
                
                // 创建雪花对象池
                CreateSnowflakePool();
                
                // 如果默认下雪，开始下雪
                if (isSnowing)
                {
                    StartSnow();
                }
            }
            else if (Camera.main != null)
            {
                player = Camera.main.transform;
                lastPlayerPosition = player.position;
                
                // 初始化雪花预制体
                InitializeSnowflakePrefab();
                
                // 创建雪花对象池
                CreateSnowflakePool();
                
                // 如果默认下雪，开始下雪
                if (isSnowing)
                {
                    StartSnow();
                }
            }
            else
            {
                // 如果还没找到相机，继续尝试
                Invoke("Initialize", 0.5f);
            }
        }
    }
    
    void InitializeSnowflakePrefab()
    {
        // 如果没有指定雪花预制体，尝试使用与云朵相同的方块
        if (snowflakePrefab == null)
        {
            // 尝试从CloudGenerator获取预制体
            CloudGenerator cloudGenerator = FindObjectOfType<CloudGenerator>();
            if (cloudGenerator != null && cloudGenerator.cubePrefab != null)
            {
                snowflakePrefab = cloudGenerator.cubePrefab;
                Debug.Log("雪花系统从CloudGenerator获取到方块预制体");
            }
            else
            {
                // 尝试从CubeGenerator获取预制体
                CubeGenerator cubeGenerator = FindObjectOfType<CubeGenerator>();
                if (cubeGenerator != null && cubeGenerator.cubePrefab != null)
                {
                    snowflakePrefab = cubeGenerator.cubePrefab;
                    Debug.Log("雪花系统从CubeGenerator获取到方块预制体");
                }
                else
                {
                    // 如果找不到，创建一个简单的立方体
                    Debug.LogWarning("雪花系统无法找到方块预制体，将创建一个默认立方体");
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.name = "SnowflakeCube";
                    cube.SetActive(false); // 隐藏原始预制体
                    DontDestroyOnLoad(cube); // 防止被销毁
                    snowflakePrefab = cube;
                }
            }
        }
    }
    
    void CreateSnowflakePool()
    {
        // 创建雪花对象池
        for (int i = 0; i < maxSnowflakes; i++)
        {
            GameObject snowflake = Instantiate(snowflakePrefab, Vector3.zero, Quaternion.identity, transform);
            snowflake.name = $"Snowflake_{i}";
            
            // 设置雪花外观
            ConfigureSnowflake(snowflake);
            
            // 初始状态为不活动
            snowflake.SetActive(false);
            snowflakes.Add(snowflake);
        }
        
        Debug.Log($"雪花系统创建了 {maxSnowflakes} 个雪花");
    }
    
    // 缓存雪花材质
    private Material snowflakeMaterial;
    
    void ConfigureSnowflake(GameObject snowflake)
    {
        // 设置雪花大小
        snowflake.transform.localScale = snowflakeScale;
        
        // 设置雪花颜色
        Renderer renderer = snowflake.GetComponent<Renderer>();
        if (renderer != null)
        {
            // 使用共享材质而不是每次创建新材质
            if (snowflakeMaterial == null)
            {
                snowflakeMaterial = new Material(renderer.material);
                snowflakeMaterial.color = snowColor;
                
                // 设置为半透明
                snowflakeMaterial.SetFloat("_Mode", 3); // 透明模式
                snowflakeMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                snowflakeMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                snowflakeMaterial.SetInt("_ZWrite", 0);
                snowflakeMaterial.DisableKeyword("_ALPHATEST_ON");
                snowflakeMaterial.EnableKeyword("_ALPHABLEND_ON");
                snowflakeMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                snowflakeMaterial.renderQueue = 3000;
            }
            renderer.sharedMaterial = snowflakeMaterial;
        }
        
        // 移除碰撞器，让雪花可以穿过物体
        Collider collider = snowflake.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
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
        
        // 如果正在下雪，更新雪花
        if (isSnowing)
        {
            // 更新雪花下落
            UpdateSnowflakes();
            
            // 每帧都生成新的雪花，保持持续的雪效果
            SpawnNewSnowflakes();
            
            // 更新漂移方向
            timeSinceLastDriftChange += Time.deltaTime;
            if (timeSinceLastDriftChange >= driftChangeInterval)
            {
                UpdateSnowflakeDrifts();
                timeSinceLastDriftChange = 0f;
            }
            
            // 如果玩家移动，更新雪花位置
            if (Vector3.Distance(player.position, lastPlayerPosition) > updateDistance)
            {
                lastPlayerPosition = player.position;
                // 不再调用UpdateSnowPosition()，而是在SpawnNewSnowflakes中处理新雪花的位置
            }
        }
    }
    
    void UpdateSnowflakeDrifts()
    {
        // 为每个活动的雪花更新漂移方向
        for (int i = 0; i < activeSnowflakes.Count; i++)
        {
            if (i < snowflakeDrifts.Count)
            {
                // 生成一个新的随机漂移方向
                Vector3 newDrift = new Vector3(
                    Random.Range(-snowDriftAmount, snowDriftAmount),
                    0,
                    Random.Range(-snowDriftAmount, snowDriftAmount)
                );
                
                // 平滑过渡到新的漂移方向
                snowflakeDrifts[i] = Vector3.Lerp(snowflakeDrifts[i], newDrift, 0.3f);
            }
        }
    }
    
    void UpdateSnowflakes()
    {
        // 更新所有活动雪花的位置和旋转
        for (int i = activeSnowflakes.Count - 1; i >= 0; i--)
        {
            GameObject snowflake = activeSnowflakes[i];
            
            // 应用下落和漂移
            Vector3 movement = Vector3.down * snowFallSpeed;
            
            // 添加漂移效果
            if (i < snowflakeDrifts.Count)
            {
                movement += snowflakeDrifts[i];
            }
            
            // 移动雪花
            snowflake.transform.position += movement * Time.deltaTime;
            
            // 旋转雪花
            if (i < snowflakeRotations.Count)
            {
                snowflake.transform.Rotate(snowflakeRotations[i] * Time.deltaTime);
            }
            
            // 如果雪花落到地面以下，直接重新定位到上方（不回收）
            if (snowflake.transform.position.y < player.position.y - 10)
            {
                // 始终重新定位雪花，保持持续下雪效果
                PositionSnowflakeAbovePlayer(snowflake);
                
                // 给每个重新定位的雪花一个随机的初始下落距离，避免同时落地
                Vector3 pos = snowflake.transform.position;
                pos.y -= Random.Range(0f, 15f);
                snowflake.transform.position = pos;
            }
        }
    }
    
    void SpawnNewSnowflakes()
    {
        // 每帧生成固定数量的雪花，确保持续均匀的雪效果
        int baseSnowflakesPerFrame = 5; // 基础每帧生成数量
        int snowflakesToSpawn = baseSnowflakesPerFrame + Mathf.FloorToInt(snowIntensity * 15); // 根据强度增加生成数量
        
        // 确保不超过最大数量
        int availableSnowflakes = maxSnowflakes - activeSnowflakes.Count;
        snowflakesToSpawn = Mathf.Min(snowflakesToSpawn, availableSnowflakes);
        
        // 生成新的雪花
        for (int i = 0; i < snowflakesToSpawn; i++)
        {
            if (snowflakes.Count > 0)
            {
                // 从对象池中获取一个雪花
                GameObject snowflake = snowflakes[0];
                snowflakes.RemoveAt(0);
                
                // 定位雪花
                PositionSnowflakeAbovePlayer(snowflake);
                
                // 给每个雪花一个随机的初始高度，避免同时落地
                Vector3 pos = snowflake.transform.position;
                pos.y -= Random.Range(0f, 15f);
                snowflake.transform.position = pos;
                
                // 为雪花设置随机旋转速度
                Vector3 rotationSpeed = new Vector3(
                    Random.Range(-snowRotationSpeed, snowRotationSpeed),
                    Random.Range(-snowRotationSpeed, snowRotationSpeed),
                    Random.Range(-snowRotationSpeed, snowRotationSpeed)
                );
                snowflakeRotations.Add(rotationSpeed);
                
                // 为雪花设置随机漂移方向
                Vector3 driftDirection = new Vector3(
                    Random.Range(-snowDriftAmount, snowDriftAmount),
                    0,
                    Random.Range(-snowDriftAmount, snowDriftAmount)
                );
                snowflakeDrifts.Add(driftDirection);
                
                // 激活雪花
                snowflake.SetActive(true);
                activeSnowflakes.Add(snowflake);
            }
        }
    }
    
    void PositionSnowflakeAbovePlayer(GameObject snowflake)
    {
        // 使用更均匀的分布方式在玩家上方生成雪花
        float sectorSize = 10f; // 将圆分成36个扇区
        float sectorIndex = Random.Range(0, 36);
        float angle = sectorIndex * sectorSize;
        
        // 在扇区内随机选择距离
        float minDistance = snowSpawnRadius * 0.2f; // 最小距离为半径的20%
        float distance = Random.Range(minDistance, snowSpawnRadius);
        
        // 添加一些随机偏移，使雪花不会完全沿着扇区边界排列
        angle += Random.Range(-sectorSize/3, sectorSize/3);
        
        Vector3 spawnPosition = player.position + new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad) * distance,
            snowHeight + Random.Range(-5f, 5f), // 高度也添加随机变化
            Mathf.Sin(angle * Mathf.Deg2Rad) * distance
        );
        
        snowflake.transform.position = spawnPosition;
    }
    
    // 公共方法：开始下雪
    public void StartSnow()
    {
        if (!isSnowing)
        {
            isSnowing = true;
            Debug.Log("开始下雪");
        }
    }
    
    // 公共方法：停止下雪
    public void StopSnow()
    {
        if (isSnowing)
        {
            isSnowing = false;
            Debug.Log("停止下雪");
            
            // 回收所有活动的雪花
            foreach (GameObject snowflake in activeSnowflakes)
            {
                snowflake.SetActive(false);
                snowflakes.Add(snowflake);
            }
            
            activeSnowflakes.Clear();
            snowflakeRotations.Clear();
            snowflakeDrifts.Clear();
        }
    }
    
    // 公共方法：设置雪的强度
    public void SetSnowIntensity(float intensity)
    {
        snowIntensity = Mathf.Clamp01(intensity);
        Debug.Log($"雪的强度设置为: {snowIntensity}");
    }
}
