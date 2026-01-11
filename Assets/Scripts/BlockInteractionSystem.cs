using UnityEngine;

/// <summary>
/// 方块交互系统 - 处理方块的放置和破坏
/// </summary>
public class BlockInteractionSystem : MonoBehaviour
{
    [Header("交互设置")]
    public float interactionRange = 5f;          // 交互距离
    public LayerMask blockLayer;                  // 方块层级
    public KeyCode destroyKey = KeyCode.Mouse0;   // 破坏方块按键（左键）
    public KeyCode placeKey = KeyCode.Mouse1;     // 放置方块按键（右键）
    public float destroyTime = 0.5f;              // 破坏所需时间（秒）
    
    [Header("方块设置")]
    public GameObject cubePrefab;                 // 方块预制体
    public float blockSize = 1f;                  // 方块大小
    
    [Header("视觉反馈")]
    public bool showBlockHighlight = true;        // 是否显示高亮
    public Color highlightColor = new Color(1f, 1f, 1f, 0.3f);
    
    [Header("音效")]
    public AudioClip destroySound;
    public AudioClip placeSound;
    
    private Camera playerCamera;
    private BlockHighlight blockHighlight;
    private SimpleInventory inventory;
    private AudioSource audioSource;
    
    // 当前瞄准的方块信息
    private GameObject targetBlock;
    private Vector3 targetBlockPosition;
    private Vector3 placePosition;
    private bool hasTarget;
    
    // 破坏进度
    private float destroyProgress = 0f;
    private Vector3 lastDestroyPosition;
    private bool isDestroying = false;
    
    void Start()
    {
        InitializeComponents();
    }
    
    void InitializeComponents()
    {
        // 获取玩家相机
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        
        // 获取或创建背包组件
        inventory = GetComponent<SimpleInventory>();
        if (inventory == null)
        {
            inventory = gameObject.AddComponent<SimpleInventory>();
        }
        
        // 获取方块预制体
        if (cubePrefab == null)
        {
            CubeGenerator cubeGen = FindObjectOfType<CubeGenerator>();
            if (cubeGen != null && cubeGen.cubePrefab != null)
            {
                cubePrefab = cubeGen.cubePrefab;
            }
        }
        
        // 创建高亮显示组件
        if (showBlockHighlight)
        {
            CreateBlockHighlight();
        }
        
        // 设置音频源
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f;
            audioSource.playOnAwake = false;
        }
        
        // 设置默认层级（如果未设置）
        if (blockLayer == 0)
        {
            blockLayer = ~0; // 所有层级
        }
        
