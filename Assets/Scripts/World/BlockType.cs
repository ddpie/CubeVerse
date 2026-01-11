using UnityEngine;

/// <summary>
/// Block type enumeration - uses byte for memory efficiency
/// </summary>
public enum BlockType : byte
{
    Air = 0,
    Grass = 1,
    Dirt = 2,
    Stone = 3,
    Sand = 4,
    Water = 5,
    Wood = 6,
    Leaf = 7
}

/// <summary>
/// Block type configuration - provides color and properties for each block type
/// </summary>
public static class BlockTypeConfig
{
    // Default colors (used if CubeGenerator not found)
    private static Color[] defaultColors = new Color[]
    {
        new Color(0, 0, 0, 0),           // Air - transparent
        new Color(0.4f, 0.7f, 0.2f),     // Grass - green
        new Color(0.6f, 0.4f, 0.2f),     // Dirt - brown
        new Color(0.5f, 0.5f, 0.5f),     // Stone - gray
        new Color(0.9f, 0.8f, 0.5f),     // Sand - yellow
        new Color(0.2f, 0.4f, 0.8f, 0.7f), // Water - blue, semi-transparent
        new Color(0.3f, 0.2f, 0.1f),     // Wood - dark brown
        new Color(0.2f, 0.5f, 0.1f)      // Leaf - dark green
    };

    // Cached reference to CubeGenerator
    private static CubeGenerator cachedGenerator;

    /// <summary>
    /// Get colors from CubeGenerator Inspector settings
    /// </summary>
    public static Color[] Colors
    {
        get
        {
            // Try to get CubeGenerator reference
            if (cachedGenerator == null)
            {
                cachedGenerator = Object.FindObjectOfType<CubeGenerator>();
            }

            if (cachedGenerator != null)
            {
                return new Color[]
                {
                    new Color(0, 0, 0, 0),          // Air
                    cachedGenerator.grassColor,     // Grass
                    cachedGenerator.dirtColor,      // Dirt
                    cachedGenerator.stoneColor,     // Stone
                    cachedGenerator.sandColor,      // Sand
                    cachedGenerator.waterColor,     // Water
                    cachedGenerator.treeColor,      // Wood
                    cachedGenerator.leafColor       // Leaf
                };
            }

            return defaultColors;
        }
    }

    /// <summary>
    /// Check if block type is solid (blocks light and collision)
    /// </summary>
    public static bool IsSolid(BlockType type)
    {
        return type != BlockType.Air && type != BlockType.Water;
    }

    /// <summary>
    /// Check if block type is transparent
    /// </summary>
    public static bool IsTransparent(BlockType type)
    {
        return type == BlockType.Air || type == BlockType.Water;
    }

    /// <summary>
    /// Get color for block type
    /// </summary>
    public static Color GetColor(BlockType type)
    {
        int index = (int)type;
        if (index >= 0 && index < Colors.Length)
            return Colors[index];
        return Color.magenta; // Error color
    }

    /// <summary>
    /// Find closest block type for a given color
    /// </summary>
    public static BlockType GetBlockTypeFromColor(Color color)
    {
        float minDistance = float.MaxValue;
        BlockType closest = BlockType.Dirt;

        for (int i = 1; i < Colors.Length; i++) // Skip Air
        {
            BlockType type = (BlockType)i;
            if (type == BlockType.Water) continue; // Skip water

            float dist = ColorDistance(color, Colors[i]);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = type;
            }
        }

        return closest;
    }

    private static float ColorDistance(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b);
    }
}
