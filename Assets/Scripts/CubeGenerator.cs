using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generates terrain using optimized chunk-based mesh merging
/// </summary>
public class CubeGenerator : MonoBehaviour
{
    [Header("Chunk Settings")]
    public int chunkSize = 16;
    public int renderDistance = 2;

    [Header("Terrain Settings")]
    public float noiseScale = 20f;
    public float heightScale = 10f;
    public int seed;
    public int terrainDepth = 5;

    [Header("Colors (for reference)")]
    public Color grassColor = new Color(0.4f, 0.7f, 0.2f);
    public Color dirtColor = new Color(0.6f, 0.4f, 0.2f);
    public Color stoneColor = new Color(0.5f, 0.5f, 0.5f);
    public Color waterColor = new Color(0.2f, 0.4f, 0.8f, 0.7f);
    public Color sandColor = new Color(0.9f, 0.8f, 0.5f);
    public Color treeColor = new Color(0.3f, 0.2f, 0.1f);
    public Color leafColor = new Color(0.2f, 0.5f, 0.1f);

    // Legacy field for BlockInteractionSystem compatibility
    public GameObject cubePrefab;

    // Chunk management using new renderer system
    private Dictionary<Vector2Int, ChunkRenderer> chunkRenderers = new Dictionary<Vector2Int, ChunkRenderer>();
    private Transform player;
    private Vector2Int currentChunk = new Vector2Int(0, 0);
    private Vector3 lastPlayerPosition = Vector3.zero;
    private float distanceMoved = 0;

    // WorldData reference
    private WorldData worldData;

    // Chunk update queue for gradual loading
    private Queue<Vector2Int> chunkLoadQueue = new Queue<Vector2Int>();
    private const int MAX_CHUNKS_PER_FRAME = 2;

    void Start()
    {
        // Setup random seed
        if (seed == 0)
            seed = Random.Range(1, 99999);
        Random.InitState(seed);

        // Ensure WorldData exists
        worldData = GetComponent<WorldData>();
        if (worldData == null)
        {
            worldData = gameObject.AddComponent<WorldData>();
        }

        // Delayed initialization
        Invoke("InitializeWorld", 0.5f);
    }

    void InitializeWorld()
    {
        // Get player reference
        if (GameManager.Instance != null && GameManager.Instance.playerTransform != null)
        {
            player = GameManager.Instance.playerTransform;
        }
        else
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else if (Camera.main != null)
            {
                player = Camera.main.transform;
            }
            else
            {
                Invoke("InitializeWorld", 0.5f);
                return;
            }
        }

