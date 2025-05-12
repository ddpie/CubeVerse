using UnityEngine;
using System.Collections.Generic;

public class RainSystem : MonoBehaviour
{
    [Header("雨滴设置")]
    public GameObject rainDropPrefab; // 雨滴预制体（会使用与云朵相同的方块）
    public int maxRainDrops = 3000; // 最大雨滴数量（增加）
    public float rainIntensity = 0.7f; // 雨的强度 (0-1)（增加）
    public float rainSpawnRadius = 60f; // 雨滴生成半径（增加）
    public float rainHeight = 35f; // 雨滴生成高度（增加）
    
    [Header("雨滴外观")]
    public Color rainColor = new Color(0.7f, 0.7f, 1.0f, 0.7f); // 雨滴颜色（淡蓝色半透明）
    public Vector3 rainDropScale = new Vector3(0.08f, 0.5f, 0.08f); // 雨滴形状（更粗更长）
    
    [Header("雨滴行为")]
    public float rainSpeed = 20f; // 雨滴下落速度
    public float rainDirection = 0f; // 雨的方向（角度，0表示垂直下落）
    public bool isRaining = false; // 是否正在下雨
    
    private Transform player; // 玩家位置引用
    private List<GameObject> rainDrops = new List<GameObject>(); // 雨滴对象池
    private List<GameObject> activeRainDrops = new List<GameObject>(); // 活动的雨滴
    private Vector3 lastPlayerPosition; // 上次更新雨滴位置时的玩家位置
    // 玩家移动阈值，用于平滑雨滴跟随
    private float playerMoveThreshold = 5f;
    
    void Start()
    {
        // 延迟初始化，等待玩家生成
        Invoke("Initialize", 1.0f);
    }
    
