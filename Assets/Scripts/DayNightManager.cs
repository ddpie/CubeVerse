using UnityEngine;

public class DayNightManager : MonoBehaviour
{
    [Header("日夜设置")]
    public bool isNight = false; // 默认为白天
    public Color dayLightColor = new Color(1f, 0.95f, 0.85f); // 白天的光照颜色
    public Color nightLightColor = new Color(0.03f, 0.03f, 0.15f); // 夜晚的光照颜色（更深的蓝色）
    public float dayLightIntensity = 1.0f; // 白天的光照强度
    public float nightLightIntensity = 0.15f; // 夜晚的光照强度（更暗）
    public Color daySkyColor = new Color(0.5f, 0.7f, 1f); // 白天的天空颜色
    public Color nightSkyColor = new Color(0.01f, 0.01f, 0.03f); // 夜晚的天空颜色（更暗）
    
    [Header("星星设置")]
    public bool enableStars = true;      // 是否启用星星系统
    
    private Light directionalLight;
    private StarSystem starSystem;
    
    void Start()
    {
        // 查找场景中的主方向光
        directionalLight = FindObjectOfType<Light>();
        if (directionalLight == null)
        {
            Debug.LogError("未找到方向光源！");
            return;
        }
        
        // 初始化星星系统
        if (enableStars)
        {
            InitializeStarSystem();
        }
        
        // 初始化日夜状态
        UpdateLighting();
    }
    
    // 初始化星星系统
    void InitializeStarSystem()
    {
        // 检查是否已经有StarSystem
        StarSystem existingSystem = FindObjectOfType<StarSystem>();
        if (existingSystem == null)
        {
            // 创建星星系统对象
            GameObject starSystemObj = new GameObject("StarSystem");
            starSystem = starSystemObj.AddComponent<StarSystem>();
            
            // 设置星星系统参数
            starSystem.maxStars = 500;
            starSystem.starSpawnRadius = 100f;
            starSystem.minStarSize = 0.08f;
            starSystem.maxStarSize = 0.25f;
            starSystem.starColor = new Color(1f, 1f, 1f, 0.9f);
            starSystem.enableTwinkle = true;
            starSystem.twinkleSpeed = 1.2f;
            starSystem.twinkleAmount = 0.4f;
            
            Debug.Log("DayNightManager: 已创建星星系统");
        }
        else
        {
            starSystem = existingSystem;
            Debug.Log("DayNightManager: 场景中已存在星星系统");
        }
    }
    
    // 切换日夜状态
    public void ToggleDayNight()
    {
        isNight = !isNight;
        UpdateLighting();
        Debug.Log("切换到" + (isNight ? "夜晚" : "白天") + "模式");
    }
    
    // 设置为白天
    public void SetDay()
    {
        if (isNight)
        {
            isNight = false;
            UpdateLighting();
            Debug.Log("设置为白天模式");
        }
    }
    
    // 设置为夜晚
    public void SetNight()
    {
        if (!isNight)
        {
            isNight = true;
            UpdateLighting();
            Debug.Log("设置为夜晚模式");
        }
    }
    
    // 更新光照和天空颜色
    private void UpdateLighting()
    {
        if (directionalLight != null)
        {
            // 更新光照颜色和强度
            directionalLight.color = isNight ? nightLightColor : dayLightColor;
            directionalLight.intensity = isNight ? nightLightIntensity : dayLightIntensity;
            
            // 更新天空颜色
            RenderSettings.ambientLight = isNight ? nightSkyColor : daySkyColor;
            
            // 调整光源角度，夜晚时光源角度更高（模拟月光）
            if (isNight)
            {
                // 夜晚时，将光源角度调整为从上方照射（模拟月光）
                directionalLight.transform.rotation = Quaternion.Euler(80, directionalLight.transform.rotation.eulerAngles.y, 0);
            }
            else
            {
                // 白天时，将光源角度调整为从侧面照射（模拟太阳光）
                directionalLight.transform.rotation = Quaternion.Euler(50, directionalLight.transform.rotation.eulerAngles.y, 0);
            }
            
            // 如果有雾效，也可以调整雾的颜色和密度
            if (RenderSettings.fog)
            {
                RenderSettings.fogColor = isNight ? 
                    new Color(nightSkyColor.r * 0.5f, nightSkyColor.g * 0.5f, nightSkyColor.b * 0.5f) : 
                    new Color(daySkyColor.r * 0.8f, daySkyColor.g * 0.8f, daySkyColor.b * 0.8f);
                
                // 夜晚时雾效更浓
                RenderSettings.fogDensity = isNight ? 0.04f : 0.01f;
            }
            
            // 调整环境反射强度
            RenderSettings.reflectionIntensity = isNight ? 0.3f : 1.0f;
            
            // 显示或隐藏星星
            if (starSystem != null)
            {
                if (isNight)
                {
                    starSystem.ShowStars();
                    Debug.Log("DayNightManager: 夜晚模式，显示星星");
                }
                else
                {
                    starSystem.HideStars();
                    Debug.Log("DayNightManager: 白天模式，隐藏星星");
                }
            }
        }
    }
}
