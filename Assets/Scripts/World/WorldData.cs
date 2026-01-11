using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages all chunk data for the world
/// Provides world coordinate to chunk coordinate conversion
/// </summary>
public class WorldData : MonoBehaviour
{
    public static WorldData Instance { get; private set; }

    private Dictionary<Vector2Int, ChunkData> chunks = new Dictionary<Vector2Int, ChunkData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Get existing chunk data or create new one
    /// </summary>
    public ChunkData GetOrCreateChunk(Vector2Int chunkPos)
    {
        if (!chunks.TryGetValue(chunkPos, out ChunkData data))
        {
            data = new ChunkData();
            chunks[chunkPos] = data;
        }
        return data;
    }

    /// <summary>
    /// Get chunk data if it exists
    /// </summary>
    public ChunkData GetChunk(Vector2Int chunkPos)
    {
        chunks.TryGetValue(chunkPos, out ChunkData data);
        return data;
    }

    /// <summary>
    /// Check if chunk exists
    /// </summary>
    public bool HasChunk(Vector2Int chunkPos)
    {
        return chunks.ContainsKey(chunkPos);
    }

    /// <summary>
    /// Remove chunk data (for unloading)
    /// </summary>
    public void RemoveChunk(Vector2Int chunkPos)
    {
        chunks.Remove(chunkPos);
    }

    /// <summary>
    /// Convert world position to chunk position and local position
    /// </summary>
    public static (Vector2Int chunkPos, Vector3Int localPos) WorldToChunk(Vector3Int worldPos)
    {
        Vector2Int chunkPos = new Vector2Int(
            Mathf.FloorToInt(worldPos.x / (float)ChunkData.SIZE),
            Mathf.FloorToInt(worldPos.z / (float)ChunkData.SIZE)
        );

        // Handle negative coordinates correctly
        int localX = ((worldPos.x % ChunkData.SIZE) + ChunkData.SIZE) % ChunkData.SIZE;
        int localZ = ((worldPos.z % ChunkData.SIZE) + ChunkData.SIZE) % ChunkData.SIZE;

        Vector3Int localPos = new Vector3Int(localX, worldPos.y, localZ);
        return (chunkPos, localPos);
    }

    /// <summary>
    /// Convert world position to chunk position
    /// </summary>
    public static Vector2Int WorldToChunkPos(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x / ChunkData.SIZE),
            Mathf.FloorToInt(worldPos.z / ChunkData.SIZE)
        );
    }

    /// <summary>
    /// Convert chunk position and local position to world position
    /// </summary>
    public static Vector3Int ChunkToWorld(Vector2Int chunkPos, Vector3Int localPos)
    {
        return new Vector3Int(
            chunkPos.x * ChunkData.SIZE + localPos.x,
            localPos.y,
            chunkPos.y * ChunkData.SIZE + localPos.z
        );
    }

    /// <summary>
    /// Get block at world coordinates
    /// </summary>
    public BlockType GetBlock(Vector3Int worldPos)
    {
        var (chunkPos, localPos) = WorldToChunk(worldPos);
        ChunkData chunk = GetChunk(chunkPos);
        if (chunk == null)
            return BlockType.Air;
        return chunk.GetBlock(localPos.x, localPos.y, localPos.z);
    }

    /// <summary>
    /// Set block at world coordinates
    /// </summary>
    public void SetBlock(Vector3Int worldPos, BlockType type)
    {
        var (chunkPos, localPos) = WorldToChunk(worldPos);
        ChunkData chunk = GetOrCreateChunk(chunkPos);
        chunk.SetBlock(localPos.x, localPos.y, localPos.z, type);
    }

    /// <summary>
    /// Check if block at world position is solid
    /// </summary>
    public bool IsBlockSolid(Vector3Int worldPos)
    {
        return BlockTypeConfig.IsSolid(GetBlock(worldPos));
    }

    /// <summary>
    /// Get all loaded chunk positions
    /// </summary>
    public IEnumerable<Vector2Int> GetLoadedChunks()
    {
        return chunks.Keys;
    }
}
