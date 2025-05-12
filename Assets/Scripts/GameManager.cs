using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("游戏设置")]
    public GameObject playerPrefab;
    public Vector3 spawnPosition = new Vector3(0, 20, 0); // 玩家生成位置，高度设置高一些以便让玩家落到地面上
    
    [Header("天气设置")]
    public bool enableWeatherSystem = true;
    public bool startWithRain = false;
    public float defaultRainIntensity = 0.7f;
    public bool startWithSnow = false;
    public float defaultSnowIntensity = 0.5f;
    public bool enableLightning = true; // 是否启用闪电系统
    
    [Header("日夜设置")]
    public bool enableDayNightSystem = true;
    public bool startWithNight = false;
    
    private GameObject player;
    private RainManager rainManager;
    private SnowManager snowManager;
    private LightningManager lightningManager;
    private DayNightManager dayNightManager;
    
    void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        InitializeGame();
    }
    
    void InitializeGame()
    {
        // 生成玩家
        SpawnPlayer();
        
        // 初始化云朵系统
        InitializeCloudSystem();
        
        // 初始化雨滴系统
        if (enableWeatherSystem)
        {
            InitializeRainSystem();
            InitializeSnowSystem();
            
            // 初始化闪电系统
            if (enableLightning)
            {
                InitializeLightningSystem();
            }
        }
        
        // 初始化日夜系统
        if (enableDayNightSystem)
        {
            InitializeDayNightSystem();
        }
    }
    
    void InitializeCloudSystem()
    {
        // 检查是否已经有CloudGenerator
        CloudGenerator existingGenerator = FindObjectOfType<CloudGenerator>();
        if (existingGenerator == null)
        {
            // 创建云朵生成器对象
            GameObject cloudGeneratorObj = new GameObject("CloudGenerator");
            CloudGenerator cloudGenerator = cloudGeneratorObj.AddComponent<CloudGenerator>();
            
            // 设置云朵生成器参数
            cloudGenerator.cubePrefab = FindCubePrefab();
            cloudGenerator.cloudCount = 10;
            cloudGenerator.minCloudHeight = 30f;
            cloudGenerator.maxCloudHeight = 50f;
            cloudGenerator.cloudSpawnRadius = 100f;
            cloudGenerator.minCubesPerCloud = 15;
            cloudGenerator.maxCubesPerCloud = 40;
            cloudGenerator.cloudDensity = 0.7f;
            cloudGenerator.cloudColor = Color.white;
            
            Debug.Log("GameManager: 已创建云朵生成器");
        }
    }
    
    void InitializeRainSystem()
    {
        // 检查是否已经有RainManager
        RainManager existingManager = FindObjectOfType<RainManager>();
        if (existingManager == null)
        {
            // 创建雨滴管理器对象
            GameObject rainManagerObj = new GameObject("RainManager");
            rainManager = rainManagerObj.AddComponent<RainManager>();
            
            // 设置雨滴管理器参数
            rainManager.enableRainSystem = true;
            rainManager.startWithRain = startWithRain;
            rainManager.defaultRainIntensity = defaultRainIntensity;
            
            Debug.Log("GameManager: 已创建雨滴管理器");
        }
        else
        {
            rainManager = existingManager;
            Debug.Log("GameManager: 场景中已存在雨滴管理器");
        }
    }
    
    void InitializeSnowSystem()
    {
        // 检查是否已经有SnowManager
        SnowManager existingManager = FindObjectOfType<SnowManager>();
        if (existingManager == null)
        {
            // 创建雪花管理器对象
            GameObject snowManagerObj = new GameObject("SnowManager");
            snowManager = snowManagerObj.AddComponent<SnowManager>();
            
            // 设置雪花管理器参数
            snowManager.enableSnowSystem = true;
            snowManager.startWithSnow = startWithSnow;
            snowManager.defaultSnowIntensity = defaultSnowIntensity;
            
            Debug.Log("GameManager: 已创建雪花管理器");
        }
        else
        {
            snowManager = existingManager;
            Debug.Log("GameManager: 场景中已存在雪花管理器");
        }
    }
    
    void InitializeLightningSystem()
    {
        // 检查是否已经有LightningManager
        LightningManager existingManager = FindObjectOfType<LightningManager>();
        if (existingManager == null)
        {
            // 创建闪电管理器对象
            GameObject lightningManagerObj = new GameObject("LightningManager");
            lightningManager = lightningManagerObj.AddComponent<LightningManager>();
            
            // 设置闪电管理器参数
            lightningManager.enableLightningSystem = true;
            lightningManager.lightningChance = 0.05f;
            lightningManager.minLightningInterval = 3f;
            lightningManager.maxLightningInterval = 15f;
            
            Debug.Log("GameManager: 已创建闪电管理器");
        }
        else
        {
            lightningManager = existingManager;
            Debug.Log("GameManager: 场景中已存在闪电管理器");
        }
    }
    
    void InitializeDayNightSystem()
    {
        // 检查是否已经有DayNightManager
        DayNightManager existingManager = FindObjectOfType<DayNightManager>();
        if (existingManager == null)
        {
            // 创建日夜管理器对象
            GameObject dayNightManagerObj = new GameObject("DayNightManager");
            dayNightManager = dayNightManagerObj.AddComponent<DayNightManager>();
            
            // 设置日夜管理器参数
            dayNightManager.isNight = startWithNight;
            
            Debug.Log("GameManager: 已创建日夜管理器");
        }
        else
        {
            dayNightManager = existingManager;
            Debug.Log("GameManager: 场景中已存在日夜管理器");
        }
    }
    
    // 查找场景中的方块预制体
    private GameObject FindCubePrefab()
    {
        // 尝试从CubeGenerator获取预制体
        CubeGenerator cubeGenerator = FindObjectOfType<CubeGenerator>();
        if (cubeGenerator != null && cubeGenerator.cubePrefab != null)
        {
            Debug.Log("GameManager: 从CubeGenerator获取到方块预制体");
            return cubeGenerator.cubePrefab;
        }
        
        // 如果找不到，创建一个简单的立方体
        Debug.LogWarning("GameManager: 无法找到方块预制体，将创建一个默认立方体");
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "CloudCube";
        cube.SetActive(false); // 隐藏原始预制体
        DontDestroyOnLoad(cube); // 防止被销毁
        return cube;
    }
    
    void SpawnPlayer()
    {
        if (playerPrefab != null)
        {
            player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            player.tag = "Player"; // 确保设置标签
            Debug.Log("玩家已生成在位置: " + spawnPosition);
            
            // 确保只有一个AudioListener
            RemoveExtraAudioListeners();
        }
        else
        {
            Debug.LogError("未设置玩家预制体！");
        }
    }
    
    public void RespawnPlayer()
    {
        if (player != null)
        {
            // 保存玩家旋转，这样重生后视角不会改变
            Quaternion rotation = player.transform.rotation;
            Destroy(player);
            player = Instantiate(playerPrefab, spawnPosition, rotation);
            player.tag = "Player"; // 确保设置标签
            Debug.Log("玩家已重生在位置: " + spawnPosition);
            
            // 确保只有一个AudioListener
            RemoveExtraAudioListeners();
        }
        else
        {
            SpawnPlayer();
        }
    }
    
    void RemoveExtraAudioListeners()
    {
        // 查找所有AudioListener
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        
        if (listeners.Length > 1)
        {
            Debug.LogWarning("场景中有多个AudioListener，正在修复...");
            
            // 保留玩家相机上的AudioListener，删除其他的
            AudioListener playerListener = null;
            
            // 查找玩家相机上的AudioListener
            if (player != null)
            {
                Camera playerCamera = player.GetComponentInChildren<Camera>();
                if (playerCamera != null)
                {
                    playerListener = playerCamera.GetComponent<AudioListener>();
                }
            }
            
            // 如果没有找到玩家相机上的AudioListener，保留第一个找到的
            if (playerListener == null && listeners.Length > 0)
            {
                playerListener = listeners[0];
            }
            
            // 删除其他AudioListener
            foreach (AudioListener listener in listeners)
            {
                if (listener != playerListener)
                {
                    Debug.Log("删除多余的AudioListener: " + listener.gameObject.name);
                    Destroy(listener);
                }
            }
        }
    }
    
    // 公共方法：开始下雨
    public void StartRain()
    {
        if (rainManager != null)
        {
            rainManager.StartRain();
        }
    }
    
    // 公共方法：停止下雨
    public void StopRain()
    {
        if (rainManager != null)
        {
            rainManager.StopRain();
        }
    }
    
    // 公共方法：切换雨的状态
    public void ToggleRain()
    {
        if (rainManager != null)
        {
            rainManager.ToggleRain();
        }
    }
    
    // 公共方法：设置雨的强度
    public void SetRainIntensity(float intensity)
    {
        if (rainManager != null)
        {
            rainManager.SetRainIntensity(intensity);
        }
    }
    
    // 公共方法：开始下雪
    public void StartSnow()
    {
        if (snowManager != null)
        {
            snowManager.StartSnow();
        }
    }
    
    // 公共方法：停止下雪
    public void StopSnow()
    {
        if (snowManager != null)
        {
            snowManager.StopSnow();
        }
    }
    
    // 公共方法：切换雪的状态
    public void ToggleSnow()
    {
        if (snowManager != null)
        {
            snowManager.ToggleSnow();
        }
    }
    
    // 公共方法：设置雪的强度
    public void SetSnowIntensity(float intensity)
    {
        if (snowManager != null)
        {
            snowManager.SetSnowIntensity(intensity);
        }
    }
    
    // 公共方法：手动触发闪电
    public void TriggerLightning()
    {
        if (lightningManager != null)
        {
            lightningManager.TriggerLightning();
        }
    }
    
    // 公共方法：切换日夜状态
    public void ToggleDayNight()
    {
        if (dayNightManager != null)
        {
            dayNightManager.ToggleDayNight();
        }
    }
    
    // 公共方法：设置为白天
    public void SetDay()
    {
        if (dayNightManager != null)
        {
            dayNightManager.SetDay();
        }
    }
    
    // 公共方法：设置为夜晚
    public void SetNight()
    {
        if (dayNightManager != null)
        {
            dayNightManager.SetNight();
        }
    }
}