        Debug.Log("BlockInteractionSystem: 方块交互系统已初始化");
    }
    
    void CreateBlockHighlight()
    {
        GameObject highlightObj = new GameObject("BlockHighlight");
        blockHighlight = highlightObj.AddComponent<BlockHighlight>();
        blockHighlight.highlightColor = highlightColor;
        blockHighlight.blockSize = blockSize;
    }
    
    void Update()
    {
        UpdateTargetBlock();
        HandleInput();
    }
    
    /// <summary>
    /// 更新当前瞄准的方块
    /// </summary>
    void UpdateTargetBlock()
    {
        if (playerCamera == null) return;
        
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, interactionRange, blockLayer))
        {
            hasTarget = true;
            targetBlock = hit.collider.gameObject;
            
            // 计算方块中心位置（对齐到网格）
            targetBlockPosition = GetBlockPosition(hit.point - hit.normal * 0.1f);
            
            // 计算放置位置（在命中面的外侧）
            placePosition = GetBlockPosition(hit.point + hit.normal * 0.5f);
            
            // 更新高亮显示
            if (blockHighlight != null)
            {
                blockHighlight.SetTarget(targetBlockPosition, true);
            }
        }
        else
        {
            hasTarget = false;
            targetBlock = null;
            
            if (blockHighlight != null)
            {
                blockHighlight.SetTarget(Vector3.zero, false);
            }
        }
    }
    
    /// <summary>
    /// 处理输入
    /// </summary>
    void HandleInput()
    {
        // 破坏方块（长按）
        if (Input.GetKey(destroyKey) && hasTarget)
        {
            // 检查是否切换了目标方块
            if (targetBlockPosition != lastDestroyPosition)
            {
                destroyProgress = 0f;
                lastDestroyPosition = targetBlockPosition;
            }
            
            isDestroying = true;
            destroyProgress += Time.deltaTime;
            
            // 更新高亮显示破坏进度
            if (blockHighlight != null)
            {
                blockHighlight.SetDestroyProgress(destroyProgress / destroyTime);
            }
            
            // 破坏完成
            if (destroyProgress >= destroyTime)
            {
                DestroyBlock();
                destroyProgress = 0f;
            }
        }
        else
        {
            // 松开按键，重置进度
            if (isDestroying)
            {
                isDestroying = false;
                destroyProgress = 0f;
                if (blockHighlight != null)
                {
                    blockHighlight.SetDestroyProgress(0f);
                }
            }
        }
        
        // 放置方块（单击）
        if (Input.GetKeyDown(placeKey) && hasTarget)
        {
            PlaceBlock();
        }
    }
    
    /// <summary>
    /// 破坏方块
    /// </summary>
    void DestroyBlock()
    {
        if (targetBlock == null) return;
        
        // 获取方块颜色用于背包
        Renderer renderer = targetBlock.GetComponent<Renderer>();
        Color blockColor = Color.white;
        if (renderer != null && renderer.material != null)
        {
            blockColor = renderer.material.color;
        }
        
        // 添加到背包
        inventory.AddBlock(blockColor);
        
        // 播放音效
        PlaySound(destroySound);
        
        // 创建破坏粒子效果
        CreateDestroyEffect(targetBlockPosition, blockColor);
        
        // 销毁方块
        Destroy(targetBlock);
        
        Debug.Log($"BlockInteractionSystem: 破坏方块于 {targetBlockPosition}");
    }
    
    /// <summary>
    /// 放置方块
    /// </summary>
    void PlaceBlock()
    {
        if (cubePrefab == null)
        {
            Debug.LogWarning("BlockInteractionSystem: 未设置方块预制体");
            return;
        }
        
        // 检查背包是否有方块
        if (!inventory.HasBlocks())
        {
            Debug.Log("BlockInteractionSystem: 背包中没有方块");
            return;
        }
        
        // 检查放置位置是否与玩家重叠
        if (IsPositionOccupiedByPlayer(placePosition))
        {
            Debug.Log("BlockInteractionSystem: 无法在玩家位置放置方块");
            return;
        }
        
        // 检查位置是否已有方块
        if (IsPositionOccupied(placePosition))
        {
            Debug.Log("BlockInteractionSystem: 该位置已有方块");
            return;
        }
        
        // 从背包取出方块
        Color blockColor = inventory.RemoveBlock();
        
        // 创建新方块
        GameObject newBlock = Instantiate(cubePrefab, placePosition, Quaternion.identity);
        newBlock.name = "PlacedBlock";
        
        // 设置颜色
        Renderer renderer = newBlock.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = blockColor;
            renderer.material = mat;
        }
        
        // 确保有碰撞体
        if (newBlock.GetComponent<Collider>() == null)
        {
            newBlock.AddComponent<BoxCollider>();
        }
        
        // 播放音效
        PlaySound(placeSound);
        
        // 创建放置粒子效果
        CreatePlaceEffect(placePosition, blockColor);
        
        Debug.Log($"BlockInteractionSystem: 放置方块于 {placePosition}");
    }

    
    /// <summary>
    /// 将世界坐标对齐到方块网格
    /// </summary>
    Vector3 GetBlockPosition(Vector3 worldPos)
    {
        return new Vector3(
            Mathf.Round(worldPos.x / blockSize) * blockSize,
            Mathf.Round(worldPos.y / blockSize) * blockSize,
            Mathf.Round(worldPos.z / blockSize) * blockSize
        );
    }
    
    /// <summary>
    /// 检查位置是否被玩家占用
    /// </summary>
    bool IsPositionOccupiedByPlayer(Vector3 position)
    {
        Vector3 playerPos = transform.position;
        float playerHeight = 2f;
        
        // 检查方块是否与玩家碰撞盒重叠
        if (Mathf.Abs(position.x - playerPos.x) < blockSize &&
            position.y >= playerPos.y - 0.5f && position.y <= playerPos.y + playerHeight &&
            Mathf.Abs(position.z - playerPos.z) < blockSize)
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 检查位置是否已有方块
    /// </summary>
    bool IsPositionOccupied(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, blockSize * 0.4f, blockLayer);
        return colliders.Length > 0;
    }
    
    /// <summary>
    /// 播放音效
    /// </summary>
    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    /// <summary>
    /// 创建破坏粒子效果
    /// </summary>
    void CreateDestroyEffect(Vector3 position, Color color)
    {
        // 创建简单的粒子效果
        GameObject effectObj = new GameObject("DestroyEffect");
        effectObj.transform.position = position;
        
        ParticleSystem ps = effectObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 3f;
        main.startSize = 0.15f;
        main.startColor = color;
        main.gravityModifier = 1f;
        main.maxParticles = 20;
        main.duration = 0.1f;
        main.loop = false;
        
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 15) });
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;
        
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.color = color;
        
        ps.Play();
        
        Destroy(effectObj, 1f);
    }
    
    /// <summary>
    /// 创建放置粒子效果
    /// </summary>
    void CreatePlaceEffect(Vector3 position, Color color)
    {
        GameObject effectObj = new GameObject("PlaceEffect");
        effectObj.transform.position = position;
        
        ParticleSystem ps = effectObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.3f;
        main.startSpeed = 1f;
        main.startSize = 0.1f;
        main.startColor = new Color(color.r, color.g, color.b, 0.5f);
        main.gravityModifier = 0f;
        main.maxParticles = 10;
        main.duration = 0.1f;
        main.loop = false;
        
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 8) });
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = Vector3.one * blockSize;
        
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.color = color;
        
        ps.Play();
        
        Destroy(effectObj, 0.5f);
    }
    
    /// <summary>
    /// 获取当前瞄准的方块位置
    /// </summary>
    public Vector3 GetTargetBlockPosition()
    {
        return targetBlockPosition;
    }
    
    /// <summary>
    /// 获取当前放置位置
    /// </summary>
    public Vector3 GetPlacePosition()
    {
        return placePosition;
    }
    
    /// <summary>
    /// 是否有瞄准目标
    /// </summary>
    public bool HasTarget()
    {
        return hasTarget;
    }
}
