using UnityEngine;

public class DayNightManager : MonoBehaviour
{
    [Header("日夜设置")]
    public bool isNight = false;
    public Color dayLightColor = new Color(1f, 0.95f, 0.85f); // 白天的光照颜色
    public Color nightLightColor = new Color(0.05f, 0.05f, 0.2f); // 夜晚的光照颜色（更深的蓝色）
    public float dayLightIntensity = 1.0f; // 白天的光照强度
    public float nightLightIntensity = 0.2f; // 夜晚的光照强度（更暗）
    public Color daySkyColor = new Color(0.5f, 0.7f, 1f); // 白天的天空颜色
    public Color nightSkyColor = new Color(0.02f, 0.02f, 0.05f); // 夜晚的天空颜色（更暗）
    
    [Header("星星设置")]
    public bool showStarsAtNight = true;
    public int starCount = 200;
    public float starSize = 0.05f;
    public Color starColor = new Color(1f, 1f, 1f, 0.8f);
    
    private Light directionalLight;
    private GameObject starsContainer;
    private GameObject[] stars;
    
    void Start()
    {
        // 查找场景中的主方向光
        directionalLight = FindObjectOfType<Light>();
        if (directionalLight == null)
        {
            Debug.LogError("未找到方向光源！");
            return;
        }
        
        // 创建星星
        if (showStarsAtNight)
        {
            CreateStars();
        }
        
        // 初始化日夜状态
        UpdateLighting();
    }
    
    // 创建星星
    void CreateStars()
    {
        // 创建星星容器
        starsContainer = new GameObject("Stars");
        starsContainer.transform.parent = transform;
        
        // 创建星星
        stars = new GameObject[starCount];
        for (int i = 0; i < starCount; i++)
        {
            // 创建星星游戏对象
            stars[i] = GameObject.CreatePrimitive(PrimitiveType.Quad);
            stars[i].name = "Star_" + i;
            stars[i].transform.parent = starsContainer.transform;
            
            // 随机位置（在天空半球上）
            float theta = Random.Range(0f, Mathf.PI * 0.5f); // 高度角（0到90度）
            float phi = Random.Range(0f, Mathf.PI * 2f); // 方位角（0到360度）
            float radius = 100f; // 天空半球半径
            
            float x = radius * Mathf.Sin(theta) * Mathf.Cos(phi);
            float y = radius * Mathf.Cos(theta);
            float z = radius * Mathf.Sin(theta) * Mathf.Sin(phi);
            
            stars[i].transform.position = new Vector3(x, y, z);
            
            // 让星星面向球心
            stars[i].transform.LookAt(Vector3.zero);
            
            // 设置大小
            float randomSize = Random.Range(starSize * 0.5f, starSize * 1.5f);
            stars[i].transform.localScale = new Vector3(randomSize, randomSize, randomSize);
            
            // 创建材质
            Material starMaterial = new Material(Shader.Find("Unlit/Transparent"));
            starMaterial.color = starColor;
            
            // 应用材质
            Renderer renderer = stars[i].GetComponent<Renderer>();
            renderer.material = starMaterial;
            
            // 初始状态隐藏
            stars[i].SetActive(false);
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
                RenderSettings.fogDensity = isNight ? 0.03f : 0.01f;
            }
            
            // 调整环境反射强度
            RenderSettings.reflectionIntensity = isNight ? 0.3f : 1.0f;
            
            // 显示或隐藏星星
            if (stars != null && stars.Length > 0)
            {
                foreach (GameObject star in stars)
                {
                    if (star != null)
                    {
                        star.SetActive(isNight);
                    }
                }
            }
        }
    }
}
