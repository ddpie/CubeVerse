using UnityEngine;

/// <summary>
/// Block interaction system - handles block placement and destruction
/// Uses optimized math-based raycast instead of physics raycast
/// </summary>
public class BlockInteractionSystem : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRange = 5f;
    public KeyCode destroyKey = KeyCode.Mouse0;
    public KeyCode placeKey = KeyCode.Mouse1;
    public float destroyTime = 0.5f;

    [Header("Visual Feedback")]
    public bool showBlockHighlight = true;
    public Color highlightColor = new Color(1f, 1f, 1f, 0.3f);

    [Header("Audio")]
    public AudioClip destroySound;
    public AudioClip placeSound;

    private Camera playerCamera;
    private BlockHighlight blockHighlight;
    private SimpleInventory inventory;
    private AudioSource audioSource;
    private CubeGenerator cubeGenerator;

    // Current target info
    private Vector3Int targetBlockPos;
    private Vector3 targetNormal;
    private bool hasTarget;

    // Destroy progress
    private float destroyProgress = 0f;
    private Vector3Int lastDestroyPos;
    private bool isDestroying = false;

    void Start()
    {
        InitializeComponents();
    }

    void InitializeComponents()
    {
        // Get player camera
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        // Get or create inventory
        inventory = GetComponent<SimpleInventory>();
        if (inventory == null)
        {
            inventory = gameObject.AddComponent<SimpleInventory>();
        }

        // Get CubeGenerator reference
        cubeGenerator = FindObjectOfType<CubeGenerator>();

        // Create highlight
        if (showBlockHighlight)
        {
            CreateBlockHighlight();
        }

        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f;
            audioSource.playOnAwake = false;
        }

        Debug.Log("BlockInteractionSystem: Initialized with optimized raycast");
    }

    void CreateBlockHighlight()
    {
        GameObject highlightObj = new GameObject("BlockHighlight");
        blockHighlight = highlightObj.AddComponent<BlockHighlight>();
        blockHighlight.highlightColor = highlightColor;
        blockHighlight.blockSize = 1f;
    }

    void Update()
    {
        UpdateTargetBlock();
        HandleInput();
    }

    /// <summary>
    /// Update current target block using raycast
    /// </summary>
    void UpdateTargetBlock()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        // Try math raycast first
        hasTarget = ChunkRaycast.Raycast(ray, interactionRange, out targetBlockPos, out targetNormal);

        // Fallback to physics raycast if math raycast fails
        if (!hasTarget)
        {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, interactionRange))
            {
                hasTarget = true;
                // Calculate block position from hit point
                Vector3 blockCenter = hit.point - hit.normal * 0.5f;
                targetBlockPos = new Vector3Int(
                    Mathf.FloorToInt(blockCenter.x),
                    Mathf.FloorToInt(blockCenter.y),
                    Mathf.FloorToInt(blockCenter.z)
                );
                targetNormal = hit.normal;
            }
        }

        // Update highlight
        if (blockHighlight != null)
        {
            if (hasTarget)
            {
                blockHighlight.SetTarget(targetBlockPos, true);
            }
            else
            {
                blockHighlight.SetTarget(Vector3.zero, false);
            }
        }
    }

    /// <summary>
    /// Handle input for block interaction
    /// </summary>
    void HandleInput()
    {
        // Destroy block (hold)
        if (Input.GetKey(destroyKey) && hasTarget)
        {
            // Check if target changed
            if (targetBlockPos != lastDestroyPos)
            {
                destroyProgress = 0f;
                lastDestroyPos = targetBlockPos;
            }

            isDestroying = true;
            destroyProgress += Time.deltaTime;

            // Update highlight progress
            if (blockHighlight != null)
            {
                blockHighlight.SetDestroyProgress(destroyProgress / destroyTime);
            }

            // Destroy complete
            if (destroyProgress >= destroyTime)
            {
                DestroyBlock();
                destroyProgress = 0f;
            }
        }
        else
        {
            // Reset progress when released
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

        // Place block (click)
        if (Input.GetKeyDown(placeKey) && hasTarget)
        {
            PlaceBlock();
        }
    }

    /// <summary>
    /// Destroy the target block
    /// </summary>
    void DestroyBlock()
    {
        // Get block type for inventory
        BlockType blockType = BlockType.Dirt; // Default
        Color blockColor = BlockTypeConfig.GetColor(BlockType.Dirt);

        if (WorldData.Instance != null)
        {
            blockType = WorldData.Instance.GetBlock(targetBlockPos);
            if (blockType != BlockType.Air)
            {
                blockColor = BlockTypeConfig.GetColor(blockType);
            }
            // Update world data
            WorldData.Instance.SetBlock(targetBlockPos, BlockType.Air);
        }

        // Add to inventory
        inventory.AddBlock(blockColor);

        // Play sound
        PlaySound(destroySound);

        // Create effect
        CreateDestroyEffect(targetBlockPos, blockColor);

        // Notify chunk system to rebuild mesh
        if (cubeGenerator != null)
        {
            cubeGenerator.OnBlockChanged(targetBlockPos);
        }

        Debug.Log($"BlockInteractionSystem: Destroyed block at {targetBlockPos}");
    }

    /// <summary>
    /// Place a block
    /// </summary>
    void PlaceBlock()
    {
        // Check inventory
        if (!inventory.HasBlocks())
        {
            Debug.Log("BlockInteractionSystem: No blocks in inventory");
            return;
        }

        // Calculate place position
        Vector3Int placePos = ChunkRaycast.GetPlacePosition(targetBlockPos, targetNormal);

        // Check if position is occupied by player
        if (IsPositionOccupiedByPlayer(placePos))
        {
            Debug.Log("BlockInteractionSystem: Cannot place block at player position");
            return;
        }

        // Check if position is already occupied (using physics)
        Vector3 checkPos = new Vector3(placePos.x + 0.5f, placePos.y + 0.5f, placePos.z + 0.5f);
        if (Physics.CheckBox(checkPos, Vector3.one * 0.4f))
        {
            Debug.Log("BlockInteractionSystem: Position already occupied");
            return;
        }

        // Get block from inventory
        Color blockColor = inventory.RemoveBlock();
        BlockType blockType = BlockTypeConfig.GetBlockTypeFromColor(blockColor);

        // Update world data
        if (WorldData.Instance != null)
        {
            WorldData.Instance.SetBlock(placePos, blockType);
        }

        // Notify chunk system to rebuild mesh
        if (cubeGenerator != null)
        {
            cubeGenerator.OnBlockChanged(placePos);
        }

        // Play sound
        PlaySound(placeSound);

        // Create effect
        CreatePlaceEffect(placePos, blockColor);

        Debug.Log($"BlockInteractionSystem: Placed block at {placePos}");
    }

    /// <summary>
    /// Check if position is occupied by player
    /// </summary>
    bool IsPositionOccupiedByPlayer(Vector3Int position)
    {
        Vector3 playerPos = transform.position;
        float playerHeight = 2f;

        return Mathf.Abs(position.x - playerPos.x) < 1f &&
               position.y >= playerPos.y - 0.5f && position.y <= playerPos.y + playerHeight &&
               Mathf.Abs(position.z - playerPos.z) < 1f;
    }

    /// <summary>
    /// Play sound effect
    /// </summary>
    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Create destroy particle effect
    /// </summary>
    void CreateDestroyEffect(Vector3Int position, Color color)
    {
        GameObject effectObj = new GameObject("DestroyEffect");
        effectObj.transform.position = new Vector3(position.x + 0.5f, position.y + 0.5f, position.z + 0.5f);

        ParticleSystem ps = effectObj.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 0.1f;
        main.loop = false;
        main.startLifetime = 0.5f;
        main.startSpeed = 3f;
        main.startSize = 0.15f;
        main.startColor = color;
        main.gravityModifier = 1f;
        main.maxParticles = 20;

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
    /// Create place particle effect
    /// </summary>
    void CreatePlaceEffect(Vector3Int position, Color color)
    {
        GameObject effectObj = new GameObject("PlaceEffect");
        effectObj.transform.position = new Vector3(position.x + 0.5f, position.y + 0.5f, position.z + 0.5f);

        ParticleSystem ps = effectObj.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 0.1f;
        main.loop = false;
        main.startLifetime = 0.3f;
        main.startSpeed = 1f;
        main.startSize = 0.1f;
        main.startColor = new Color(color.r, color.g, color.b, 0.5f);
        main.gravityModifier = 0f;
        main.maxParticles = 10;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 8) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = Vector3.one;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.color = color;

        ps.Play();

        Destroy(effectObj, 0.5f);
    }

    /// <summary>
    /// Get current target block position
    /// </summary>
    public Vector3 GetTargetBlockPosition()
    {
        return new Vector3(targetBlockPos.x, targetBlockPos.y, targetBlockPos.z);
    }

    /// <summary>
    /// Get current place position
    /// </summary>
    public Vector3 GetPlacePosition()
    {
        Vector3Int placePos = ChunkRaycast.GetPlacePosition(targetBlockPos, targetNormal);
        return new Vector3(placePos.x, placePos.y, placePos.z);
    }

    /// <summary>
    /// Check if has target
    /// </summary>
    public bool HasTarget()
    {
        return hasTarget;
    }
}
