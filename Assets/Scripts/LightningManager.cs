using UnityEngine;

// 这个脚本用于在场景中添加LightningSystem组件
[DefaultExecutionOrder(-85)] // 确保在RainManager之后执行
public class LightningManager : MonoBehaviour
{
    [Header("闪电系统设置")]
    public bool enableLightningSystem = true;
    public float lightningChance = 0.05f; // 每秒触发闪电的概率
    public float minLightningInterval = 3f; // 两次闪电之间的最小间隔时间
    public float maxLightningInterval = 15f; // 两次闪电之间的最大间隔时间
    
    private LightningSystem lightningSystem;
    private RainManager rainManager;
    
    void Awake()
    {
        if (!enableLightningSystem)
            return;
            
        // 检查场景中是否已经有LightningSystem
        LightningSystem existingSystem = FindObjectOfType<LightningSystem>();
        if (existingSystem == null)
        {
            // 创建闪电系统对象
            GameObject lightningSystemObj = new GameObject("LightningSystem");
            lightningSystem = lightningSystemObj.AddComponent<LightningSystem>();
            
            // 设置闪电系统参数
            lightningSystem.lightningChance = lightningChance;
            lightningSystem.minLightningInterval = minLightningInterval;
            lightningSystem.maxLightningInterval = maxLightningInterval;
            
            Debug.Log("LightningManager: 已创建闪电系统");
        }
        else
        {
            lightningSystem = existingSystem;
            Debug.Log("LightningManager: 场景中已存在闪电系统");
        }
        
        // 获取雨滴管理器
        rainManager = FindObjectOfType<RainManager>();
        if (rainManager != null)
        {
            // 监听雨滴系统状态变化
            Invoke("ConnectToRainSystem", 1.0f);
        }
    }
    
    void ConnectToRainSystem()
    {
        if (rainManager != null && lightningSystem != null)
        {
            // 获取RainSystem组件
            RainSystem rainSystem = FindObjectOfType<RainSystem>();
            if (rainSystem != null)
            {
                // 设置初始雨状态
                lightningSystem.SetRaining(rainSystem.isRaining);
                Debug.Log("LightningManager: 已连接到雨滴系统");
            }
        }
    }
    
    // 公共方法：手动触发闪电
    public void TriggerLightning()
    {
        if (lightningSystem != null)
        {
            lightningSystem.TriggerManualLightning();
        }
    }
    
    // 当雨开始或停止时调用
    public void OnRainStateChanged(bool isRaining)
    {
        if (lightningSystem != null)
        {
            lightningSystem.SetRaining(isRaining);
            Debug.Log("LightningManager: 雨状态变更为 " + (isRaining ? "下雨" : "停雨"));
        }
    }
}
