using UnityEngine;
using System.Collections.Generic;

public class AnimalManager : MonoBehaviour
{
    [Header("生成设置")]
    public GameObject cubePrefab;
    public int minAnimals = 25;
    public int maxAnimals = 35;
    public float spawnRadius = 40f;
    public float updateInterval = 1f;
    
    [Header("动物设置")]
    public float minScale = 0.3f;
    public float maxScale = 0.8f;
    
    private List<GameObject> animals = new List<GameObject>();
    private Transform playerTransform;
    private float updateTimer;
    private Dictionary<GameObject, Material[]> materialCache = new Dictionary<GameObject, Material[]>();
    
    private Dictionary<Animal.AnimalType, AnimalData> animalData = new Dictionary<Animal.AnimalType, AnimalData>()
    {
        {
            Animal.AnimalType.Rabbit,
            new AnimalData(
                new Color(0.9f, 0.9f, 0.9f), // 白色
                new Color(0.8f, 0.8f, 0.8f), // 浅灰
                0.4f, // 缩放
                8f,   // 跳跃力
                3f    // 移动速度
            )
        },
        {
            Animal.AnimalType.Chicken,
            new AnimalData(
                new Color(1f, 0.8f, 0.2f),   // 黄色
                new Color(1f, 0.3f, 0.1f),   // 红色
                0.3f, // 缩放
                2f,   // 跳跃力
                1.5f  // 移动速度
            )
        },
        {
            Animal.AnimalType.Cat,
            new AnimalData(
                new Color(0.8f, 0.8f, 0.8f), // 灰色
                new Color(0.6f, 0.6f, 0.6f), // 深灰
                0.4f, // 缩放
                6f,   // 跳跃力
                4f    // 移动速度
            )
        },
        {
            Animal.AnimalType.Dog,
            new AnimalData(
                new Color(0.6f, 0.4f, 0.2f), // 棕色
                new Color(0.4f, 0.3f, 0.1f), // 深棕
                0.5f, // 缩放
                5f,   // 跳跃力
                3.5f  // 移动速度
            )
        },
        {
            Animal.AnimalType.Sheep,
            new AnimalData(
                new Color(1f, 1f, 1f),       // 白色
                new Color(0.9f, 0.9f, 0.9f), // 浅灰
                0.6f, // 缩放
                3f,   // 跳跃力
                2f    // 移动速度
            )
        }
    };
    
