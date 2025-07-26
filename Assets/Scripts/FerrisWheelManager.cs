using UnityEngine;
using System.Collections.Generic;

public class FerrisWheelManager : MonoBehaviour
{
    [Header("基础设置")]
    public GameObject cubePrefab;
    public int cabinCount = 12;
    public float wheelRadius = 25f;
    public float rotationSpeed = 8f;
    
    [Header("颜色设置")]
    public Color frameColor = new Color(0.8f, 0.8f, 0.9f);  // 银白色支架
    public Color wheelColor = new Color(0.3f, 0.6f, 1f);    // 蓝色轮圈
    public Color accentColor = new Color(1f, 0.8f, 0.2f);   // 金色装饰
    public Color cabinColor = new Color(0.9f, 0.95f, 1f);   // 白色车厢
    public Color lightColor = new Color(1f, 0.9f, 0.5f);    // 暖黄色灯光
    
    private List<GameObject> cabins = new List<GameObject>();
    private GameObject wheel;
    private GameObject frame;
    private GameObject decorations;
    private float timeOffset;
    
    void Start()
    {
        timeOffset = Random.value * 10f;
        
        // 如果没有设置方块预制体，尝试从CubeGenerator获取
        if (cubePrefab == null)
        {
            CubeGenerator cubeGen = FindObjectOfType<CubeGenerator>();
            if (cubeGen != null)
            {
                cubePrefab = cubeGen.cubePrefab;
                Debug.Log("获取到方块预制体");
            }
            else
            {
                Debug.LogError("找不到方块预制体！");
                return;
            }
        }
        
        CreateFerrisWheel();
    }
    
    void Update()
    {
        if (wheel != null)
        {
            wheel.transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        }
        
        UpdateLights();
    }
    
