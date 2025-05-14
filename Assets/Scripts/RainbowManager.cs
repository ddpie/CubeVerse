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
        // 首先尝试从GameManager获取玩家引用
        if (GameManager.Instance != null && GameManager.Instance.playerTransform != null)
        {
            player = GameManager.Instance.playerTransform;
        }
        else
        {
            // 查找玩家
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else if (Camera.main != null)
            {
                player = Camera.main.transform;
            }
            else
            {
                // 如果还没找到相机，继续尝试
                Invoke("Initialize", 0.5f);
            }
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
        }
    }
}
