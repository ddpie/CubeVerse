using UnityEngine;

[DefaultExecutionOrder(-85)] // 确保在RainManager之后执行
public class RainbowManager : MonoBehaviour
{
    [Header("彩虹系统设置")]
    public bool enableRainbowSystem = true;
    
    private RainbowSystem rainbowSystem;
    private RainManager rainManager;
    private Transform player;
    private bool wasRaining = false;
    
    void Awake()
    {
        if (!enableRainbowSystem)
            return;
            
        // 检查场景中是否已经有RainbowSystem
        RainbowSystem existingSystem = FindObjectOfType<RainbowSystem>();
        if (existingSystem == null)
        {
            // 创建彩虹系统对象
            GameObject rainbowSystemObj = new GameObject("RainbowSystem");
            rainbowSystem = rainbowSystemObj.AddComponent<RainbowSystem>();
            
            Debug.Log("RainbowManager: 已创建彩虹系统");
        }
        else
        {
            rainbowSystem = existingSystem;
            Debug.Log("RainbowManager: 场景中已存在彩虹系统");
        }
        
        // 查找雨滴管理器
        rainManager = FindObjectOfType<RainManager>();
        if (rainManager != null)
        {
            Debug.Log("RainbowManager: 找到雨滴管理器");
            
            // 获取初始雨状态
            if (rainManager.enableRainSystem && rainManager.startWithRain)
            {
                wasRaining = true;
            }
        }
        else
        {
            Debug.LogWarning("RainbowManager: 未找到雨滴管理器");
        }
    }
    
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
            Debug.Log("RainbowManager: 找到玩家: " + player.position);
        }
        else if (Camera.main != null)
        {
            player = Camera.main.transform;
            Debug.Log("RainbowManager: 找到相机: " + player.position);
        }
        else
        {
            // 如果还没找到相机，继续尝试
            Invoke("Initialize", 0.5f);
            Debug.Log("RainbowManager: 等待玩家初始化...");
        }
    }
    
    void Update()
    {
        if (!enableRainbowSystem || rainbowSystem == null || rainManager == null || player == null)
            return;
            
        // 检查雨的状态变化
        CheckRainState();
    }
    
    void CheckRainState()
    {
        // 获取当前雨的状态
        bool isRaining = false;
        
        // 从RainSystem获取状态
        RainSystem rainSystem = FindObjectOfType<RainSystem>();
        if (rainSystem != null)
        {
            isRaining = rainSystem.isRaining;
            Debug.Log($"RainbowManager: 当前雨状态 - isRaining: {isRaining}, wasRaining: {wasRaining}");
        }
        
        // 检测雨是否刚刚停止
        if (wasRaining && !isRaining)
        {
            // 雨刚刚停止，显示彩虹
            ShowRainbow();
        }
        
        // 更新上一帧的雨状态
        wasRaining = isRaining;
    }
    
    void ShowRainbow()
    {
        if (rainbowSystem != null && player != null)
        {
            // 获取玩家位置和方向
            Vector3 position = player.position;
            
            // 获取玩家的前进方向（使用玩家的旋转）
            float rotationY = player.rotation.eulerAngles.y;
            
            // 显示彩虹
            rainbowSystem.ShowRainbow(position, rotationY);
            
            Debug.Log("RainbowManager: 雨停止，显示彩虹！位置: " + position + ", 旋转: " + rotationY);
        }
        else
        {
            Debug.LogError("RainbowManager: 无法显示彩虹，rainbowSystem或player为null");
        }
    }
}
