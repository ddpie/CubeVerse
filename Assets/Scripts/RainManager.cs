using UnityEngine;
using System.Collections.Generic;

// 这个脚本用于在场景中添加RainSystem组件
[DefaultExecutionOrder(-90)] // 确保在CloudManager之后执行
public class RainManager : MonoBehaviour
{
    [Header("雨滴系统设置")]
    public bool enableRainSystem = true;
    public bool startWithRain = false;
    public float defaultRainIntensity = 0.5f;
    
    private RainSystem rainSystem;
    private List<LightningManager> lightningManagers = new List<LightningManager>();
    
    void Awake()
    {
        if (!enableRainSystem)
            return;
            
        // 检查场景中是否已经有RainSystem
        RainSystem existingSystem = FindObjectOfType<RainSystem>();
        if (existingSystem == null)
        {
            // 创建雨滴系统对象
            GameObject rainSystemObj = new GameObject("RainSystem");
            rainSystem = rainSystemObj.AddComponent<RainSystem>();
            
            // 设置雨滴系统参数
            rainSystem.maxRainDrops = 3000;
            rainSystem.rainIntensity = defaultRainIntensity;
            rainSystem.rainSpawnRadius = 60f;
            rainSystem.rainHeight = 35f;
            rainSystem.rainColor = new Color(0.7f, 0.7f, 1.0f, 0.7f);
            rainSystem.rainDropScale = new Vector3(0.08f, 0.5f, 0.08f);
            rainSystem.rainSpeed = 25f;
            rainSystem.rainDirection = 0f;
            rainSystem.isRaining = startWithRain;
            
            Debug.Log("RainManager: 已创建雨滴系统");
        }
        else
        {
            rainSystem = existingSystem;
            Debug.Log("RainManager: 场景中已存在雨滴系统");
        }
        
        // 延迟查找闪电管理器，确保它们已经初始化
        Invoke("FindLightningManagers", 1.5f);
    }
    
    void FindLightningManagers()
    {
        // 查找所有闪电管理器
        LightningManager[] managers = FindObjectsOfType<LightningManager>();
        lightningManagers.Clear();
        
        foreach (LightningManager manager in managers)
        {
            lightningManagers.Add(manager);
        }
        
        // 通知闪电管理器当前雨的状态
        if (rainSystem != null)
        {
            NotifyLightningManagers(rainSystem.isRaining);
        }
        
        Debug.Log($"RainManager: 找到 {lightningManagers.Count} 个闪电管理器");
    }
    
    // 通知所有闪电管理器雨的状态变化
    void NotifyLightningManagers(bool isRaining)
    {
        foreach (LightningManager manager in lightningManagers)
        {
            if (manager != null)
            {
                manager.OnRainStateChanged(isRaining);
            }
        }
    }
    
    // 公共方法：开始下雨
    public void StartRain()
    {
        if (rainSystem != null)
        {
            rainSystem.StartRain();
            NotifyLightningManagers(true);
        }
    }
    
    // 公共方法：停止下雨
    public void StopRain()
    {
        if (rainSystem != null)
        {
            bool wasRaining = rainSystem.isRaining;
            rainSystem.StopRain();
            NotifyLightningManagers(false);
            
            // 记录日志，帮助调试彩虹系统
            if (wasRaining)
            {
                Debug.Log("RainManager: 雨已停止，应该触发彩虹效果");
            }
        }
    }
    
    // 公共方法：切换雨的状态
    public void ToggleRain()
    {
        if (rainSystem != null)
        {
            bool newRainState;
            
            if (rainSystem.isRaining)
            {
                rainSystem.StopRain();
                newRainState = false;
            }
            else
            {
                rainSystem.StartRain();
                newRainState = true;
            }
            
            NotifyLightningManagers(newRainState);
        }
    }
    
    // 公共方法：设置雨的强度
    public void SetRainIntensity(float intensity)
    {
        if (rainSystem != null)
        {
            rainSystem.SetRainIntensity(intensity);
        }
    }
}
