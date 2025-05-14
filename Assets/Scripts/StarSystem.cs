using UnityEngine;
using System.Collections.Generic;

public class StarSystem : MonoBehaviour
{
    [Header("星星设置")]
    public int maxStars = 800;           // 增加星星数量
    public float starSpawnRadius = 100f;  // 星星生成半径
    public float minStarSize = 0.08f;     // 最小星星大小
    public float maxStarSize = 0.25f;     // 最大星星大小
    public Color starColor = new Color(1f, 1f, 1f, 0.95f); // 星星颜色（增加亮度）
    
    [Header("闪烁设置")]
    public bool enableTwinkle = true;     // 是否启用闪烁效果
    public float twinkleSpeed = 1.0f;     // 闪烁速度
    public float twinkleAmount = 0.3f;    // 闪烁幅度
    
    private GameObject starContainer;
    private List<GameObject> stars = new List<GameObject>();
    private List<float> starPhases = new List<float>(); // 用于闪烁效果的相位
    private bool starsVisible = false;
    
    // 缓存星星材质
    private Material starMaterial;
    
    void Start()
    {
        // 创建星星容器
        starContainer = new GameObject("StarContainer");
        starContainer.transform.SetParent(transform);
        
        // 生成星星
        GenerateStars();
        
        // 默认隐藏星星（白天）
        HideStars();
    }
    
    void Update()
    {
        // 如果启用闪烁效果且星星可见，更新星星闪烁
        if (enableTwinkle && starsVisible)
        {
            UpdateStarTwinkle();
        }
    }
    
    void GenerateStars()
    {
        // 创建星星
        for (int i = 0; i < maxStars; i++)
        {
            // 创建一个立方体作为星星（与项目风格一致）
            GameObject star = GameObject.CreatePrimitive(PrimitiveType.Cube);
            star.name = $"Star_{i}";
            
            // 随机位置（在天空半球上）
            float theta = Random.Range(0f, Mathf.PI * 0.5f); // 高度角（0到90度）
            float phi = Random.Range(0f, Mathf.PI * 2f);     // 方位角（0到360度）
            
            float x = starSpawnRadius * Mathf.Sin(theta) * Mathf.Cos(phi);
            float y = starSpawnRadius * Mathf.Cos(theta);
            float z = starSpawnRadius * Mathf.Sin(theta) * Mathf.Sin(phi);
            
            star.transform.position = new Vector3(x, y, z);
            
            // 随机大小
            float size = Random.Range(minStarSize, maxStarSize);
            star.transform.localScale = new Vector3(size, size, size);
            
            // 设置材质
            Renderer renderer = star.GetComponent<Renderer>();
            
            // 使用共享材质
            if (starMaterial == null)
            {
                starMaterial = new Material(Shader.Find("Standard"));
                starMaterial.SetFloat("_Mode", 3); // 透明模式
                starMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                starMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                starMaterial.SetInt("_ZWrite", 0);
                starMaterial.DisableKeyword("_ALPHATEST_ON");
                starMaterial.EnableKeyword("_ALPHABLEND_ON");
                starMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                starMaterial.renderQueue = 3000;
                starMaterial.color = starColor;
                
                // 设置发光效果
                starMaterial.SetFloat("_EmissionEnabled", 1);
                starMaterial.EnableKeyword("_EMISSION");
                starMaterial.SetColor("_EmissionColor", starColor * 1.5f);
            }
            
            renderer.sharedMaterial = starMaterial;
            
            // 移除碰撞器
            Destroy(star.GetComponent<Collider>());
            
            // 添加到星星列表
            stars.Add(star);
            
            // 添加随机相位用于闪烁
            starPhases.Add(Random.Range(0f, Mathf.PI * 2f));
            
            // 设置父对象
            star.transform.SetParent(starContainer.transform);
        }
    }
    
    void UpdateStarTwinkle()
    {
        for (int i = 0; i < stars.Count; i++)
        {
            if (stars[i] != null)
            {
                // 更新相位
                starPhases[i] += Time.deltaTime * twinkleSpeed * (0.5f + Random.value * 0.5f);
                
                // 计算闪烁强度
                float twinkle = 1.0f - twinkleAmount + twinkleAmount * Mathf.Sin(starPhases[i]);
                
                // 应用到星星颜色 - 使用共享材质的情况下，我们不能直接修改每个星星的颜色
                // 所以我们只修改发光强度
                Renderer renderer = stars[i].GetComponent<Renderer>();
                if (renderer != null)
                {
                    // 同时更新发光强度
                    renderer.material.SetColor("_EmissionColor", starColor * twinkle * 1.5f);
                }
            }
        }
    }
    
    // 公共方法：显示星星
    public void ShowStars()
    {
        starContainer.SetActive(true);
        starsVisible = true;
    }
    
    // 公共方法：隐藏星星
    public void HideStars()
    {
        starContainer.SetActive(false);
        starsVisible = false;
    }
}