    void UpdateLights()
    {
        if (decorations == null) return;

        float time = Time.time + timeOffset;
        float pulse = Mathf.PingPong(time * 0.5f, 1f);
        Color currentLight = Color.Lerp(lightColor * 0.6f, lightColor * 1.2f, pulse);
        
        foreach (Renderer renderer in decorations.GetComponentsInChildren<Renderer>())
        {
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = currentLight;
                renderer.material.EnableKeyword("_EMISSION");
                renderer.material.SetColor("_EmissionColor", currentLight * 0.5f);
            }
        }
    }
    
    void CreateFerrisWheel()
    {
        Debug.Log("开始创建摩天轮...");
        
        // 创建父物体
        GameObject ferrisWheel = new GameObject("FerrisWheel");
        ferrisWheel.transform.parent = transform;
        ferrisWheel.transform.localPosition = Vector3.zero;
        
        // 创建支架
        frame = new GameObject("Frame");
        frame.transform.parent = ferrisWheel.transform;
        frame.transform.localPosition = Vector3.zero;
        CreateFrame();
        
        // 创建装饰
        decorations = new GameObject("Decorations");
        decorations.transform.parent = ferrisWheel.transform;
        decorations.transform.localPosition = Vector3.zero;
        CreateDecorations();
        
        // 创建轮圈和轮辐
        wheel = new GameObject("Wheel");
        wheel.transform.parent = ferrisWheel.transform;
        wheel.transform.localPosition = Vector3.zero;
        CreateWheel();
        
        // 创建车厢
        CreateCabins();
        
        Debug.Log("摩天轮创建完成！");
    }
    
    void CreateFrame()
    {
        float pillarHeight = wheelRadius * 2.2f;
        float pillarWidth = wheelRadius * 0.4f;
        
        // 主支柱
        CreatePillar(-wheelRadius - pillarWidth/2, 0, pillarWidth, pillarHeight);
        CreatePillar(wheelRadius + pillarWidth/2, 0, pillarWidth, pillarHeight);
        
        // 横梁连接
        for (float y = pillarHeight * 0.3f; y < pillarHeight; y += pillarHeight * 0.3f)
        {
            for (float x = -wheelRadius - pillarWidth; x <= wheelRadius + pillarWidth; x += 1f)
            {
                CreateCube(new Vector3(x, y, 0), frame.transform, frameColor);
            }
        }
        
        // 装饰性斜支撑
        CreateDiagonalSupports();
    }
    
    void CreateDiagonalSupports()
    {
        float height = wheelRadius * 2.2f;
        float width = wheelRadius * 0.4f;
        int steps = 10;
        
        // 左侧斜支撑
        for (int i = 0; i < steps; i++)
        {
            float t = i / (float)steps;
            Vector3 pos = new Vector3(
                Mathf.Lerp(-wheelRadius - width, -wheelRadius, t),
                Mathf.Lerp(0, height * 0.7f, t),
                0
            );
            CreateCube(pos, frame.transform, frameColor);
        }
        
        // 右侧斜支撑
        for (int i = 0; i < steps; i++)
        {
            float t = i / (float)steps;
            Vector3 pos = new Vector3(
                Mathf.Lerp(wheelRadius + width, wheelRadius, t),
                Mathf.Lerp(0, height * 0.7f, t),
                0
            );
            CreateCube(pos, frame.transform, frameColor);
        }
    }
    
    void CreatePillar(float x, float y, float width, float height)
    {
        // 创建主体
        for (float py = y; py < y + height; py += 1f)
        {
            for (float px = x - width/2; px <= x + width/2; px += 1f)
            {
                CreateCube(new Vector3(px, py, 0), frame.transform, frameColor);
            }
        }
        
        // 添加装饰性边框
        for (float py = y; py < y + height; py += 1f)
        {
            CreateCube(new Vector3(x - width/2 - 0.5f, py, 0), frame.transform, accentColor);
            CreateCube(new Vector3(x + width/2 + 0.5f, py, 0), frame.transform, accentColor);
        }
    }
    
    void CreateDecorations()
    {
        // 在轮圈上添加装饰灯
        int lightCount = 72;
        float angleStep = 360f / lightCount;
        
        for (int i = 0; i < lightCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(
                Mathf.Cos(angle) * (wheelRadius + 0.5f),
                Mathf.Sin(angle) * (wheelRadius + 0.5f),
                0
            );
            
            GameObject light = CreateCube(pos, decorations.transform, lightColor);
            if (light != null)
            {
                Renderer renderer = light.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material material = renderer.material;
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", lightColor * 0.5f);
                }
            }
        }
        
        // 在支架上添加装饰
        float height = wheelRadius * 2.2f;
        float spacing = height / 10f;
        
        for (float y = spacing; y < height; y += spacing)
        {
            CreateCube(new Vector3(-wheelRadius - 1f, y, 0), decorations.transform, lightColor);
            CreateCube(new Vector3(wheelRadius + 1f, y, 0), decorations.transform, lightColor);
        }
    }
    
    void CreateWheel()
    {
        // 创建外圈
        int segments = 72;
        float angleStep = 360f / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(
                Mathf.Cos(angle) * wheelRadius,
                Mathf.Sin(angle) * wheelRadius,
                0
            );
            CreateCube(pos, wheel.transform, wheelColor);
            
            // 添加金色装饰
            if (i % 6 == 0)
            {
                Vector3 decorPos = new Vector3(
                    Mathf.Cos(angle) * (wheelRadius + 0.5f),
                    Mathf.Sin(angle) * (wheelRadius + 0.5f),
                    0
                );
                CreateCube(decorPos, wheel.transform, accentColor);
            }
        }
        
        // 创建轮辐
        int spokeCount = 12;
        angleStep = 360f / spokeCount;
        
        for (int i = 0; i < spokeCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            // 主轮辐
            for (float r = 1; r < wheelRadius; r += 1f)
            {
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * r,
                    Mathf.Sin(angle) * r,
                    0
                );
                CreateCube(pos, wheel.transform, wheelColor);
            }
            
            // 装饰性副轮辐
            float subAngle1 = (angle * Mathf.Rad2Deg + 2f) * Mathf.Deg2Rad;
            float subAngle2 = (angle * Mathf.Rad2Deg - 2f) * Mathf.Deg2Rad;
            
            for (float r = wheelRadius * 0.3f; r < wheelRadius * 0.7f; r += 1f)
            {
                Vector3 pos1 = new Vector3(
                    Mathf.Cos(subAngle1) * r,
                    Mathf.Sin(subAngle1) * r,
                    0
                );
                Vector3 pos2 = new Vector3(
                    Mathf.Cos(subAngle2) * r,
                    Mathf.Sin(subAngle2) * r,
                    0
                );
                CreateCube(pos1, wheel.transform, accentColor);
                CreateCube(pos2, wheel.transform, accentColor);
            }
        }
        
        // 创建中心装饰
        CreateWheelCenter();
    }
    
    void CreateWheelCenter()
    {
        float centerSize = wheelRadius * 0.15f;
        
        // 创建中心圆盘
        for (float x = -centerSize; x <= centerSize; x += 1f)
        {
            for (float y = -centerSize; y <= centerSize; y += 1f)
            {
                if (Vector2.Distance(Vector2.zero, new Vector2(x, y)) <= centerSize)
                {
                    CreateCube(new Vector3(x, y, 0), wheel.transform, accentColor);
                }
            }
        }
        
        // 添加装饰性环形
        int decorCount = 8;
        float decorRadius = centerSize + 1f;
        float angleStep = 360f / decorCount;
        
        for (int i = 0; i < decorCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(
                Mathf.Cos(angle) * decorRadius,
                Mathf.Sin(angle) * decorRadius,
                0
            );
            CreateCube(pos, wheel.transform, wheelColor);
        }
    }
    
    void CreateCabins()
    {
        float angleStep = 360f / cabinCount;
        float cabinSize = wheelRadius * 0.15f;
        
        for (int i = 0; i < cabinCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(
                Mathf.Cos(angle) * wheelRadius,
                Mathf.Sin(angle) * wheelRadius,
                0
            );
            
            GameObject cabin = new GameObject($"Cabin_{i}");
            cabin.transform.parent = wheel.transform;
            cabin.transform.position = pos;
            
            // 创建豪华车厢
            CreateLuxuryCabin(cabin.transform, cabinSize);
            cabins.Add(cabin);
        }
    }
    
    void CreateLuxuryCabin(Transform parent, float size)
    {
        // 主体结构
        for (float x = -size; x <= size; x++)
        {
            for (float y = -size; y <= size; y++)
            {
                for (float z = -size; z <= size; z++)
                {
                    // 只创建外壳
                    if (Mathf.Abs(x) == size || 
                        Mathf.Abs(y) == size || 
                        Mathf.Abs(z) == size)
                    {
                        CreateCube(new Vector3(x, y, z), parent, cabinColor);
                    }
                }
            }
        }
        
        // 窗户
        float windowSize = size - 1;
        // 前窗
        for (float x = -windowSize; x <= windowSize; x++)
        {
            for (float y = -windowSize; y <= windowSize; y++)
            {
                CreateCube(new Vector3(x, y, size + 0.1f), parent, accentColor);
            }
        }
        
        // 后窗
        for (float x = -windowSize; x <= windowSize; x++)
        {
            for (float y = -windowSize; y <= windowSize; y++)
            {
                CreateCube(new Vector3(x, y, -size - 0.1f), parent, accentColor);
            }
        }
        
        // 装饰性边框
        for (float x = -size - 0.2f; x <= size + 0.2f; x++)
        {
            CreateCube(new Vector3(x, -size - 0.2f, 0), parent, accentColor);
            CreateCube(new Vector3(x, size + 0.2f, 0), parent, accentColor);
        }
        
        // 顶部装饰
        for (float x = -size/2; x <= size/2; x++)
        {
            CreateCube(new Vector3(x, size + 1f, 0), parent, lightColor);
        }
    }
    
    GameObject CreateCube(Vector3 position, Transform parent, Color color)
    {
        if (cubePrefab != null)
        {
            // 添加一个微小的随机偏移来避免Z-fighting
            Vector3 offset = new Vector3(
                Random.Range(-0.01f, 0.01f),
                Random.Range(-0.01f, 0.01f),
                Random.Range(-0.01f, 0.01f)
            );
            
            GameObject cube = Instantiate(cubePrefab, Vector3.zero, Quaternion.identity, parent);
            cube.transform.localPosition = position + offset;
            
            Renderer renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material material = new Material(Shader.Find("Standard"));
                material.color = color;
                
                // 调整渲染设置以减少闪烁
                material.enableInstancing = true;
                material.SetFloat("_Metallic", 0);
                material.SetFloat("_Glossiness", 0.1f);
                
                // 设置更好的阴影接收和投射
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                renderer.receiveShadows = true;
                
                renderer.material = material;
            }
            
            return cube;
        }
        return null;
    }
}
