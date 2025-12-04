using UnityEngine;

// 这个脚本用于在场景中添加SnowSystem组件
[DefaultExecutionOrder(-85)] // 确保在RainManager之后执行
public class SnowManager : MonoBehaviour
{
    [Header("雪花系统设置")]
    public bool enableSnowSystem = true;
    public bool startWithSnow = false;
    public float defaultSnowIntensity = 0.5f;
    
    [Header("下雪控制开关")]
    [Tooltip("直接控制下雪开关 - 设置为true开始下雪，false停止下雪")]
    public bool isSnowingEnabled = false;
    
    private SnowSystem snowSystem;
    private bool lastSnowingState = false;
    
    void Awake()
    {
        if (!enableSnowSystem)
            return;
            
        // 检查场景中是否已经有SnowSystem
        SnowSystem existingSystem = FindObjectOfType<SnowSystem>();
        if (existingSystem == null)
        {
            // 创建雪花系统对象
            GameObject snowSystemObj = new GameObject("SnowSystem");
            snowSystem = snowSystemObj.AddComponent<SnowSystem>();
            
            // 设置雪花系统参数
            snowSystem.maxSnowflakes = 2000;
            snowSystem.snowIntensity = defaultSnowIntensity;
            snowSystem.snowSpawnRadius = 60f;
            snowSystem.snowHeight = 35f;
            snowSystem.snowColor = new Color(1.0f, 1.0f, 1.0f, 0.8f);
            snowSystem.snowflakeScale = new Vector3(0.15f, 0.15f, 0.15f);
            snowSystem.snowFallSpeed = 5f;
            snowSystem.snowDriftAmount = 1.5f;
            snowSystem.snowRotationSpeed = 50f;
            snowSystem.isSnowing = startWithSnow;
            
            Debug.Log("SnowManager: 已创建雪花系统");
        }
        else
        {
            snowSystem = existingSystem;
            Debug.Log("SnowManager: 场景中已存在雪花系统");
        }
        
        // 初始化下雪状态
        isSnowingEnabled = startWithSnow;
        lastSnowingState = isSnowingEnabled;
    }
    
    void Update()
    {
        // 检测isSnowingEnabled的变化，自动控制下雪
        if (isSnowingEnabled != lastSnowingState)
        {
            lastSnowingState = isSnowingEnabled;
            
            if (isSnowingEnabled)
            {
                StartSnow();
                Debug.Log("SnowManager: 下雪已开启");
            }
            else
            {
                StopSnow();
                Debug.Log("SnowManager: 下雪已关闭");
            }
        }
    }
    
    // 公共方法：开始下雪
    public void StartSnow()
    {
        if (snowSystem != null)
        {
            snowSystem.StartSnow();
        }
    }
    
    // 公共方法：停止下雪
    public void StopSnow()
    {
        if (snowSystem != null)
        {
            snowSystem.StopSnow();
        }
    }
    
    // 公共方法：切换雪的状态
    public void ToggleSnow()
    {
        if (snowSystem != null)
        {
            if (snowSystem.isSnowing)
            {
                snowSystem.StopSnow();
            }
            else
            {
                snowSystem.StartSnow();
            }
        }
    }
    
    // 公共方法：设置雪的强度
    public void SetSnowIntensity(float intensity)
    {
        if (snowSystem != null)
        {
            snowSystem.SetSnowIntensity(intensity);
        }
    }
}