        lastPlayerPosition = player.position;
        UpdateChunks();
    }

    void Update()
    {
        // Ensure player reference
        if (player == null)
        {
            if (GameManager.Instance != null && GameManager.Instance.playerTransform != null)
            {
                player = GameManager.Instance.playerTransform;
                lastPlayerPosition = player.position;
            }
            else
            {
                GameObject playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null)
                {
                    player = playerObj.transform;
                    lastPlayerPosition = player.position;
                }
                else if (Camera.main != null)
                {
                    player = Camera.main.transform;
                    lastPlayerPosition = player.position;
                }
                else
                {
                    return;
                }
            }
        }

        // Track player movement
        distanceMoved += Vector3.Distance(player.position, lastPlayerPosition);
        lastPlayerPosition = player.position;

        // Get current chunk
        Vector2Int newChunk = new Vector2Int(
            Mathf.FloorToInt(player.position.x / chunkSize),
            Mathf.FloorToInt(player.position.z / chunkSize)
        );

        // Respawn if fallen
        if (player.position.y < -10)
        {
            GameManager manager = GameManager.Instance;
            if (manager != null)
            {
                manager.RespawnPlayer();
            }
        }

        // Update chunks when player moves
        if (newChunk != currentChunk || distanceMoved > chunkSize / 2)
        {
            currentChunk = newChunk;
            distanceMoved = 0;
            UpdateChunks();
        }

        // Process chunk load queue
        ProcessChunkQueue();

        // Periodic update
        if (Time.frameCount % 300 == 0)
        {
            UpdateChunks();
        }
    }

    void UpdateChunks()
    {
        if (player == null) return;

        HashSet<Vector2Int> neededChunks = new HashSet<Vector2Int>();

        // Calculate needed chunks
        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int z = -renderDistance; z <= renderDistance; z++)
            {
                Vector2Int chunkPos = new Vector2Int(currentChunk.x + x, currentChunk.y + z);
                neededChunks.Add(chunkPos);

                // Queue chunk for loading if not exists
                if (!chunkRenderers.ContainsKey(chunkPos) && !chunkLoadQueue.Contains(chunkPos))
                {
                    chunkLoadQueue.Enqueue(chunkPos);
                }
            }
        }

        // Remove unneeded chunks
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        foreach (var kvp in chunkRenderers)
        {
            if (!neededChunks.Contains(kvp.Key))
            {
                chunksToRemove.Add(kvp.Key);
            }
        }

        foreach (var chunkPos in chunksToRemove)
        {
            chunkRenderers[chunkPos].Unload();
            chunkRenderers.Remove(chunkPos);
            worldData.RemoveChunk(chunkPos);
        }
    }

    void ProcessChunkQueue()
    {
        int processed = 0;
        while (chunkLoadQueue.Count > 0 && processed < MAX_CHUNKS_PER_FRAME)
        {
            Vector2Int chunkPos = chunkLoadQueue.Dequeue();

            // Skip if already loaded
            if (chunkRenderers.ContainsKey(chunkPos))
                continue;

            LoadChunk(chunkPos);
            processed++;
        }
    }

    void LoadChunk(Vector2Int chunkPos)
    {
        // Get or create chunk data
        ChunkData chunkData = worldData.GetOrCreateChunk(chunkPos);

        // Generate terrain data if empty
        if (chunkData.IsEmpty())
        {
            GenerateChunkData(chunkPos, chunkData);
        }

        // Create renderer
        GameObject chunkObj = new GameObject($"Chunk_{chunkPos.x}_{chunkPos.y}");
        chunkObj.transform.parent = transform;

        ChunkRenderer renderer = chunkObj.AddComponent<ChunkRenderer>();
        renderer.Initialize(chunkData, chunkPos);

        chunkRenderers[chunkPos] = renderer;
    }

    /// <summary>
    /// Generate terrain data for a chunk (no GameObjects created)
    /// </summary>
    void GenerateChunkData(Vector2Int chunkPos, ChunkData chunkData)
    {
        int startX = chunkPos.x * chunkSize;
        int startZ = chunkPos.y * chunkSize;

        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                int worldX = startX + x;
                int worldZ = startZ + z;

                // Generate height using Perlin noise
                float height = GenerateHeight(worldX, worldZ);
                int intHeight = Mathf.FloorToInt(height);

                // Generate terrain layers
                for (int depth = 0; depth < terrainDepth; depth++)
                {
                    int y = intHeight - depth;
                    if (y < 0 || y >= ChunkData.HEIGHT) continue;

                    BlockType blockType;
                    if (depth == 0)
                    {
                        blockType = GetTerrainBlockType(intHeight);
                    }
                    else if (depth < 3)
                    {
                        blockType = BlockType.Dirt;
                    }
                    else
                    {
                        blockType = BlockType.Stone;
                    }

                    chunkData.SetBlock(x, y, z, blockType);
                }

                // Generate water
                int waterLevel = 3;
                if (intHeight < waterLevel)
                {
                    for (int y = intHeight + 1; y <= waterLevel; y++)
                    {
                        if (y >= 0 && y < ChunkData.HEIGHT)
                        {
                            chunkData.SetBlock(x, y, z, BlockType.Water);
                        }
                    }
                }

                // Generate trees
                if (Random.value < 0.02f && intHeight > waterLevel)
                {
                    GenerateTreeData(chunkData, x, intHeight + 1, z);
                }
            }
        }
    }

    float GenerateHeight(int x, int z)
    {
        float scale = noiseScale;
        float height = 0;
        height += Mathf.PerlinNoise((x + seed) / scale, (z + seed) / scale) * heightScale;
        height += Mathf.PerlinNoise((x + seed) / (scale * 0.5f), (z + seed) / (scale * 0.5f)) * 2;
        return height;
    }

    BlockType GetTerrainBlockType(int height)
    {
        int waterLevel = 3;

        if (height < waterLevel - 1)
            return BlockType.Stone;
        else if (height < waterLevel)
            return BlockType.Sand;
        else if (height < 8)
            return BlockType.Grass;
        else if (height < 12)
            return BlockType.Dirt;
        else
            return BlockType.Stone;
    }

    void GenerateTreeData(ChunkData chunkData, int x, int baseY, int z)
    {
        int treeHeight = Random.Range(3, 6);

        // Tree trunk
        for (int y = 0; y < treeHeight; y++)
        {
            int blockY = baseY + y;
            if (blockY >= 0 && blockY < ChunkData.HEIGHT &&
                x >= 0 && x < ChunkData.SIZE && z >= 0 && z < ChunkData.SIZE)
            {
                chunkData.SetBlock(x, blockY, z, BlockType.Wood);
            }
        }

        // Tree leaves
        int leafSize = Random.Range(2, 4);
        for (int lx = -leafSize; lx <= leafSize; lx++)
        {
            for (int lz = -leafSize; lz <= leafSize; lz++)
            {
                for (int ly = 0; ly < leafSize; ly++)
                {
                    if (lx * lx + ly * ly + lz * lz <= leafSize * leafSize)
                    {
                        int leafX = x + lx;
                        int leafY = baseY + treeHeight + ly;
                        int leafZ = z + lz;

                        // Only place within chunk bounds
                        if (leafX >= 0 && leafX < ChunkData.SIZE &&
                            leafY >= 0 && leafY < ChunkData.HEIGHT &&
                            leafZ >= 0 && leafZ < ChunkData.SIZE)
                        {
                            // Don't overwrite trunk
                            if (chunkData.GetBlock(leafX, leafY, leafZ) == BlockType.Air)
                            {
                                chunkData.SetBlock(leafX, leafY, leafZ, BlockType.Leaf);
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Called when a block is changed (from BlockInteractionSystem)
    /// </summary>
    public void OnBlockChanged(Vector3Int worldPos)
    {
        var (chunkPos, localPos) = WorldData.WorldToChunk(worldPos);

        // Rebuild affected chunk
        if (chunkRenderers.TryGetValue(chunkPos, out ChunkRenderer renderer))
        {
            renderer.SetDirty();
        }

        // Check if at chunk boundary and mark neighbors dirty
        if (localPos.x == 0)
            MarkChunkDirty(chunkPos + Vector2Int.left);
        if (localPos.x == ChunkData.SIZE - 1)
            MarkChunkDirty(chunkPos + Vector2Int.right);
        if (localPos.z == 0)
            MarkChunkDirty(chunkPos + Vector2Int.down);
        if (localPos.z == ChunkData.SIZE - 1)
            MarkChunkDirty(chunkPos + Vector2Int.up);
    }

    private void MarkChunkDirty(Vector2Int chunkPos)
    {
        if (chunkRenderers.TryGetValue(chunkPos, out ChunkRenderer renderer))
        {
            renderer.SetDirty();
        }
    }

    /// <summary>
    /// Get terrain height at world position (for spawning)
    /// </summary>
    public float GetTerrainHeightAt(float x, float z)
    {
        return GenerateHeight(Mathf.FloorToInt(x), Mathf.FloorToInt(z));
    }
}
