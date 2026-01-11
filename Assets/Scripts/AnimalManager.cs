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
                new Color(0.95f, 0.95f, 0.95f), // 白色
                new Color(1f, 0.6f, 0.6f),      // 粉色内耳
                1.2f, // 缩放 - 更大
                8f,
                3f
            )
        },
        {
            Animal.AnimalType.Chicken,
            new AnimalData(
                new Color(1f, 0.85f, 0.3f),  // 黄色
                new Color(1f, 0.2f, 0.1f),   // 红色鸡冠
                1.0f, // 缩放
                2f,
                1.5f
            )
        },
        {
            Animal.AnimalType.Cat,
            new AnimalData(
                new Color(1f, 0.6f, 0.2f),   // 橘猫
                new Color(0.95f, 0.95f, 0.95f), // 白色
                1.0f, // 缩放
                6f,
                4f
            )
        },
        {
            Animal.AnimalType.Dog,
            new AnimalData(
                new Color(0.65f, 0.45f, 0.25f), // 棕色
                new Color(0.4f, 0.25f, 0.1f),  // 深棕耳朵
                1.2f, // 缩放
                5f,
                3.5f
            )
        },
        {
            Animal.AnimalType.Sheep,
            new AnimalData(
                new Color(1f, 1f, 1f),       // 白羊毛
                new Color(0.15f, 0.15f, 0.15f), // 黑脸
                1.3f, // 缩放
                3f,
                2f
            )
        },
        {
            Animal.AnimalType.Tiger,
            new AnimalData(
                new Color(1f, 0.6f, 0.1f),   // 橙色
                new Color(0.1f, 0.1f, 0.1f), // 黑条纹
                1.4f, // 缩放
                6f,
                5f
            )
        },
        {
            Animal.AnimalType.Lion,
            new AnimalData(
                new Color(0.9f, 0.7f, 0.35f), // 金色身体
                new Color(0.7f, 0.45f, 0.15f), // 棕色鬃毛
                1.4f, // 缩放
                6f,
                5f
            )
        },
        {
            Animal.AnimalType.Elephant,
            new AnimalData(
                new Color(0.55f, 0.55f, 0.6f), // 灰色
                new Color(0.7f, 0.5f, 0.5f),   // 粉色内耳
                1.5f, // 缩放 - 大象最大
                2f,
                2f
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
            case Animal.AnimalType.Tiger:
                CreateTiger(parent, data);
                break;
            case Animal.AnimalType.Lion:
                CreateLion(parent, data);
                break;
            case Animal.AnimalType.Elephant:
                CreateElephant(parent, data);
                break;
        }
    }
    GameObject CreatePart(Vector3 position, Transform parent, Color color, PrimitiveType shapeType, Vector3 scale, Vector3? rotation = null)
    {
        GameObject part = GameObject.CreatePrimitive(shapeType);
        part.transform.parent = parent;
        part.transform.localPosition = position;
        part.transform.localScale = scale;

        if (rotation.HasValue)
        {
            part.transform.localEulerAngles = rotation.Value;
        }

        // 移除碰撞体，避免物理干扰
        Collider col = part.GetComponent<Collider>();
        if (col != null) Destroy(col);

        Renderer renderer = part.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = color;
            material.SetFloat("_Metallic", 0);
            material.SetFloat("_Glossiness", 0.3f);
            renderer.material = material;
        }

        return part;
    }

    // 简化版：默认立方体
    GameObject CreateCube(Vector3 position, Transform parent, Color color)
    {
        return CreatePart(position, parent, color, PrimitiveType.Cube, Vector3.one);
    }
    
    void CreateRabbit(Transform parent, AnimalData data)
    {
        parent.localScale = Vector3.one * data.scale;

        // 圆头
        CreatePart(new Vector3(0, 0.9f, 0), parent, data.mainColor, PrimitiveType.Sphere, new Vector3(0.8f, 0.7f, 0.7f));

        // 长耳朵 - 兔子最明显特征 (胶囊形)
        CreatePart(new Vector3(-0.2f, 1.6f, 0), parent, data.mainColor, PrimitiveType.Capsule, new Vector3(0.15f, 0.5f, 0.08f));
        CreatePart(new Vector3(0.2f, 1.6f, 0), parent, data.mainColor, PrimitiveType.Capsule, new Vector3(0.15f, 0.5f, 0.08f));
        // 粉色内耳
        CreatePart(new Vector3(-0.2f, 1.6f, 0.03f), parent, data.secondaryColor, PrimitiveType.Capsule, new Vector3(0.08f, 0.4f, 0.02f));
        CreatePart(new Vector3(0.2f, 1.6f, 0.03f), parent, data.secondaryColor, PrimitiveType.Capsule, new Vector3(0.08f, 0.4f, 0.02f));

        // 眼睛
        CreatePart(new Vector3(-0.2f, 1f, 0.3f), parent, Color.black, PrimitiveType.Sphere, new Vector3(0.15f, 0.15f, 0.1f));
        CreatePart(new Vector3(0.2f, 1f, 0.3f), parent, Color.black, PrimitiveType.Sphere, new Vector3(0.15f, 0.15f, 0.1f));

        // 粉鼻子
        CreatePart(new Vector3(0, 0.85f, 0.4f), parent, data.secondaryColor, PrimitiveType.Sphere, new Vector3(0.1f, 0.08f, 0.08f));

        // 椭圆身体
        CreatePart(new Vector3(0, 0.35f, 0), parent, data.mainColor, PrimitiveType.Sphere, new Vector3(0.6f, 0.5f, 0.7f));

        // 短腿
        CreatePart(new Vector3(-0.2f, 0.1f, 0.15f), parent, data.mainColor, PrimitiveType.Sphere, new Vector3(0.15f, 0.2f, 0.15f));
        CreatePart(new Vector3(0.2f, 0.1f, 0.15f), parent, data.mainColor, PrimitiveType.Sphere, new Vector3(0.15f, 0.2f, 0.15f));
        CreatePart(new Vector3(-0.15f, 0.1f, -0.2f), parent, data.mainColor, PrimitiveType.Sphere, new Vector3(0.2f, 0.2f, 0.25f));
        CreatePart(new Vector3(0.15f, 0.1f, -0.2f), parent, data.mainColor, PrimitiveType.Sphere, new Vector3(0.2f, 0.2f, 0.25f));

        // 圆尾巴
        CreatePart(new Vector3(0, 0.4f, -0.4f), parent, Color.white, PrimitiveType.Sphere, new Vector3(0.25f, 0.25f, 0.25f));
    }

    void CreateChicken(Transform parent, AnimalData data)
    {
        parent.localScale = Vector3.one * data.scale;

        // 小圆头
        CreatePart(new Vector3(0, 0.9f, 0.1f), parent, data.mainColor, PrimitiveType.Sphere, new Vector3(0.4f, 0.45f, 0.4f));

        // 红色鸡冠 - 最明显特征
        CreatePart(new Vector3(0, 1.2f, 0.1f), parent, data.secondaryColor, PrimitiveType.Sphere, new Vector3(0.15f, 0.2f, 0.08f));
        CreatePart(new Vector3(0.08f, 1.1f, 0.1f), parent, data.secondaryColor, PrimitiveType.Sphere, new Vector3(0.12f, 0.15f, 0.06f));
        CreatePart(new Vector3(-0.08f, 1.1f, 0.1f), parent, data.secondaryColor, PrimitiveType.Sphere, new Vector3(0.12f, 0.15f, 0.06f));

        // 眼睛
        CreatePart(new Vector3(-0.12f, 0.95f, 0.28f), parent, Color.black, PrimitiveType.Sphere, new Vector3(0.08f, 0.08f, 0.05f));
        CreatePart(new Vector3(0.12f, 0.95f, 0.28f), parent, Color.black, PrimitiveType.Sphere, new Vector3(0.08f, 0.08f, 0.05f));

        // 尖喙 - 圆锥形用立方体模拟
        CreatePart(new Vector3(0, 0.85f, 0.4f), parent, new Color(1f, 0.6f, 0.2f), PrimitiveType.Cube, new Vector3(0.1f, 0.08f, 0.2f));

        // 红色肉垂
        CreatePart(new Vector3(0, 0.72f, 0.3f), parent, data.secondaryColor, PrimitiveType.Sphere, new Vector3(0.08f, 0.12f, 0.06f));

        // 椭圆身体
        CreatePart(new Vector3(0, 0.45f, 0), parent, data.mainColor, PrimitiveType.Sphere, new Vector3(0.5f, 0.55f, 0.65f));

        // 翅膀
        CreatePart(new Vector3(-0.3f, 0.5f, 0), parent, data.mainColor, PrimitiveType.Sphere, new Vector3(0.15f, 0.25f, 0.35f));
        CreatePart(new Vector3(0.3f, 0.5f, 0), parent, data.mainColor, PrimitiveType.Sphere, new Vector3(0.15f, 0.25f, 0.35f));

        // 尾巴羽毛
        CreatePart(new Vector3(0, 0.6f, -0.35f), parent, data.mainColor, PrimitiveType.Cube, new Vector3(0.08f, 0.3f, 0.15f), new Vector3(-30, 0, 0));

        // 细黄腿
        CreatePart(new Vector3(-0.12f, 0.1f, 0.05f), parent, new Color(1f, 0.7f, 0.2f), PrimitiveType.Cylinder, new Vector3(0.05f, 0.12f, 0.05f));
        CreatePart(new Vector3(0.12f, 0.1f, 0.05f), parent, new Color(1f, 0.7f, 0.2f), PrimitiveType.Cylinder, new Vector3(0.05f, 0.12f, 0.05f));
    }

    void CreateCat(Transform parent, AnimalData data)
    {
        parent.localScale = Vector3.one * data.scale;

        // 圆头
        CreatePart(new Vector3(0, 0.85f, 0.2f), parent, data.mainColor, PrimitiveType.Sphere, new Vector3(0.55f, 0.5f, 0.5f));

        // 三角尖耳朵 - 猫的特征
        CreatePart(new Vector3(-0.2f, 1.15f, 0.15f), parent, data.mainColor, PrimitiveType.Cube, new Vector3(0.15f, 0.2f, 0.08f), new Vector3(0, 0, 15));
        CreatePart(new Vector3(0.2f, 1.15f, 0.15f), parent, data.mainColor, PrimitiveType.Cube, new Vector3(0.15f, 0.2f, 0.08f), new Vector3(0, 0, -15));
        // 粉色内耳
        CreatePart(new Vector3(-0.2f, 1.12f, 0.18f), parent, new Color(1f, 0.6f, 0.6f), PrimitiveType.Cube, new Vector3(0.08f, 0.12f, 0.02f), new Vector3(0, 0, 15));
        CreatePart(new Vector3(0.2f, 1.12f, 0.18f), parent, new Color(1f, 0.6f, 0.6f), PrimitiveType.Cube, new Vector3(0.08f, 0.12f, 0.02f), new Vector3(0, 0, -15));

        // 大眼睛
        CreatePart(new Vector3(-0.15f, 0.9f, 0.42f), parent, new Color(0.2f, 0.8f, 0.2f), PrimitiveType.Sphere, new Vector3(0.14f, 0.16f, 0.08f));
        CreatePart(new Vector3(0.15f, 0.9f, 0.42f), parent, new Color(0.2f, 0.8f, 0.2f), PrimitiveType.Sphere, new Vector3(0.14f, 0.16f, 0.08f));
        // 瞳孔
        CreatePart(new Vector3(-0.15f, 0.9f, 0.46f), parent, Color.black, PrimitiveType.Sphere, new Vector3(0.05f, 0.12f, 0.02f));
        CreatePart(new Vector3(0.15f, 0.9f, 0.46f), parent, Color.black, PrimitiveType.Sphere, new Vector3(0.05f, 0.12f, 0.02f));

        // 粉鼻子
        CreatePart(new Vector3(0, 0.8f, 0.45f), parent, new Color(1f, 0.6f, 0.6f), PrimitiveType.Sphere, new Vector3(0.08f, 0.06f, 0.06f));

        // 白色嘴巴
        CreatePart(new Vector3(0, 0.73f, 0.4f), parent, data.secondaryColor, PrimitiveType.Sphere, new Vector3(0.15f, 0.1f, 0.12f));

        // 椭圆身体
        CreatePart(new Vector3(0, 0.4f, -0.15f), parent, data.mainColor, PrimitiveType.Capsule, new Vector3(0.35f, 0.3f, 0.4f), new Vector3(90, 0, 0));

        // 四条腿
        CreatePart(new Vector3(-0.15f, 0.15f, 0.1f), parent, data.mainColor, PrimitiveType.Cylinder, new Vector3(0.08f, 0.15f, 0.08f));
        CreatePart(new Vector3(0.15f, 0.15f, 0.1f), parent, data.mainColor, PrimitiveType.Cylinder, new Vector3(0.08f, 0.15f, 0.08f));
        CreatePart(new Vector3(-0.15f, 0.15f, -0.35f), parent, data.mainColor, PrimitiveType.Cylinder, new Vector3(0.08f, 0.15f, 0.08f));
        CreatePart(new Vector3(0.15f, 0.15f, -0.35f), parent, data.mainColor, PrimitiveType.Cylinder, new Vector3(0.08f, 0.15f, 0.08f));

        // 翘起的长尾巴
        CreatePart(new Vector3(0, 0.45f, -0.55f), parent, data.mainColor, PrimitiveType.Capsule, new Vector3(0.06f, 0.2f, 0.06f), new Vector3(-30, 0, 0));
        CreatePart(new Vector3(0, 0.7f, -0.65f), parent, data.mainColor, PrimitiveType.Capsule, new Vector3(0.06f, 0.15f, 0.06f), new Vector3(30, 0, 0));
    }

    void CreateDog(Transform parent, AnimalData data)
    {
        parent.localScale = Vector3.one * data.scale;

        // 圆头
        CreatePart(new Vector3(0, 0.85f, 0.15f), parent, data.mainColor, PrimitiveType.Sphere, new Vector3(0.55f, 0.5f, 0.55f));

        // 长嘴巴 - 狗的特征
        CreatePart(new Vector3(0, 0.75f, 0.45f), parent, data.mainColor, PrimitiveType.Sphere, new Vector3(0.25f, 0.2f, 0.3f));

        // 垂耳 - 狗的特征
        CreatePart(new Vector3(-0.3f, 0.75f, 0.1f), parent, data.secondaryColor, PrimitiveType.Capsule, new Vector3(0.12f, 0.2f, 0.08f));
        CreatePart(new Vector3(0.3f, 0.75f, 0.1f), parent, data.secondaryColor, PrimitiveType.Capsule, new Vector3(0.12f, 0.2f, 0.08f));

        // 眼睛
        CreatePart(new Vector3(-0.15f, 0.92f, 0.35f), parent, Color.black, PrimitiveType.Sphere, new Vector3(0.1f, 0.1f, 0.06f));
        CreatePart(new Vector3(0.15f, 0.92f, 0.35f), parent, Color.black, PrimitiveType.Sphere, new Vector3(0.1f, 0.1f, 0.06f));

        // 黑鼻子
        CreatePart(new Vector3(0, 0.8f, 0.6f), parent, Color.black, PrimitiveType.Sphere, new Vector3(0.1f, 0.08f, 0.08f));

        // 吐舌头
        CreatePart(new Vector3(0, 0.65f, 0.55f), parent, new Color(1f, 0.5f, 0.5f), PrimitiveType.Cube, new Vector3(0.08f, 0.02f, 0.15f), new Vector3(20, 0, 0));

        // 椭圆身体
        CreatePart(new Vector3(0, 0.4f, -0.2f), parent, data.mainColor, PrimitiveType.Capsule, new Vector3(0.35f, 0.35f, 0.45f), new Vector3(90, 0, 0));

        // 四条腿
        CreatePart(new Vector3(-0.18f, 0.18f, 0.1f), parent, data.mainColor, PrimitiveType.Cylinder, new Vector3(0.1f, 0.18f, 0.1f));
        CreatePart(new Vector3(0.18f, 0.18f, 0.1f), parent, data.mainColor, PrimitiveType.Cylinder, new Vector3(0.1f, 0.18f, 0.1f));
        CreatePart(new Vector3(-0.18f, 0.18f, -0.4f), parent, data.mainColor, PrimitiveType.Cylinder, new Vector3(0.1f, 0.18f, 0.1f));
        CreatePart(new Vector3(0.18f, 0.18f, -0.4f), parent, data.mainColor, PrimitiveType.Cylinder, new Vector3(0.1f, 0.18f, 0.1f));

        // 翘起的尾巴
        CreatePart(new Vector3(0, 0.55f, -0.6f), parent, data.mainColor, PrimitiveType.Capsule, new Vector3(0.08f, 0.2f, 0.08f), new Vector3(-45, 0, 0));
    }

    void CreateSheep(Transform parent, AnimalData data)
    {
        parent.localScale = Vector3.one * data.scale;

        // 蓬松羊毛头 - 多个球组成
        CreatePart(new Vector3(0, 0.95f, 0), parent, data.mainColor, PrimitiveType.Sphere, new Vector3(0.5f, 0.45f, 0.45f));
        CreatePart(new Vector3(-0.2f, 1.05f, 0), parent, data.mainColor, PrimitiveType.Sphere, new Vector3(0.25f, 0.25f, 0.25f));
        CreatePart(new Vector3(0.2f, 1.05f, 0), parent, data.mainColor, PrimitiveType.Sphere, new Vector3(0.25f, 0.25f, 0.25f));
        CreatePart(new Vector3(0, 1.15f, 0), parent, data.mainColor, PrimitiveType.Sphere, new Vector3(0.2f, 0.2f, 0.2f));

        // 黑色小脸 - 绵羊特征
        CreatePart(new Vector3(0, 0.85f, 0.3f), parent, data.secondaryColor, PrimitiveType.Sphere, new Vector3(0.25f, 0.3f, 0.2f));

        // 眼睛
        CreatePart(new Vector3(-0.1f, 0.92f, 0.38f), parent, Color.black, PrimitiveType.Sphere, new Vector3(0.06f, 0.06f, 0.04f));
        CreatePart(new Vector3(0.1f, 0.92f, 0.38f), parent, Color.black, PrimitiveType.Sphere, new Vector3(0.06f, 0.06f, 0.04f));

        // 小黑耳朵
        CreatePart(new Vector3(-0.32f, 0.9f, 0.1f), parent, data.secondaryColor, PrimitiveType.Sphere, new Vector3(0.12f, 0.08f, 0.06f));
        CreatePart(new Vector3(0.32f, 0.9f, 0.1f), parent, data.secondaryColor, PrimitiveType.Sphere, new Vector3(0.12f, 0.08f, 0.06f));

        // 蓬松身体 - 多个球组成
        CreatePart(new Vector3(0, 0.45f, -0.1f), parent, data.mainColor, PrimitiveType.Sphere, new Vector3(0.6f, 0.5f, 0.7f));
        CreatePart(new Vector3(-0.25f, 0.5f, -0.1f), parent, data.mainColor, PrimitiveType.Sphere, new Vector3(0.3f, 0.3f, 0.35f));
        CreatePart(new Vector3(0.25f, 0.5f, -0.1f), parent, data.mainColor, PrimitiveType.Sphere, new Vector3(0.3f, 0.3f, 0.35f));
        CreatePart(new Vector3(0, 0.6f, -0.1f), parent, data.mainColor, PrimitiveType.Sphere, new Vector3(0.35f, 0.25f, 0.4f));

        // 黑色细腿
        CreatePart(new Vector3(-0.2f, 0.12f, 0.15f), parent, data.secondaryColor, PrimitiveType.Cylinder, new Vector3(0.06f, 0.12f, 0.06f));
        CreatePart(new Vector3(0.2f, 0.12f, 0.15f), parent, data.secondaryColor, PrimitiveType.Cylinder, new Vector3(0.06f, 0.12f, 0.06f));
        CreatePart(new Vector3(-0.2f, 0.12f, -0.3f), parent, data.secondaryColor, PrimitiveType.Cylinder, new Vector3(0.06f, 0.12f, 0.06f));
        CreatePart(new Vector3(0.2f, 0.12f, -0.3f), parent, data.secondaryColor, PrimitiveType.Cylinder, new Vector3(0.06f, 0.12f, 0.06f));

        // 小尾巴
        CreatePart(new Vector3(0, 0.45f, -0.45f), parent, data.mainColor, PrimitiveType.Sphere, new Vector3(0.15f, 0.12f, 0.1f));
    }

    void CreateTiger(Transform parent, AnimalData data)
    {
        parent.localScale = Vector3.one * data.scale;
        Color orange = data.mainColor;
        Color black = data.secondaryColor;
        Color white = new Color(1f, 0.95f, 0.9f);

        // 大圆头
        CreatePart(new Vector3(0, 0.9f, 0.1f), parent, orange, PrimitiveType.Sphere, new Vector3(0.6f, 0.55f, 0.55f));

        // 脸部条纹 - 老虎最明显特征
        CreatePart(new Vector3(0, 1.05f, 0.1f), parent, black, PrimitiveType.Cube, new Vector3(0.08f, 0.15f, 0.4f));
        CreatePart(new Vector3(-0.2f, 0.95f, 0.25f), parent, black, PrimitiveType.Cube, new Vector3(0.04f, 0.2f, 0.15f), new Vector3(0, 0, -20));
        CreatePart(new Vector3(0.2f, 0.95f, 0.25f), parent, black, PrimitiveType.Cube, new Vector3(0.04f, 0.2f, 0.15f), new Vector3(0, 0, 20));

        // 圆耳朵
        CreatePart(new Vector3(-0.25f, 1.15f, 0.05f), parent, orange, PrimitiveType.Sphere, new Vector3(0.15f, 0.15f, 0.08f));
        CreatePart(new Vector3(0.25f, 1.15f, 0.05f), parent, orange, PrimitiveType.Sphere, new Vector3(0.15f, 0.15f, 0.08f));
        // 白色内耳
        CreatePart(new Vector3(-0.25f, 1.13f, 0.08f), parent, white, PrimitiveType.Sphere, new Vector3(0.08f, 0.08f, 0.04f));
        CreatePart(new Vector3(0.25f, 1.13f, 0.08f), parent, white, PrimitiveType.Sphere, new Vector3(0.08f, 0.08f, 0.04f));

        // 眼睛
        CreatePart(new Vector3(-0.15f, 0.95f, 0.38f), parent, new Color(1f, 0.8f, 0.2f), PrimitiveType.Sphere, new Vector3(0.12f, 0.1f, 0.06f));
        CreatePart(new Vector3(0.15f, 0.95f, 0.38f), parent, new Color(1f, 0.8f, 0.2f), PrimitiveType.Sphere, new Vector3(0.12f, 0.1f, 0.06f));
        // 瞳孔
        CreatePart(new Vector3(-0.15f, 0.95f, 0.41f), parent, Color.black, PrimitiveType.Sphere, new Vector3(0.05f, 0.08f, 0.02f));
        CreatePart(new Vector3(0.15f, 0.95f, 0.41f), parent, Color.black, PrimitiveType.Sphere, new Vector3(0.05f, 0.08f, 0.02f));

        // 白色嘴部
        CreatePart(new Vector3(0, 0.78f, 0.35f), parent, white, PrimitiveType.Sphere, new Vector3(0.2f, 0.15f, 0.18f));

        // 粉鼻子
        CreatePart(new Vector3(0, 0.85f, 0.45f), parent, new Color(1f, 0.6f, 0.6f), PrimitiveType.Sphere, new Vector3(0.08f, 0.06f, 0.06f));

        // 椭圆身体带条纹
        CreatePart(new Vector3(0, 0.45f, -0.2f), parent, orange, PrimitiveType.Capsule, new Vector3(0.4f, 0.35f, 0.5f), new Vector3(90, 0, 0));
        // 身体条纹
        CreatePart(new Vector3(-0.22f, 0.5f, -0.15f), parent, black, PrimitiveType.Cube, new Vector3(0.03f, 0.2f, 0.15f));
        CreatePart(new Vector3(0.22f, 0.5f, -0.15f), parent, black, PrimitiveType.Cube, new Vector3(0.03f, 0.2f, 0.15f));
        CreatePart(new Vector3(0, 0.55f, -0.25f), parent, black, PrimitiveType.Cube, new Vector3(0.03f, 0.15f, 0.2f));

        // 四条腿
        CreatePart(new Vector3(-0.2f, 0.18f, 0.1f), parent, orange, PrimitiveType.Cylinder, new Vector3(0.1f, 0.18f, 0.1f));
        CreatePart(new Vector3(0.2f, 0.18f, 0.1f), parent, orange, PrimitiveType.Cylinder, new Vector3(0.1f, 0.18f, 0.1f));
        CreatePart(new Vector3(-0.2f, 0.18f, -0.4f), parent, orange, PrimitiveType.Cylinder, new Vector3(0.1f, 0.18f, 0.1f));
        CreatePart(new Vector3(0.2f, 0.18f, -0.4f), parent, orange, PrimitiveType.Cylinder, new Vector3(0.1f, 0.18f, 0.1f));

        // 长尾巴带条纹
        CreatePart(new Vector3(0, 0.5f, -0.6f), parent, orange, PrimitiveType.Capsule, new Vector3(0.08f, 0.25f, 0.08f), new Vector3(-50, 0, 0));
        // 尾巴条纹
        CreatePart(new Vector3(0, 0.6f, -0.7f), parent, black, PrimitiveType.Cylinder, new Vector3(0.09f, 0.03f, 0.09f));
        CreatePart(new Vector3(0, 0.75f, -0.6f), parent, black, PrimitiveType.Cylinder, new Vector3(0.09f, 0.03f, 0.09f));
    }

    void CreateLion(Transform parent, AnimalData data)
    {
        parent.localScale = Vector3.one * data.scale;
        Color body = data.mainColor;
        Color mane = data.secondaryColor;

        // 大蓬松鬃毛 - 狮子最明显的特征（围绕脸一圈）
        CreatePart(new Vector3(0, 1.05f, -0.05f), parent, mane, PrimitiveType.Sphere, new Vector3(0.9f, 0.8f, 0.7f));
        // 额外鬃毛球
        CreatePart(new Vector3(-0.35f, 1f, 0), parent, mane, PrimitiveType.Sphere, new Vector3(0.3f, 0.35f, 0.25f));
        CreatePart(new Vector3(0.35f, 1f, 0), parent, mane, PrimitiveType.Sphere, new Vector3(0.3f, 0.35f, 0.25f));
        CreatePart(new Vector3(0, 1.25f, 0), parent, mane, PrimitiveType.Sphere, new Vector3(0.35f, 0.25f, 0.25f));
        CreatePart(new Vector3(-0.25f, 0.75f, 0.1f), parent, mane, PrimitiveType.Sphere, new Vector3(0.25f, 0.25f, 0.2f));
        CreatePart(new Vector3(0.25f, 0.75f, 0.1f), parent, mane, PrimitiveType.Sphere, new Vector3(0.25f, 0.25f, 0.2f));

        // 金色脸
        CreatePart(new Vector3(0, 0.95f, 0.2f), parent, body, PrimitiveType.Sphere, new Vector3(0.45f, 0.4f, 0.4f));

        // 小圆耳朵
        CreatePart(new Vector3(-0.3f, 1.2f, -0.05f), parent, body, PrimitiveType.Sphere, new Vector3(0.12f, 0.12f, 0.06f));
        CreatePart(new Vector3(0.3f, 1.2f, -0.05f), parent, body, PrimitiveType.Sphere, new Vector3(0.12f, 0.12f, 0.06f));

        // 眼睛
        CreatePart(new Vector3(-0.12f, 1f, 0.4f), parent, new Color(0.9f, 0.7f, 0.2f), PrimitiveType.Sphere, new Vector3(0.1f, 0.08f, 0.05f));
        CreatePart(new Vector3(0.12f, 1f, 0.4f), parent, new Color(0.9f, 0.7f, 0.2f), PrimitiveType.Sphere, new Vector3(0.1f, 0.08f, 0.05f));
        // 瞳孔
        CreatePart(new Vector3(-0.12f, 1f, 0.43f), parent, Color.black, PrimitiveType.Sphere, new Vector3(0.04f, 0.06f, 0.02f));
        CreatePart(new Vector3(0.12f, 1f, 0.43f), parent, Color.black, PrimitiveType.Sphere, new Vector3(0.04f, 0.06f, 0.02f));

        // 嘴巴区域
        CreatePart(new Vector3(0, 0.85f, 0.4f), parent, new Color(1f, 0.95f, 0.9f), PrimitiveType.Sphere, new Vector3(0.18f, 0.12f, 0.15f));

        // 棕色鼻子
        CreatePart(new Vector3(0, 0.9f, 0.48f), parent, new Color(0.3f, 0.2f, 0.1f), PrimitiveType.Sphere, new Vector3(0.08f, 0.06f, 0.06f));

        // 椭圆身体
        CreatePart(new Vector3(0, 0.45f, -0.25f), parent, body, PrimitiveType.Capsule, new Vector3(0.4f, 0.35f, 0.5f), new Vector3(90, 0, 0));

        // 四条腿
        CreatePart(new Vector3(-0.18f, 0.18f, 0.05f), parent, body, PrimitiveType.Cylinder, new Vector3(0.1f, 0.18f, 0.1f));
        CreatePart(new Vector3(0.18f, 0.18f, 0.05f), parent, body, PrimitiveType.Cylinder, new Vector3(0.1f, 0.18f, 0.1f));
        CreatePart(new Vector3(-0.18f, 0.18f, -0.45f), parent, body, PrimitiveType.Cylinder, new Vector3(0.1f, 0.18f, 0.1f));
        CreatePart(new Vector3(0.18f, 0.18f, -0.45f), parent, body, PrimitiveType.Cylinder, new Vector3(0.1f, 0.18f, 0.1f));

        // 尾巴带毛球
        CreatePart(new Vector3(0, 0.5f, -0.65f), parent, body, PrimitiveType.Capsule, new Vector3(0.06f, 0.25f, 0.06f), new Vector3(-50, 0, 0));
        // 毛球
        CreatePart(new Vector3(0, 0.72f, -0.78f), parent, mane, PrimitiveType.Sphere, new Vector3(0.15f, 0.15f, 0.12f));
    }

    void CreateElephant(Transform parent, AnimalData data)
    {
        parent.localScale = Vector3.one * data.scale;
        Color gray = data.mainColor;
        Color pink = data.secondaryColor;

        // 大圆头
        CreatePart(new Vector3(0, 1f, 0.1f), parent, gray, PrimitiveType.Sphere, new Vector3(0.7f, 0.65f, 0.6f));

        // 超大扇形耳朵 - 大象最明显的特征
        // 左耳
        CreatePart(new Vector3(-0.5f, 0.95f, 0.05f), parent, gray, PrimitiveType.Sphere, new Vector3(0.45f, 0.55f, 0.08f));
        // 粉色内耳
        CreatePart(new Vector3(-0.5f, 0.95f, 0.1f), parent, pink, PrimitiveType.Sphere, new Vector3(0.3f, 0.4f, 0.04f));
        // 右耳
        CreatePart(new Vector3(0.5f, 0.95f, 0.05f), parent, gray, PrimitiveType.Sphere, new Vector3(0.45f, 0.55f, 0.08f));
        // 粉色内耳
        CreatePart(new Vector3(0.5f, 0.95f, 0.1f), parent, pink, PrimitiveType.Sphere, new Vector3(0.3f, 0.4f, 0.04f));

        // 眼睛
        CreatePart(new Vector3(-0.2f, 1.05f, 0.35f), parent, Color.black, PrimitiveType.Sphere, new Vector3(0.08f, 0.08f, 0.05f));
        CreatePart(new Vector3(0.2f, 1.05f, 0.35f), parent, Color.black, PrimitiveType.Sphere, new Vector3(0.08f, 0.08f, 0.05f));

        // 长鼻子 - 大象第二明显特征 (用多个圆柱连接)
        CreatePart(new Vector3(0, 0.85f, 0.45f), parent, gray, PrimitiveType.Sphere, new Vector3(0.2f, 0.2f, 0.2f));
        CreatePart(new Vector3(0, 0.65f, 0.55f), parent, gray, PrimitiveType.Capsule, new Vector3(0.12f, 0.15f, 0.12f), new Vector3(20, 0, 0));
        CreatePart(new Vector3(0, 0.4f, 0.6f), parent, gray, PrimitiveType.Capsule, new Vector3(0.1f, 0.15f, 0.1f), new Vector3(10, 0, 0));
        CreatePart(new Vector3(0, 0.15f, 0.55f), parent, gray, PrimitiveType.Capsule, new Vector3(0.09f, 0.12f, 0.09f), new Vector3(-20, 0, 0));
        // 鼻子末端
        CreatePart(new Vector3(0, 0.08f, 0.4f), parent, gray, PrimitiveType.Sphere, new Vector3(0.1f, 0.08f, 0.12f));

        // 椭圆身体
        CreatePart(new Vector3(0, 0.5f, -0.25f), parent, gray, PrimitiveType.Sphere, new Vector3(0.55f, 0.5f, 0.7f));

        // 粗腿
        CreatePart(new Vector3(-0.22f, 0.2f, 0.1f), parent, gray, PrimitiveType.Cylinder, new Vector3(0.12f, 0.2f, 0.12f));
        CreatePart(new Vector3(0.22f, 0.2f, 0.1f), parent, gray, PrimitiveType.Cylinder, new Vector3(0.12f, 0.2f, 0.12f));
        CreatePart(new Vector3(-0.22f, 0.2f, -0.45f), parent, gray, PrimitiveType.Cylinder, new Vector3(0.12f, 0.2f, 0.12f));
        CreatePart(new Vector3(0.22f, 0.2f, -0.45f), parent, gray, PrimitiveType.Cylinder, new Vector3(0.12f, 0.2f, 0.12f));

        // 小尾巴
        CreatePart(new Vector3(0, 0.55f, -0.6f), parent, gray, PrimitiveType.Capsule, new Vector3(0.04f, 0.15f, 0.04f), new Vector3(-30, 0, 0));
        // 尾巴末端毛
        CreatePart(new Vector3(0, 0.38f, -0.68f), parent, new Color(0.4f, 0.4f, 0.45f), PrimitiveType.Sphere, new Vector3(0.06f, 0.08f, 0.04f));
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
