using UnityEngine;

/// <summary>
/// Renders a single chunk using merged mesh
/// Handles mesh rebuilding when blocks change
/// </summary>
public class ChunkRenderer : MonoBehaviour
{
    private ChunkData chunkData;
    private Vector2Int chunkPosition;

    // Solid mesh components
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    private Mesh solidMesh;

    // Water mesh components (separate for transparency)
    private GameObject waterObject;
    private MeshFilter waterMeshFilter;
    private MeshRenderer waterMeshRenderer;
    private Mesh waterMesh;

    // Materials
    private static Material solidMaterial;
    private static Material waterMaterial;

    // Dirty flag for deferred mesh rebuild
    private bool isDirty = false;
    private bool isInitialized = false;

    /// <summary>
    /// Initialize the chunk renderer
    /// </summary>
    public void Initialize(ChunkData data, Vector2Int pos)
    {
        chunkData = data;
        chunkPosition = pos;

        // Set world position
        transform.position = new Vector3(
            pos.x * ChunkData.SIZE,
            0,
            pos.y * ChunkData.SIZE
        );

        gameObject.name = $"Chunk_{pos.x}_{pos.y}";

        // Create materials if not exist
        CreateMaterials();

        // Setup solid mesh components
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshCollider = gameObject.AddComponent<MeshCollider>();

        meshRenderer.sharedMaterial = solidMaterial;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        meshRenderer.receiveShadows = true;

        // Setup water mesh (child object for separate rendering)
        waterObject = new GameObject("Water");
        waterObject.transform.parent = transform;
        waterObject.transform.localPosition = Vector3.zero;

        waterMeshFilter = waterObject.AddComponent<MeshFilter>();
        waterMeshRenderer = waterObject.AddComponent<MeshRenderer>();

        waterMeshRenderer.sharedMaterial = waterMaterial;
        waterMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        waterMeshRenderer.receiveShadows = true;

        isInitialized = true;

        // Build initial mesh
        RebuildMesh();
    }

    /// <summary>
    /// Create shared materials for all chunks
    /// </summary>
    private void CreateMaterials()
    {
        if (solidMaterial == null)
        {
            // Try shaders in order of preference
            string[] shaderNames = new string[]
            {
                "Custom/VertexColor",
                "Custom/VertexColorUnlit",   // Simple unlit vertex color shader
                "Particles/Standard Unlit",  // Unity built-in, supports vertex colors
                "Sprites/Default",           // Unity built-in, supports vertex colors
                "Legacy Shaders/Diffuse",
                "Diffuse",
                "Standard"
            };

            Shader shader = null;
            foreach (string name in shaderNames)
            {
                shader = Shader.Find(name);
                if (shader != null) break;
            }

            solidMaterial = new Material(shader);
            solidMaterial.enableInstancing = true;

            // For Particles/Standard Unlit, set color mode
            if (shader.name.Contains("Particles"))
            {
                solidMaterial.SetFloat("_ColorMode", 1); // Multiply
            }
        }

        if (waterMaterial == null)
        {
            // Try transparent shaders
            string[] waterShaderNames = new string[]
            {
                "Custom/VertexColorTransparent",
                "Custom/VertexColorUnlit",
                "Particles/Standard Unlit",
                "Sprites/Default",
                "Legacy Shaders/Transparent/Diffuse",
                "Transparent/Diffuse",
                "Standard"
            };

            Shader shader = null;
            foreach (string name in waterShaderNames)
            {
                shader = Shader.Find(name);
                if (shader != null) break;
            }

            waterMaterial = new Material(shader);
            waterMaterial.renderQueue = 3000;

            // Configure transparency for Standard shader
            if (shader.name == "Standard")
            {
                waterMaterial.SetFloat("_Mode", 3);
                waterMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                waterMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                waterMaterial.SetInt("_ZWrite", 0);
                waterMaterial.EnableKeyword("_ALPHABLEND_ON");
            }
        }
    }

    /// <summary>
    /// Mark chunk as needing mesh rebuild
    /// </summary>
    public void SetDirty()
    {
        isDirty = true;
    }

    void LateUpdate()
    {
        // Deferred mesh rebuild
        if (isDirty && isInitialized)
        {
            RebuildMesh();
            isDirty = false;
        }
    }

    /// <summary>
    /// Rebuild the chunk mesh
    /// </summary>
    public void RebuildMesh()
    {
        if (chunkData == null) return;

        // Build new meshes
        ChunkMeshBuilder builder = new ChunkMeshBuilder(chunkData, chunkPosition);
        builder.BuildMesh(out Mesh newSolidMesh, out Mesh newWaterMesh);

        // Update solid mesh
        if (solidMesh != null)
        {
            Destroy(solidMesh);
        }
        solidMesh = newSolidMesh;
        meshFilter.sharedMesh = solidMesh;

        // Update collider
        if (solidMesh.vertexCount > 0)
        {
            meshCollider.sharedMesh = solidMesh;
        }
        else
        {
            meshCollider.sharedMesh = null;
        }

        // Update water mesh
        if (waterMesh != null)
        {
            Destroy(waterMesh);
        }
        waterMesh = newWaterMesh;
        waterMeshFilter.sharedMesh = waterMesh;

        // Enable/disable water object based on content
        waterObject.SetActive(waterMesh.vertexCount > 0);

        // Clear dirty flag on chunk data
        chunkData.ClearDirty();
    }

    /// <summary>
    /// Get chunk position
    /// </summary>
    public Vector2Int GetChunkPosition()
    {
        return chunkPosition;
    }

    /// <summary>
    /// Clean up when chunk is unloaded
    /// </summary>
    public void Unload()
    {
        if (solidMesh != null)
        {
            Destroy(solidMesh);
        }
        if (waterMesh != null)
        {
            Destroy(waterMesh);
        }
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (solidMesh != null)
        {
            Destroy(solidMesh);
        }
        if (waterMesh != null)
        {
            Destroy(waterMesh);
        }
    }
}
