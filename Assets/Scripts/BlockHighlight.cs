using UnityEngine;

/// <summary>
/// 方块高亮显示 - 显示玩家当前瞄准的方块
/// </summary>
public class BlockHighlight : MonoBehaviour
{
    [Header("高亮设置")]
    public Color highlightColor = new Color(1f, 1f, 1f, 0.3f);
    public float blockSize = 1f;
    public float outlineWidth = 0.02f;
    
    private GameObject highlightCube;
    private Material highlightMaterial;
    private LineRenderer[] outlineRenderers;
    private bool isVisible = false;
    
    // 破坏进度相关
    private float destroyProgress = 0f;
    private GameObject progressOverlay;
    private Material progressMaterial;
    
    void Start()
    {
        CreateHighlightCube();
        CreateOutline();
        CreateProgressOverlay();
        SetVisible(false);
    }
    
    /// <summary>
    /// 创建高亮方块
    /// </summary>
    void CreateHighlightCube()
    {
        highlightCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        highlightCube.name = "HighlightCube";
        highlightCube.transform.SetParent(transform);
        highlightCube.transform.localScale = Vector3.one * (blockSize + 0.01f);
        
        // 移除碰撞体
        Collider col = highlightCube.GetComponent<Collider>();
        if (col != null) Destroy(col);
        
        // 创建半透明材质
        highlightMaterial = new Material(Shader.Find("Standard"));
        highlightMaterial.color = highlightColor;
        
        // 设置为透明模式
        highlightMaterial.SetFloat("_Mode", 3);
        highlightMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        highlightMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        highlightMaterial.SetInt("_ZWrite", 0);
        highlightMaterial.DisableKeyword("_ALPHATEST_ON");
        highlightMaterial.EnableKeyword("_ALPHABLEND_ON");
        highlightMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        highlightMaterial.renderQueue = 3000;
        
        Renderer renderer = highlightCube.GetComponent<Renderer>();
        renderer.material = highlightMaterial;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
    }
    
    /// <summary>
    /// 创建边框线
    /// </summary>
    void CreateOutline()
    {
        outlineRenderers = new LineRenderer[12];
        
        // 定义立方体的12条边
        Vector3[][] edges = new Vector3[][]
        {
            // 底面4条边
            new Vector3[] { new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f) },
            new Vector3[] { new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, 0.5f) },
            new Vector3[] { new Vector3(0.5f, -0.5f, 0.5f), new Vector3(-0.5f, -0.5f, 0.5f) },
            new Vector3[] { new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(-0.5f, -0.5f, -0.5f) },
            // 顶面4条边
            new Vector3[] { new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f) },
            new Vector3[] { new Vector3(0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f) },
            new Vector3[] { new Vector3(0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f) },
            new Vector3[] { new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, -0.5f) },
            // 垂直4条边
            new Vector3[] { new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f) },
            new Vector3[] { new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f) },
            new Vector3[] { new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f) },
            new Vector3[] { new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f) }
        };
        
        Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
        lineMaterial.color = Color.black;
        
        for (int i = 0; i < 12; i++)
        {
            GameObject lineObj = new GameObject($"OutlineLine_{i}");
            lineObj.transform.SetParent(transform);
            
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = lineMaterial;
            lr.startWidth = outlineWidth;
            lr.endWidth = outlineWidth;
            lr.positionCount = 2;
            lr.useWorldSpace = false;
            lr.SetPosition(0, edges[i][0] * blockSize);
            lr.SetPosition(1, edges[i][1] * blockSize);
            lr.startColor = Color.black;
            lr.endColor = Color.black;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            
            outlineRenderers[i] = lr;
        }
    }
    
    /// <summary>
    /// 设置高亮目标位置
    /// </summary>
    public void SetTarget(Vector3 position, bool visible)
    {
        transform.position = position;
        SetVisible(visible);
    }
    
    /// <summary>
    /// 设置可见性
    /// </summary>
    void SetVisible(bool visible)
    {
        if (isVisible == visible) return;
        
        isVisible = visible;
        
        if (highlightCube != null)
        {
            highlightCube.SetActive(visible);
        }
        
        if (outlineRenderers != null)
        {
            foreach (var lr in outlineRenderers)
            {
                if (lr != null)
                {
                    lr.gameObject.SetActive(visible);
                }
            }
        }
    }
    
    void OnDestroy()
    {
        if (highlightMaterial != null)
        {
            Destroy(highlightMaterial);
        }
        if (progressMaterial != null)
        {
            Destroy(progressMaterial);
        }
    }
    
    /// <summary>
    /// 创建破坏进度覆盖层
    /// </summary>
    void CreateProgressOverlay()
    {
        progressOverlay = GameObject.CreatePrimitive(PrimitiveType.Cube);
        progressOverlay.name = "ProgressOverlay";
        progressOverlay.transform.SetParent(transform);
        progressOverlay.transform.localScale = Vector3.one * (blockSize + 0.02f);
        progressOverlay.transform.localPosition = Vector3.zero;
        
        // 移除碰撞体
        Collider col = progressOverlay.GetComponent<Collider>();
        if (col != null) Destroy(col);
        
        // 创建破坏进度材质（红色半透明）
        progressMaterial = new Material(Shader.Find("Standard"));
        progressMaterial.color = new Color(1f, 0f, 0f, 0f);
        
        // 设置为透明模式
        progressMaterial.SetFloat("_Mode", 3);
        progressMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        progressMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        progressMaterial.SetInt("_ZWrite", 0);
        progressMaterial.DisableKeyword("_ALPHATEST_ON");
        progressMaterial.EnableKeyword("_ALPHABLEND_ON");
        progressMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        progressMaterial.renderQueue = 3001;
        
        Renderer renderer = progressOverlay.GetComponent<Renderer>();
        renderer.material = progressMaterial;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        
        progressOverlay.SetActive(false);
    }
    
    /// <summary>
    /// 设置破坏进度 (0-1)
    /// </summary>
    public void SetDestroyProgress(float progress)
    {
        destroyProgress = Mathf.Clamp01(progress);
        
        if (progressOverlay != null && progressMaterial != null)
        {
            if (destroyProgress > 0)
            {
                progressOverlay.SetActive(true);
                // 进度越高，红色越深
                progressMaterial.color = new Color(1f, 0.2f, 0.2f, destroyProgress * 0.6f);
            }
            else
            {
                progressOverlay.SetActive(false);
            }
        }
    }
}