    void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerTransform != null)
        {
            playerTransform = GameManager.Instance.playerTransform;
        }
        
        if (cubePrefab == null)
        {
            CubeGenerator cubeGen = FindObjectOfType<CubeGenerator>();
            if (cubeGen != null)
            {
                cubePrefab = cubeGen.cubePrefab;
            }
        }
        
        SpawnInitialAnimals();
    }
    
    void Update()
    {
        if (playerTransform == null)
        {
            if (GameManager.Instance != null && GameManager.Instance.playerTransform != null)
            {
                playerTransform = GameManager.Instance.playerTransform;
            }
            else
            {
                return;
            }
        }
        
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            UpdateAnimals();
        }
    }
    
    void SpawnInitialAnimals()
    {
        int count = Random.Range(minAnimals, maxAnimals + 1);
        for (int i = 0; i < count; i++)
        {
            SpawnAnimal();
        }
    }
    
    void UpdateAnimals()
    {
        for (int i = animals.Count - 1; i >= 0; i--)
        {
            if (animals[i] == null) continue;
            
            float distance = Vector3.Distance(
                new Vector3(playerTransform.position.x, 0, playerTransform.position.z),
                new Vector3(animals[i].transform.position.x, 0, animals[i].transform.position.z)
            );
            
            if (distance > spawnRadius * 1.5f)
            {
                // 清理材质缓存
                if (materialCache.ContainsKey(animals[i]))
                {
                    materialCache.Remove(animals[i]);
                }
                Destroy(animals[i]);
                animals.RemoveAt(i);
            }
        }
        
        while (animals.Count < minAnimals)
        {
            SpawnAnimal();
        }
    }
    
    void SpawnAnimal()
    {
        if (cubePrefab == null || playerTransform == null) return;
        
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = playerTransform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
        
        RaycastHit hit;
        if (Physics.Raycast(spawnPos + Vector3.up * 50f, Vector3.down, out hit, 100f))
        {
            spawnPos.y = hit.point.y;
        }
        else
        {
            return;
        }
        
        GameObject animal = new GameObject("Animal");
        animal.transform.position = spawnPos;
        
        Animal.AnimalType type = (Animal.AnimalType)Random.Range(0, System.Enum.GetValues(typeof(Animal.AnimalType)).Length);
        AnimalData data = animalData[type];
        
        Animal animalComponent = animal.AddComponent<Animal>();
        animalComponent.animalType = type;
        animalComponent.mainColor = data.mainColor;
        animalComponent.secondaryColor = data.secondaryColor;
        animalComponent.moveSpeed = data.moveSpeed;
        animalComponent.jumpForce = data.jumpForce;
        
        CreateAnimalModel(animal.transform, type, data);
        
        animals.Add(animal);
    }
    
    void CreateAnimalModel(Transform parent, Animal.AnimalType type, AnimalData data)
    {
        switch (type)
        {
            case Animal.AnimalType.Rabbit:
                CreateRabbit(parent, data);
                break;
            case Animal.AnimalType.Chicken:
                CreateChicken(parent, data);
                break;
            case Animal.AnimalType.Cat:
                CreateCat(parent, data);
                break;
            case Animal.AnimalType.Dog:
                CreateDog(parent, data);
                break;
            case Animal.AnimalType.Sheep:
                CreateSheep(parent, data);
                break;
        }
    }
    
    GameObject CreateCube(Vector3 position, Transform parent, Color color)
    {
        if (cubePrefab != null)
        {
            // 添加微小的随机偏移来避免Z-fighting
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
                
                // 优化渲染设置
                material.enableInstancing = true;
                material.SetFloat("_Metallic", 0);
                material.SetFloat("_Glossiness", 0.1f);
                
                // 设置更好的阴影
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                renderer.receiveShadows = true;
                
                renderer.material = material;
                
                // 缓存材质
                if (!materialCache.ContainsKey(parent.gameObject))
                {
                    materialCache[parent.gameObject] = new Material[1];
                }
                materialCache[parent.gameObject][0] = material;
            }
            
            return cube;
        }
        return null;
    }
    
    void CreateRabbit(Transform parent, AnimalData data)
    {
        parent.localScale = Vector3.one * data.scale;
        
        // 身体（更圆润的形状）
        for (float z = -0.5f; z <= 0.5f; z += 0.5f)
        {
            for (float x = -0.5f; x <= 0.5f; x += 0.5f)
            {
                CreateCube(new Vector3(x, 0.5f, z), parent, data.mainColor);
            }
        }
        
        // 头部（更精细的细节）
        CreateCube(new Vector3(0, 1f, 0.7f), parent, data.mainColor);
        CreateCube(new Vector3(-0.2f, 1f, 0.7f), parent, data.mainColor);
        CreateCube(new Vector3(0.2f, 1f, 0.7f), parent, data.mainColor);
        
        // 眼睛
        CreateCube(new Vector3(-0.3f, 1.1f, 1f), parent, Color.black);
        CreateCube(new Vector3(0.3f, 1.1f, 1f), parent, Color.black);
        
        // 鼻子
        CreateCube(new Vector3(0, 0.9f, 1.1f), parent, new Color(1f, 0.8f, 0.8f));
        
        // 长耳朵
        for (float y = 0; y <= 1f; y += 0.5f)
        {
            CreateCube(new Vector3(-0.2f, 1.5f + y, 0.7f), parent, data.mainColor);
            CreateCube(new Vector3(0.2f, 1.5f + y, 0.7f), parent, data.mainColor);
        }
        
        // 内耳
        CreateCube(new Vector3(-0.2f, 2f, 0.8f), parent, new Color(1f, 0.8f, 0.8f));
        CreateCube(new Vector3(0.2f, 2f, 0.8f), parent, new Color(1f, 0.8f, 0.8f));
        
        // 后腿（更强壮）
        CreateCube(new Vector3(-0.3f, 0.2f, 0), parent, data.mainColor);
        CreateCube(new Vector3(0.3f, 0.2f, 0), parent, data.mainColor);
        CreateCube(new Vector3(-0.3f, 0, 0.2f), parent, data.mainColor);
        CreateCube(new Vector3(0.3f, 0, 0.2f), parent, data.mainColor);
        
        // 前腿
        CreateCube(new Vector3(-0.3f, 0.2f, 0.6f), parent, data.mainColor);
        CreateCube(new Vector3(0.3f, 0.2f, 0.6f), parent, data.mainColor);
        
        // 蓬松的尾巴
        CreateCube(new Vector3(0, 0.5f, -0.4f), parent, Color.white);
        CreateCube(new Vector3(0.2f, 0.5f, -0.4f), parent, Color.white);
        CreateCube(new Vector3(-0.2f, 0.5f, -0.4f), parent, Color.white);
    }

    void CreateChicken(Transform parent, AnimalData data)
    {
        parent.localScale = Vector3.one * data.scale;
        
        // 身体（圆润的形状）
        for (float x = -0.3f; x <= 0.3f; x += 0.3f)
        {
            for (float z = -0.3f; z <= 0.3f; z += 0.3f)
            {
                CreateCube(new Vector3(x, 0.3f, z), parent, data.mainColor);
            }
        }
        
        // 头部
        CreateCube(new Vector3(0, 0.6f, 0.3f), parent, data.mainColor);
        
        // 鸡冠
        for (float x = -0.2f; x <= 0.2f; x += 0.2f)
        {
            CreateCube(new Vector3(x, 0.8f, 0.3f), parent, data.secondaryColor);
        }
        
        // 眼睛
        CreateCube(new Vector3(-0.15f, 0.6f, 0.5f), parent, Color.black);
        CreateCube(new Vector3(0.15f, 0.6f, 0.5f), parent, Color.black);
        
        // 喙
        CreateCube(new Vector3(0, 0.5f, 0.6f), parent, new Color(1f, 0.6f, 0));
        
        // 翅膀
        for (float y = 0.2f; y <= 0.4f; y += 0.2f)
        {
            CreateCube(new Vector3(-0.4f, y, 0), parent, data.mainColor);
            CreateCube(new Vector3(0.4f, y, 0), parent, data.mainColor);
        }
        
        // 尾羽
        for (float x = -0.2f; x <= 0.2f; x += 0.2f)
        {
            CreateCube(new Vector3(x, 0.4f, -0.4f), parent, data.mainColor);
        }
        
        // 腿
        CreateCube(new Vector3(-0.2f, 0, 0), parent, data.secondaryColor);
        CreateCube(new Vector3(0.2f, 0, 0), parent, data.secondaryColor);
    }

    void CreateCat(Transform parent, AnimalData data)
    {
        parent.localScale = Vector3.one * data.scale;
        
        // 身体
        for (float z = -0.7f; z <= 0.7f; z += 0.5f)
        {
            for (float x = -0.4f; x <= 0.4f; x += 0.4f)
            {
                CreateCube(new Vector3(x, 0.5f, z), parent, data.mainColor);
            }
        }
        
        // 头部
        CreateCube(new Vector3(0, 1f, 1f), parent, data.mainColor);
        CreateCube(new Vector3(-0.2f, 1f, 1f), parent, data.mainColor);
        CreateCube(new Vector3(0.2f, 1f, 1f), parent, data.mainColor);
        
        // 眼睛（发光的猫眼）
        CreateCube(new Vector3(-0.2f, 1.1f, 1.3f), parent, new Color(0.3f, 0.8f, 0.3f));
        CreateCube(new Vector3(0.2f, 1.1f, 1.3f), parent, new Color(0.3f, 0.8f, 0.3f));
        
        // 鼻子
        CreateCube(new Vector3(0, 0.9f, 1.4f), parent, new Color(1f, 0.8f, 0.8f));
        
        // 耳朵（三角形）
        CreateCube(new Vector3(-0.3f, 1.5f, 1f), parent, data.mainColor);
        CreateCube(new Vector3(0.3f, 1.5f, 1f), parent, data.mainColor);
        CreateCube(new Vector3(-0.3f, 1.7f, 1f), parent, data.mainColor);
        CreateCube(new Vector3(0.3f, 1.7f, 1f), parent, data.mainColor);
        
        // 内耳
        CreateCube(new Vector3(-0.3f, 1.5f, 1.1f), parent, new Color(1f, 0.8f, 0.8f));
        CreateCube(new Vector3(0.3f, 1.5f, 1.1f), parent, new Color(1f, 0.8f, 0.8f));
        
        // 腿
        CreateCube(new Vector3(-0.3f, 0, -0.3f), parent, data.mainColor);
        CreateCube(new Vector3(0.3f, 0, -0.3f), parent, data.mainColor);
        CreateCube(new Vector3(-0.3f, 0, 0.3f), parent, data.mainColor);
        CreateCube(new Vector3(0.3f, 0, 0.3f), parent, data.mainColor);
        
        // 优雅的尾巴（弧形）
        float tailLength = 1.2f;
        int tailSegments = 4;
        for (int i = 0; i < tailSegments; i++)
        {
            float t = i / (float)(tailSegments - 1);
            float angle = Mathf.Lerp(0, Mathf.PI * 0.5f, t);
            Vector3 pos = new Vector3(
                0,
                0.5f + Mathf.Sin(angle) * 0.5f,
                -0.7f - Mathf.Cos(angle) * tailLength
            );
            CreateCube(pos, parent, data.mainColor);
        }
    }

    void CreateDog(Transform parent, AnimalData data)
    {
        parent.localScale = Vector3.one * data.scale;
        
        // 身体
        for (float z = -0.8f; z <= 0.8f; z += 0.4f)
        {
            for (float x = -0.4f; x <= 0.4f; x += 0.4f)
            {
                CreateCube(new Vector3(x, 0.6f, z), parent, data.mainColor);
            }
        }
        
        // 头部
        for (float x = -0.4f; x <= 0.4f; x += 0.4f)
        {
            for (float z = 0.8f; z <= 1.2f; z += 0.4f)
            {
                CreateCube(new Vector3(x, 1f, z), parent, data.mainColor);
            }
        }
        
        // 吻部
        CreateCube(new Vector3(0, 0.8f, 1.4f), parent, data.mainColor);
        CreateCube(new Vector3(0, 0.6f, 1.4f), parent, data.secondaryColor);
        
        // 眼睛
        CreateCube(new Vector3(-0.3f, 1.1f, 1.3f), parent, Color.black);
        CreateCube(new Vector3(0.3f, 1.1f, 1.3f), parent, Color.black);
        
        // 鼻子
        CreateCube(new Vector3(0, 0.8f, 1.6f), parent, Color.black);
        
        // 耳朵（下垂）
        for (float y = 1.4f; y >= 1f; y -= 0.2f)
        {
            CreateCube(new Vector3(-0.4f, y, 1f), parent, data.mainColor);
            CreateCube(new Vector3(0.4f, y, 1f), parent, data.mainColor);
        }
        
        // 腿
        float legHeight = 0.8f;
        for (float y = 0; y < legHeight; y += 0.2f)
        {
            CreateCube(new Vector3(-0.3f, y, -0.6f), parent, data.mainColor);
            CreateCube(new Vector3(0.3f, y, -0.6f), parent, data.mainColor);
            CreateCube(new Vector3(-0.3f, y, 0.6f), parent, data.mainColor);
            CreateCube(new Vector3(0.3f, y, 0.6f), parent, data.mainColor);
        }
        
        // 摇摆的尾巴
        float tailLength = 0.8f;
        int tailSegments = 4;
        for (int i = 0; i < tailSegments; i++)
        {
            float t = i / (float)(tailSegments - 1);
            Vector3 pos = new Vector3(
                Mathf.Sin(t * Mathf.PI * 0.5f) * 0.3f,
                0.8f + t * 0.4f,
                -0.8f - t * tailLength
            );
            CreateCube(pos, parent, data.mainColor);
        }
    }

    void CreateSheep(Transform parent, AnimalData data)
    {
        parent.localScale = Vector3.one * data.scale;
        
        // 蓬松的身体
        for (float x = -0.6f; x <= 0.6f; x += 0.3f)
        {
            for (float y = 0.3f; y <= 0.9f; y += 0.3f)
            {
                for (float z = -0.6f; z <= 0.6f; z += 0.3f)
                {
                    if (Random.value < 0.8f) // 随机创建蓬松效果
                    {
                        CreateCube(new Vector3(x, y, z), parent, Color.white);
                    }
                }
            }
        }
        
        // 头
        CreateCube(new Vector3(0, 0.9f, 0.9f), parent, data.secondaryColor);
        CreateCube(new Vector3(-0.2f, 0.9f, 0.9f), parent, data.secondaryColor);
        CreateCube(new Vector3(0.2f, 0.9f, 0.9f), parent, data.secondaryColor);
        
        // 眼睛
        CreateCube(new Vector3(-0.3f, 1f, 1.1f), parent, Color.black);
        CreateCube(new Vector3(0.3f, 1f, 1.1f), parent, Color.black);
        
        // 耳朵
        CreateCube(new Vector3(-0.4f, 1.1f, 0.9f), parent, data.secondaryColor);
        CreateCube(new Vector3(0.4f, 1.1f, 0.9f), parent, data.secondaryColor);
        
        // 腿
        CreateCube(new Vector3(-0.3f, 0, -0.3f), parent, data.secondaryColor);
        CreateCube(new Vector3(0.3f, 0, -0.3f), parent, data.secondaryColor);
        CreateCube(new Vector3(-0.3f, 0, 0.3f), parent, data.secondaryColor);
        CreateCube(new Vector3(0.3f, 0, 0.3f), parent, data.secondaryColor);
    }
    
    void OnDestroy()
    {
        // 清理材质缓存
        foreach (var materials in materialCache.Values)
        {
            foreach (var material in materials)
            {
                if (material != null)
                {
                    Destroy(material);
                }
            }
        }
        materialCache.Clear();
    }
}
