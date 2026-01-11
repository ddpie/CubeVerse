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
    
    void Start()
    {
        CreateHighlightCube();
        CreateOutline();
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
    }
}
