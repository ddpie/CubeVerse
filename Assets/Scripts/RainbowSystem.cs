using UnityEngine;
using System.Collections;

public class RainbowSystem : MonoBehaviour
{
    [Header("彩虹设置")]
    public float rainbowWidth = 40f;        // 彩虹宽度
    public float rainbowHeight = 20f;       // 彩虹高度
    public float rainbowThickness = 2f;     // 彩虹厚度
    public int rainbowSegments = 30;        // 彩虹分段数
    public int colorLayers = 7;             // 彩虹层数（7种颜色）
    public float layerSpacing = 0.5f;       // 层间距
    public float fadeInTime = 5f;           // 淡入时间
    public float displayTime = 20f;         // 显示时间
    public float fadeOutTime = 5f;          // 淡出时间
    
    [Header("彩虹颜色")]
    public Color[] rainbowColors = new Color[7] {
        new Color(1, 0, 0, 0.5f),       // 红
        new Color(1, 0.5f, 0, 0.5f),    // 橙
        new Color(1, 1, 0, 0.5f),       // 黄
        new Color(0, 1, 0, 0.5f),       // 绿
        new Color(0, 0.5f, 1, 0.5f),    // 青
        new Color(0, 0, 1, 0.5f),       // 蓝
        new Color(0.5f, 0, 1, 0.5f)     // 紫
    };
    
    private GameObject rainbowObject;
    private GameObject[] rainbowLayers;
    private bool isDisplaying = false;
    private Coroutine displayCoroutine;
    
    void Awake()
    {
        // 创建彩虹对象
        rainbowObject = new GameObject("Rainbow");
        rainbowObject.transform.SetParent(transform);
        
        // 创建彩虹层
        rainbowLayers = new GameObject[colorLayers];
        for (int i = 0; i < colorLayers; i++)
        {
            rainbowLayers[i] = CreateRainbowLayer(i);
            rainbowLayers[i].transform.SetParent(rainbowObject.transform);
        }
        
        // 初始状态为隐藏
        SetRainbowVisibility(0f);
        rainbowObject.SetActive(false);
    }
    
    GameObject CreateRainbowLayer(int layerIndex)
    {
        GameObject layer = new GameObject($"RainbowLayer_{layerIndex}");
        
        // 创建彩虹弧形
        MeshFilter meshFilter = layer.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = layer.AddComponent<MeshRenderer>();
        
        // 创建彩虹网格
        Mesh mesh = new Mesh();
        
        Vector3[] vertices = new Vector3[(rainbowSegments + 1) * 2];
        int[] triangles = new int[rainbowSegments * 6];
        Color[] colors = new Color[(rainbowSegments + 1) * 2];
        
        // 计算当前层的半径
        float radius = rainbowWidth / 2f + layerIndex * layerSpacing;
        
        // 创建顶点 - 彩虹在XY平面上，这样可以从Z轴正方向看到完整的彩虹
        for (int i = 0; i <= rainbowSegments; i++)
        {
            float angle = Mathf.PI * i / rainbowSegments;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * (rainbowHeight / rainbowWidth) * radius;
            
            // 内侧顶点
            vertices[i * 2] = new Vector3(x, y, 0);
            // 外侧顶点
            vertices[i * 2 + 1] = new Vector3(x, y, rainbowThickness);
            
            // 设置颜色
            colors[i * 2] = rainbowColors[layerIndex];
            colors[i * 2 + 1] = rainbowColors[layerIndex];
        }
        
        // 创建三角形
        for (int i = 0; i < rainbowSegments; i++)
        {
            int baseIndex = i * 6;
            int vertIndex = i * 2;
            
            // 第一个三角形
            triangles[baseIndex] = vertIndex;
            triangles[baseIndex + 1] = vertIndex + 1;
            triangles[baseIndex + 2] = vertIndex + 2;
            
            // 第二个三角形
            triangles[baseIndex + 3] = vertIndex + 1;
            triangles[baseIndex + 4] = vertIndex + 3;
            triangles[baseIndex + 5] = vertIndex + 2;
        }
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.RecalculateNormals();
        
        meshFilter.mesh = mesh;
        
        // 设置材质
        Material material = new Material(Shader.Find("Standard"));
        material.SetFloat("_Mode", 3); // 透明模式
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
        material.color = rainbowColors[layerIndex];
        
        meshRenderer.material = material;
        
        return layer;
    }
    
    // 设置彩虹可见度
    void SetRainbowVisibility(float alpha)
    {
        for (int i = 0; i < rainbowLayers.Length; i++)
        {
            MeshRenderer renderer = rainbowLayers[i].GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Color color = renderer.material.color;
                color.a = alpha * 0.5f; // 保持半透明效果
                renderer.material.color = color;
            }
        }
    }
    
    // 显示彩虹
    public void ShowRainbow(Vector3 position, float rotationY)
    {
        if (isDisplaying)
        {
            // 如果已经在显示，停止当前协程
            if (displayCoroutine != null)
            {
                StopCoroutine(displayCoroutine);
            }
        }
        
        // 设置彩虹位置和旋转
        // 计算玩家前方的位置（距离30米）
        Vector3 playerForward = Quaternion.Euler(0, rotationY, 0) * Vector3.forward;
        Vector3 rainbowPosition = position + playerForward * 30f + new Vector3(0, 15f, 0);
        
        rainbowObject.transform.position = rainbowPosition;
        // 让彩虹面向玩家
        rainbowObject.transform.rotation = Quaternion.Euler(0, rotationY + 180f, 0);
        
        Debug.Log("RainbowSystem: 准备显示彩虹，位置: " + rainbowObject.transform.position);
        
        // 启动显示协程
        displayCoroutine = StartCoroutine(DisplayRainbowCoroutine());
    }
    
    IEnumerator DisplayRainbowCoroutine()
    {
        isDisplaying = true;
        rainbowObject.SetActive(true);
        
        // 淡入
        float elapsedTime = 0f;
        while (elapsedTime < fadeInTime)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInTime);
            SetRainbowVisibility(alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        SetRainbowVisibility(1f);
        
        // 显示一段时间
        yield return new WaitForSeconds(displayTime);
        
        // 淡出
        elapsedTime = 0f;
        while (elapsedTime < fadeOutTime)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutTime);
            SetRainbowVisibility(alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 隐藏彩虹
        SetRainbowVisibility(0f);
        rainbowObject.SetActive(false);
        isDisplaying = false;
    }
}
