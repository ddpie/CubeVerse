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
    public Color cabinColor = new Color(0.9f, 0.95f, 1f, 0.1f);   // 非常透明的白色车厢
    public Color lightColor = new Color(1f, 0.9f, 0.5f);    // 暖黄色灯光
    
    private List<GameObject> cabins = new List<GameObject>();
    private List<Quaternion> cabinOriginalRotations = new List<Quaternion>();
    private GameObject wheel;
    private GameObject frame;
    private GameObject decorations;
    private float timeOffset;
    
    void Start()
    {
        // 设置摩天轮的位置，让圆心在地面上方60个单位
        transform.position = new Vector3(20f, 60f, 20f);
        
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
        
        // 保持车厢水平
        KeepCabinsLevel();
        
        UpdateLights();
    }
    
    void UpdateLights()
    {
        if (decorations == null) return;

        float time = Time.time + timeOffset;
        float pulse = Mathf.PingPong(time * 0.5f, 1f);
        Color currentLight = Color.Lerp(lightColor * 0.8f, lightColor * 1.2f, pulse);
        
        foreach (Renderer renderer in decorations.GetComponentsInChildren<Renderer>())
        {
            if (renderer != null && renderer.material != null)
            {
                // 创建新材质实例避免共享材质问题
                if (renderer.material.name.Contains("(Instance)") == false)
                {
                    renderer.material = new Material(renderer.material);
                }
                renderer.material.color = currentLight;
                // 完全禁用发光效果以避免颜色抖动
                renderer.material.DisableKeyword("_EMISSION");
                renderer.material.SetColor("_EmissionColor", Color.black);
            }
        }
    }
    
    void CreateFerrisWheel()
    {
        Debug.Log("开始创建摩天轮...");
        
        // 创建父物体并设置位置
        GameObject ferrisWheel = new GameObject("FerrisWheel");
        ferrisWheel.transform.SetParent(transform, false); // 使用false保持世界坐标
        ferrisWheel.transform.localPosition = Vector3.zero;
        
        // 不再创建支架，直接创建装饰
        decorations = new GameObject("Decorations");
        decorations.transform.SetParent(ferrisWheel.transform, false);
        decorations.transform.localPosition = Vector3.zero;
        CreateDecorations();
        
        // 创建轮圈和轮辐
        wheel = new GameObject("Wheel");
        wheel.transform.SetParent(ferrisWheel.transform, false);
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
        
        // 去掉支架上的装饰，因为不再有支架
    }
    
    void CreateWheel()
    {
        // 创建外圈，使用更好的Z层次管理
        int segments = 72;
        float angleStep = 360f / segments;
        float baseZ = -1f;  // 轮圈放在最后面，避免穿过车厢
        
        // 先创建蓝色主体（放在最后面）
        for (int i = 0; i < segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(
                Mathf.Cos(angle) * wheelRadius,
                Mathf.Sin(angle) * wheelRadius,
                baseZ
            );
            CreateCube(pos, wheel.transform, wheelColor);
        }
        
        // 再创建金色装饰（放在中间层）
        for (int i = 0; i < segments; i++)
        {
            if (i % 6 == 0)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * (wheelRadius + 0.5f),
                    Mathf.Sin(angle) * (wheelRadius + 0.5f),
                    baseZ + 0.2f  // 金色装饰放在中间层
                );
                CreateCube(pos, wheel.transform, accentColor);
            }
        }
        
        // 创建轮辐，分层处理
        int spokeCount = 12;
        angleStep = 360f / spokeCount;
        
        // 先创建蓝色主轮辐（放在最后面）
        for (int i = 0; i < spokeCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            for (float r = 1; r < wheelRadius; r += 1f)
            {
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * r,
                    Mathf.Sin(angle) * r,
                    baseZ - 0.2f  // 主轮辐放在最后面
                );
                CreateCube(pos, wheel.transform, wheelColor);
            }
        }
        
        // 再创建金色副轮辐（放在中间层）
        for (int i = 0; i < spokeCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float subAngle1 = (angle * Mathf.Rad2Deg + 2f) * Mathf.Deg2Rad;
            float subAngle2 = (angle * Mathf.Rad2Deg - 2f) * Mathf.Deg2Rad;
            
            for (float r = wheelRadius * 0.3f; r < wheelRadius * 0.7f; r += 1f)
            {
                // 第一条副轮辐
                Vector3 pos1 = new Vector3(
                    Mathf.Cos(subAngle1) * r,
                    Mathf.Sin(subAngle1) * r,
                    baseZ + 0.1f  // 副轮辐放在中间层
                );
                CreateCube(pos1, wheel.transform, accentColor);
                
                // 第二条副轮辐
                Vector3 pos2 = new Vector3(
                    Mathf.Cos(subAngle2) * r,
                    Mathf.Sin(subAngle2) * r,
                    baseZ + 0.1f  // 副轮辐放在中间层
                );
                CreateCube(pos2, wheel.transform, accentColor);
            }
        }
        
        // 创建中心装饰
        CreateWheelCenter();
    }
    
    void CreateWheelCenter()
    {
        float centerSize = wheelRadius * 0.15f;
        float baseZ = -0.8f;  // 中心装饰放在后面，避免穿过车厢
        
        // 创建中心圆盘，从后到前分层
        for (int layer = 0; layer < 3; layer++)
        {
            float currentZ = baseZ + (layer * 0.1f);  // 每层都往前一点
            for (float x = -centerSize; x <= centerSize; x += 1f)
            {
                for (float y = -centerSize; y <= centerSize; y += 1f)
                {
                    float distanceFromCenter = Vector2.Distance(Vector2.zero, new Vector2(x, y));
                    if (distanceFromCenter <= centerSize - layer)
                    {
                        Vector3 pos = new Vector3(x, y, currentZ);
                        CreateCube(pos, wheel.transform, accentColor);
                    }
                }
            }
        }
        
        // 添加装饰性环形，放在中间层
        int decorCount = 8;
        float decorRadius = centerSize + 1f;
        float angleStep = 360f / decorCount;
        float decorZ = baseZ + 0.4f;  // 装饰环放在中间层
        
        for (int i = 0; i < decorCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(
                Mathf.Cos(angle) * decorRadius,
                Mathf.Sin(angle) * decorRadius,
                decorZ
            );
            CreateCube(pos, wheel.transform, wheelColor);
        }
    }
    
    void CreateCabins()
    {
        float angleStep = 360f / cabinCount;
        float cabinSize = wheelRadius * 0.25f; // 车厢尺寸
        float cabinOffset = cabinSize + 2f; // 车厢偏移距离，确保不与轮圈重叠
        
        for (int i = 0; i < cabinCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            
            // 车厢挂载在轮圈外侧，避免穿过轮圈
            Vector3 cabinPos = new Vector3(
                Mathf.Cos(angle) * (wheelRadius + cabinOffset),
                Mathf.Sin(angle) * (wheelRadius + cabinOffset),
                2f  // 车厢放在轮圈前面，避免被遮挡
            );
            
            GameObject cabin = new GameObject($"Cabin_{i}");
            cabin.transform.SetParent(wheel.transform, false);
            cabin.transform.localPosition = cabinPos;
            
            // 记录车厢的初始旋转（保持水平）
            cabinOriginalRotations.Add(Quaternion.identity);
            
            // 创建车厢
            CreateLuxuryCabin(cabin.transform, cabinSize);
            cabins.Add(cabin);
        }
    }
    
    void KeepCabinsLevel()
    {
        // 保持所有车厢水平
        for (int i = 0; i < cabins.Count; i++)
        {
            if (cabins[i] != null)
            {
                // 让车厢保持世界坐标的水平状态
                cabins[i].transform.rotation = Quaternion.identity;
            }
        }
    }
    
    void CreateLuxuryCabin(Transform parent, float size)
    {
        // 创建轿厢的地板 - 不添加单独的碰撞体，稍后统一添加
        for (float x = -size; x <= size; x++)
        {
            for (float z = -size; z <= size; z++)
            {
                GameObject floorCube = CreateCube(new Vector3(x, -size, z), parent, cabinColor);
                // 不给每个地板方块添加碰撞体，避免冲突
            }
        }
        
        // 创建四周的围栏，高度为0.5个单位
        float railingHeight = 0.5f;
        
        // 前后围栏
        for (float x = -size; x <= size; x++)
        {
            for (float y = -size + 1; y <= -size + railingHeight; y += 0.5f)
            {
                CreateCube(new Vector3(x, y, size), parent, accentColor);    // 前围栏
                CreateCube(new Vector3(x, y, -size), parent, accentColor);   // 后围栏
            }
        }
        
        // 左右围栏
        for (float z = -size; z <= size; z++)
        {
            for (float y = -size + 1; y <= -size + railingHeight; y += 0.5f)
            {
                CreateCube(new Vector3(size, y, z), parent, accentColor);    // 右围栏
                CreateCube(new Vector3(-size, y, z), parent, accentColor);   // 左围栏
            }
        }
        
        // 创建轿厢的外壳，但要留出门洞，使用自然高度
        for (float x = -size; x <= size; x++)
        {
            for (float y = -size + 1; y <= size; y++) // 恢复自然高度
            {
                for (float z = -size; z <= size; z++)
                {
                    // 只创建外壳
                    if (Mathf.Abs(x) == size || 
                        Mathf.Abs(y) == size || 
                        Mathf.Abs(z) == size)
                    {
                        // 检查是否应该创建这个方块
                        bool shouldCreate = true;
                        
                        // 检查门洞 - 四个方向的门，门洞更大更宽
                        // 前门 (z = size)
                        if (Mathf.Abs(z) == size && z > 0)
                        {
                            if (Mathf.Abs(x) <= 3.5f && y >= -size + railingHeight + 0.5f && y <= size - 0.5f)
                            {
                                shouldCreate = false; // 门洞
                            }
                        }
                        
                        // 后门 (z = -size)
                        if (Mathf.Abs(z) == size && z < 0)
                        {
                            if (Mathf.Abs(x) <= 3.5f && y >= -size + railingHeight + 0.5f && y <= size - 0.5f)
                            {
                                shouldCreate = false; // 门洞
                            }
                        }
                        
                        // 右门 (x = size)
                        if (Mathf.Abs(x) == size && x > 0)
                        {
                            if (Mathf.Abs(z) <= 3.5f && y >= -size + railingHeight + 0.5f && y <= size - 0.5f)
                            {
                                shouldCreate = false; // 门洞
                            }
                        }
                        
                        // 左门 (x = -size)
                        if (Mathf.Abs(x) == size && x < 0)
                        {
                            if (Mathf.Abs(z) <= 3.5f && y >= -size + railingHeight + 0.5f && y <= size - 0.5f)
                            {
                                shouldCreate = false; // 门洞
                            }
                        }
                        
                        if (shouldCreate)
                        {
                            GameObject wallCube = CreateCube(new Vector3(x, y, z), parent, cabinColor);
                            // 墙壁也不添加单独碰撞体
                        }
                    }
                }
            }
        }
        
        // 为整个车厢添加一个统一的物理碰撞体
        CreateCabinPhysics(parent, size);
        
        // 创建装饰性窗框
        CreateWindowFrames(parent, size);
        
        // 顶部装饰
        for (float x = -size/2; x <= size/2; x++)
        {
            Vector3 pos = new Vector3(x, size + 1f, 0);
            CreateCube(pos, parent, lightColor);
        }
    }
    
    void CreateCabinPhysics(Transform parent, float size)
    {
        // 创建一个不可见的物理对象来处理车厢碰撞
        GameObject physicsObject = new GameObject("CabinPhysics");
        physicsObject.transform.SetParent(parent, false);
        physicsObject.transform.localPosition = Vector3.zero;
        
        // 添加地板碰撞体 - 一个大的平面
        GameObject floorCollider = new GameObject("FloorCollider");
        floorCollider.transform.SetParent(physicsObject.transform, false);
        floorCollider.transform.localPosition = new Vector3(0, -size + 0.5f, 0); // 地板上方一点
        
        BoxCollider floorBox = floorCollider.AddComponent<BoxCollider>();
        floorBox.size = new Vector3(size * 2f, 1f, size * 2f); // 覆盖整个地板
        
        // 添加墙壁碰撞体 - 四面墙
        // 前墙
        GameObject frontWall = new GameObject("FrontWall");
        frontWall.transform.SetParent(physicsObject.transform, false);
        frontWall.transform.localPosition = new Vector3(0, 0, size);
        BoxCollider frontBox = frontWall.AddComponent<BoxCollider>();
        frontBox.size = new Vector3(size * 2f, size * 2f, 0.5f);
        
        // 后墙
        GameObject backWall = new GameObject("BackWall");
        backWall.transform.SetParent(physicsObject.transform, false);
        backWall.transform.localPosition = new Vector3(0, 0, -size);
        BoxCollider backBox = backWall.AddComponent<BoxCollider>();
        backBox.size = new Vector3(size * 2f, size * 2f, 0.5f);
        
        // 左墙
        GameObject leftWall = new GameObject("LeftWall");
        leftWall.transform.SetParent(physicsObject.transform, false);
        leftWall.transform.localPosition = new Vector3(-size, 0, 0);
        BoxCollider leftBox = leftWall.AddComponent<BoxCollider>();
        leftBox.size = new Vector3(0.5f, size * 2f, size * 2f);
        
        // 右墙
        GameObject rightWall = new GameObject("RightWall");
        rightWall.transform.SetParent(physicsObject.transform, false);
        rightWall.transform.localPosition = new Vector3(size, 0, 0);
        BoxCollider rightBox = rightWall.AddComponent<BoxCollider>();
        rightBox.size = new Vector3(0.5f, size * 2f, size * 2f);
        
        Debug.Log($"为车厢创建了统一的物理碰撞体，地板大小: {floorBox.size}");
    }
    
    void CreateWindowFrames(Transform parent, float size)
    {
        // 为每个门创建装饰性边框，使用稳定的材质避免颜色抖动，宽度与门洞匹配
        
        // 前门边框
        for (float x = -4f; x <= 4f; x += 8f) // 左右边框，调整位置适应更宽的窗户
        {
            for (float y = -size + 1f; y <= size - 0.5f; y++) // 恢复自然高度
            {
                Vector3 pos = new Vector3(x, y, size + 0.2f); // 增加更多偏移
                CreateStableCube(pos, parent, accentColor, "WindowFrame");
            }
        }
        for (float x = -3.5f; x <= 3.5f; x++) // 上下边框，宽度匹配门洞
        {
            Vector3 topPos = new Vector3(x, size - 0.5f, size + 0.2f); // 恢复自然高度
            Vector3 bottomPos = new Vector3(x, -size + 1f, size + 0.2f);
            CreateStableCube(topPos, parent, accentColor, "WindowFrame");
            CreateStableCube(bottomPos, parent, accentColor, "WindowFrame");
        }
        
        // 后门边框
        for (float x = -4f; x <= 4f; x += 8f)
        {
            for (float y = -size + 1f; y <= size - 0.5f; y++) // 恢复自然高度
            {
                Vector3 pos = new Vector3(x, y, -size - 0.2f); // 增加更多偏移
                CreateStableCube(pos, parent, accentColor, "WindowFrame");
            }
        }
        for (float x = -3.5f; x <= 3.5f; x++)
        {
            Vector3 topPos = new Vector3(x, size - 0.5f, -size - 0.2f); // 恢复自然高度
            Vector3 bottomPos = new Vector3(x, -size + 1f, -size - 0.2f);
            CreateStableCube(topPos, parent, accentColor, "WindowFrame");
            CreateStableCube(bottomPos, parent, accentColor, "WindowFrame");
        }
        
        // 右门边框
        for (float z = -4f; z <= 4f; z += 8f)
        {
            for (float y = -size + 1f; y <= size - 0.5f; y++) // 恢复自然高度
            {
                Vector3 pos = new Vector3(size + 0.2f, y, z); // 增加更多偏移
                CreateStableCube(pos, parent, accentColor, "WindowFrame");
            }
        }
        for (float z = -3.5f; z <= 3.5f; z++)
        {
            Vector3 topPos = new Vector3(size + 0.2f, size - 0.5f, z); // 恢复自然高度
            Vector3 bottomPos = new Vector3(size + 0.2f, -size + 1f, z);
            CreateStableCube(topPos, parent, accentColor, "WindowFrame");
            CreateStableCube(bottomPos, parent, accentColor, "WindowFrame");
        }
        
        // 左门边框
        for (float z = -4f; z <= 4f; z += 8f)
        {
            for (float y = -size + 1f; y <= size - 0.5f; y++) // 恢复自然高度
            {
                Vector3 pos = new Vector3(-size - 0.2f, y, z); // 增加更多偏移
                CreateStableCube(pos, parent, accentColor, "WindowFrame");
            }
        }
        for (float z = -3.5f; z <= 3.5f; z++)
        {
            Vector3 topPos = new Vector3(-size - 0.2f, size - 0.5f, z); // 恢复自然高度
            Vector3 bottomPos = new Vector3(-size - 0.2f, -size + 1f, z);
            CreateStableCube(topPos, parent, accentColor, "WindowFrame");
            CreateStableCube(bottomPos, parent, accentColor, "WindowFrame");
        }
    }
    
    void EnsureCollider(GameObject obj)
    {
        // 确保对象有碰撞体来支撑玩家
        if (obj.GetComponent<Collider>() == null)
        {
            obj.AddComponent<BoxCollider>();
        }
    }
    
    void RemoveCollider(GameObject obj)
    {
        // 移除碰撞体
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null)
        {
            DestroyImmediate(collider);
        }
    }
    
    GameObject CreateCube(Vector3 position, Transform parent, Color color)
    {
        if (cubePrefab != null)
        {
            GameObject cube = Instantiate(cubePrefab, Vector3.zero, Quaternion.identity, parent);
            cube.transform.localPosition = position;
            
            // 默认移除碰撞体，只有车厢需要碰撞体
            RemoveCollider(cube);
            
            Renderer renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material material = new Material(Shader.Find("Standard"));
                
                // 如果颜色有透明度，设置透明渲染模式
                if (color.a < 1f)
                {
                    // 设置为透明模式
                    material.SetFloat("_Mode", 3);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = 3000;
                    
                    // 设置透明颜色
                    material.color = color;
                    
                    Debug.Log($"创建透明材质，透明度: {color.a}");
                }
                else
                {
                    material.color = color;
                }
                
                // 禁用发光效果
                material.DisableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.black);
                material.SetFloat("_Metallic", 0f);
                material.SetFloat("_Glossiness", 0.3f);
                
                material.name = $"FerrisWheelMaterial_{System.Guid.NewGuid()}";
                renderer.material = material;
            }
            
            return cube;
        }
        return null;
    }
    
    GameObject CreateStableCube(Vector3 position, Transform parent, Color color, string materialType)
    {
        if (cubePrefab != null)
        {
            GameObject cube = Instantiate(cubePrefab, Vector3.zero, Quaternion.identity, parent);
            cube.transform.localPosition = position;
            
            Renderer renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material material;
                
                // 如果颜色有透明度，使用透明材质
                if (color.a < 1f)
                {
                    material = new Material(Shader.Find("Standard"));
                    material.SetFloat("_Mode", 3); // 设置为透明模式
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = 3000;
                }
                else
                {
                    // 创建完全独立的材质实例，专门用于避免颜色抖动
                    material = new Material(Shader.Find("Unlit/Color"));
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                }
                
                material.color = color;
                
                // 使用Unlit着色器完全避免光照计算导致的颜色变化
                material.name = $"Stable_{materialType}_{Time.time}_{UnityEngine.Random.Range(0, 10000)}";
                
                renderer.material = material;
            }
            
            return cube;
        }
        return null;
    }
}