    void Initialize()
    {
        // 查找玩家
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log("雨滴系统找到玩家: " + player.position);
            lastPlayerPosition = player.position;
            
            // 初始化雨滴预制体
            InitializeRainDropPrefab();
            
            // 创建雨滴对象池
            CreateRainDropPool();
            
            // 如果默认下雨，开始下雨
            if (isRaining)
            {
                StartRain();
            }
        }
        else if (Camera.main != null)
        {
            player = Camera.main.transform;
            Debug.Log("雨滴系统找到相机: " + player.position);
            lastPlayerPosition = player.position;
            
            // 初始化雨滴预制体
            InitializeRainDropPrefab();
            
            // 创建雨滴对象池
            CreateRainDropPool();
            
            // 如果默认下雨，开始下雨
            if (isRaining)
            {
                StartRain();
            }
        }
        else
        {
            // 如果还没找到相机，继续尝试
            Invoke("Initialize", 0.5f);
            Debug.Log("雨滴系统等待玩家初始化...");
        }
    }
    
    void InitializeRainDropPrefab()
    {
        // 如果没有指定雨滴预制体，尝试使用与云朵相同的方块
        if (rainDropPrefab == null)
        {
            // 尝试从CloudGenerator获取预制体
            CloudGenerator cloudGenerator = FindObjectOfType<CloudGenerator>();
            if (cloudGenerator != null && cloudGenerator.cubePrefab != null)
            {
                rainDropPrefab = cloudGenerator.cubePrefab;
                Debug.Log("雨滴系统从CloudGenerator获取到方块预制体");
            }
            else
            {
                // 尝试从CubeGenerator获取预制体
                CubeGenerator cubeGenerator = FindObjectOfType<CubeGenerator>();
                if (cubeGenerator != null && cubeGenerator.cubePrefab != null)
                {
                    rainDropPrefab = cubeGenerator.cubePrefab;
                    Debug.Log("雨滴系统从CubeGenerator获取到方块预制体");
                }
                else
                {
                    // 如果找不到，创建一个简单的立方体
                    Debug.LogWarning("雨滴系统无法找到方块预制体，将创建一个默认立方体");
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.name = "RainDropCube";
                    cube.SetActive(false); // 隐藏原始预制体
                    DontDestroyOnLoad(cube); // 防止被销毁
                    rainDropPrefab = cube;
                }
            }
        }
    }
    
    void CreateRainDropPool()
    {
        // 创建雨滴对象池
        for (int i = 0; i < maxRainDrops; i++)
        {
            GameObject rainDrop = Instantiate(rainDropPrefab, Vector3.zero, Quaternion.identity, transform);
            rainDrop.name = $"RainDrop_{i}";
            
            // 设置雨滴外观
            ConfigureRainDrop(rainDrop);
            
            // 初始状态为不活动
            rainDrop.SetActive(false);
            rainDrops.Add(rainDrop);
        }
        
        Debug.Log($"雨滴系统创建了 {maxRainDrops} 个雨滴");
    }
    
    void ConfigureRainDrop(GameObject rainDrop)
    {
        // 设置雨滴大小
        rainDrop.transform.localScale = rainDropScale;
        
        // 设置雨滴颜色
        Renderer renderer = rainDrop.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(renderer.material);
            material.color = rainColor;
            
            // 设置为半透明
            material.SetFloat("_Mode", 3); // 透明模式
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
            
            renderer.material = material;
        }
        
        // 移除碰撞器，让雨滴可以穿过物体
        Collider collider = rainDrop.GetComponent<Collider>();
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
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("雨滴系统找到玩家: " + player.position);
                lastPlayerPosition = player.position;
            }
            else if (Camera.main != null)
            {
                player = Camera.main.transform;
                Debug.Log("雨滴系统找到相机: " + player.position);
                lastPlayerPosition = player.position;
            }
            else
            {
                return; // 如果没有玩家，不执行更新
            }
        }
        
        // 如果正在下雨，更新雨滴
        if (isRaining)
        {
            // 更新雨滴下落
            UpdateRainDrops();
            
            // 每帧都生成新的雨滴，保持持续的雨效果
            SpawnNewRainDrops();
            
            // 如果玩家移动，只更新一小部分雨滴的位置，避免突然的变化
            if (Vector3.Distance(player.position, lastPlayerPosition) > playerMoveThreshold)
            {
                lastPlayerPosition = player.position;
                // 不再调用UpdateRainPosition()，而是在SpawnNewRainDrops中处理新雨滴的位置
            }
        }
    }
    
    void UpdateRainPosition()
    {
        // 将所有活动的雨滴重新定位到玩家周围
        foreach (GameObject rainDrop in activeRainDrops)
        {
            // 只重新定位高于玩家的雨滴，并且随机选择一部分雨滴进行重定位
            // 这样可以避免所有雨滴同时重定位导致的"一阵一阵"效果
            if (rainDrop.transform.position.y > player.position.y && Random.value < 0.3f)
            {
                PositionRainDropAbovePlayer(rainDrop);
            }
        }
    }
    
    void UpdateRainDrops()
    {
        // 计算雨滴下落方向
        Vector3 fallDirection = Quaternion.Euler(0, rainDirection, 0) * Vector3.down;
        
        // 更新所有活动雨滴的位置
        for (int i = activeRainDrops.Count - 1; i >= 0; i--)
        {
            GameObject rainDrop = activeRainDrops[i];
            
            // 移动雨滴
            rainDrop.transform.position += fallDirection * rainSpeed * Time.deltaTime;
            
            // 如果雨滴落到地面以下，直接重新定位到上方（不回收）
            if (rainDrop.transform.position.y < player.position.y - 10)
            {
                // 始终重新定位雨滴，保持持续下雨效果
                PositionRainDropAbovePlayer(rainDrop);
                
                // 给每个重新定位的雨滴一个随机的初始下落距离，避免同时落地
                Vector3 pos = rainDrop.transform.position;
                pos.y -= Random.Range(0f, 15f);
                rainDrop.transform.position = pos;
            }
        }
    }
    
    void SpawnNewRainDrops()
    {
        // 每帧生成固定数量的雨滴，确保持续均匀的雨效果
        int baseRainDropsPerFrame = 10; // 基础每帧生成数量
        int rainDropsToSpawn = baseRainDropsPerFrame + Mathf.FloorToInt(rainIntensity * 20); // 根据强度增加生成数量
        
        // 确保不超过最大数量
        int availableDrops = maxRainDrops - activeRainDrops.Count;
        rainDropsToSpawn = Mathf.Min(rainDropsToSpawn, availableDrops);
        
        // 生成新的雨滴
        for (int i = 0; i < rainDropsToSpawn; i++)
        {
            if (rainDrops.Count > 0)
            {
                // 从对象池中获取一个雨滴
                GameObject rainDrop = rainDrops[0];
                rainDrops.RemoveAt(0);
                
                // 定位雨滴 - 使用更均匀的分布
                PositionRainDropAbovePlayer(rainDrop);
                
                // 给每个雨滴一个随机的初始高度，避免同时落地
                Vector3 pos = rainDrop.transform.position;
                pos.y -= Random.Range(0f, 15f);
                rainDrop.transform.position = pos;
                
                // 激活雨滴
                rainDrop.SetActive(true);
                activeRainDrops.Add(rainDrop);
            }
        }
    }
    
    void PositionRainDropAbovePlayer(GameObject rainDrop)
    {
        // 使用更均匀的分布方式在玩家上方生成雨滴
        // 将圆形区域分成扇形，确保雨滴分布更均匀
        float sectorSize = 10f; // 将圆分成36个扇区
        float sectorIndex = Random.Range(0, 36);
        float angle = sectorIndex * sectorSize;
        
        // 在扇区内随机选择距离
        float minDistance = rainSpawnRadius * 0.2f; // 最小距离为半径的20%
        float distance = Random.Range(minDistance, rainSpawnRadius);
        
        // 添加一些随机偏移，使雨滴不会完全沿着扇区边界排列
        angle += Random.Range(-sectorSize/3, sectorSize/3);
        
        Vector3 spawnPosition = player.position + new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad) * distance,
            rainHeight + Random.Range(-5f, 5f), // 高度也添加随机变化
            Mathf.Sin(angle * Mathf.Deg2Rad) * distance
        );
        
        rainDrop.transform.position = spawnPosition;
    }
    
    // 公共方法：开始下雨
    public void StartRain()
    {
        if (!isRaining)
        {
            isRaining = true;
            Debug.Log("开始下雨");
        }
    }
    
    // 公共方法：停止下雨
    public void StopRain()
    {
        if (isRaining)
        {
            isRaining = false;
            Debug.Log("停止下雨");
            
            // 回收所有活动的雨滴
            foreach (GameObject rainDrop in activeRainDrops)
            {
                rainDrop.SetActive(false);
                rainDrops.Add(rainDrop);
            }
            
            activeRainDrops.Clear();
        }
    }
    
    // 公共方法：设置雨的强度
    public void SetRainIntensity(float intensity)
    {
        rainIntensity = Mathf.Clamp01(intensity);
        Debug.Log($"雨的强度设置为: {rainIntensity}");
    }
}
