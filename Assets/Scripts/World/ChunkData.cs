using UnityEngine;

/// <summary>
/// Stores block data for a single chunk using a 3D byte array
/// Memory efficient: 16 * 64 * 16 = 16,384 bytes per chunk
/// </summary>
public class ChunkData
{
    public const int SIZE = 16;      // Chunk width and depth
    public const int HEIGHT = 64;    // Chunk height (supports tall worlds)

    // 3D array storing block types as bytes
    private byte[,,] blocks;

    // Track if chunk has been modified (for save/load)
    public bool IsDirty { get; private set; }

    public ChunkData()
    {
        blocks = new byte[SIZE, HEIGHT, SIZE];
        IsDirty = false;
    }

    /// <summary>
    /// Get block type at local coordinates
    /// Returns Air for out-of-bounds coordinates
    /// </summary>
    public BlockType GetBlock(int x, int y, int z)
    {
        if (x < 0 || x >= SIZE || y < 0 || y >= HEIGHT || z < 0 || z >= SIZE)
            return BlockType.Air;
        return (BlockType)blocks[x, y, z];
    }

    /// <summary>
    /// Set block type at local coordinates
    /// </summary>
    public void SetBlock(int x, int y, int z, BlockType type)
    {
        if (x >= 0 && x < SIZE && y >= 0 && y < HEIGHT && z >= 0 && z < SIZE)
        {
            blocks[x, y, z] = (byte)type;
            IsDirty = true;
        }
    }

    /// <summary>
    /// Check if block at position is solid (not air or water)
    /// </summary>
    public bool IsBlockSolid(int x, int y, int z)
    {
        return BlockTypeConfig.IsSolid(GetBlock(x, y, z));
    }

    /// <summary>
    /// Check if block at position is transparent
    /// </summary>
    public bool IsBlockTransparent(int x, int y, int z)
    {
        return BlockTypeConfig.IsTransparent(GetBlock(x, y, z));
    }

    /// <summary>
    /// Check if chunk is empty (all air)
    /// </summary>
    public bool IsEmpty()
    {
        for (int x = 0; x < SIZE; x++)
        {
            for (int y = 0; y < HEIGHT; y++)
            {
                for (int z = 0; z < SIZE; z++)
                {
                    if (blocks[x, y, z] != (byte)BlockType.Air)
                        return false;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Clear dirty flag after saving or mesh rebuild
    /// </summary>
    public void ClearDirty()
    {
        IsDirty = false;
    }

    /// <summary>
    /// Get the highest non-air block at given x,z position
    /// Returns -1 if column is empty
    /// </summary>
    public int GetHighestBlock(int x, int z)
    {
        if (x < 0 || x >= SIZE || z < 0 || z >= SIZE)
            return -1;

        for (int y = HEIGHT - 1; y >= 0; y--)
        {
            if (blocks[x, y, z] != (byte)BlockType.Air)
                return y;
        }
        return -1;
    }
}
