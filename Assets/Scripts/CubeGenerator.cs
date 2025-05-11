using UnityEngine;
using System.Collections.Generic;

public class CubeGenerator : MonoBehaviour
{
    [Header("方块设置")]
    public GameObject cubePrefab;
    public int chunkSize = 16;
    public int renderDistance = 3;
    
    [Header("地形设置")]
    public float noiseScale = 20f;
    public float heightScale = 10f;
    public int seed;
    
    [Header("颜色设置")]
    public Color grassColor = new Color(0.4f, 0.7f, 0.2f);
    public Color dirtColor = new Color(0.6f, 0.4f, 0.2f);
    public Color stoneColor = new Color(0.5f, 0.5f, 0.5f);
    public Color waterColor = new Color(0.2f, 0.4f, 0.8f, 0.7f);
    public Color sandColor = new Color(0.9f, 0.8f, 0.5f);
    public Color treeColor = new Color(0.3f, 0.2f, 0.1f);
    public Color leafColor = new Color(0.2f, 0.5f, 0.1f);
    
    private Dictionary<Vector2Int, GameObject> chunks = new Dictionary<Vector2Int, GameObject>();
    private Transform player;
    private Vector2Int currentChunk = new Vector2Int(0, 0);
    private Vector3 lastPlayerPosition = Vector3.zero;
    private float distanceMoved = 0;
    
    void Start()
    {
        // 设置随机种子
        if (seed == 0)
            seed = Random.Range(1, 99999);
        Random.InitState(seed);
        
        // 延迟初始化，等待玩家生成
        Invoke("InitializeWorld", 0.5f);
    }
    
    void InitializeWorld()
    {
        // 查找玩家
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log("找到玩家: " + player.position);
            lastPlayerPosition = player.position;
            
            // 生成初始区块
            UpdateChunks();
        }
        else if (Camera.main != null)
        {
            player = Camera.main.transform;
            Debug.Log("找到相机: " + player.position);
            lastPlayerPosition = player.position;
            
            // 生成初始区块
            UpdateChunks();
        }
        else
        {
            // 如果还没找到相机，继续尝试
            Invoke("InitializeWorld", 0.5f);
            Debug.Log("等待玩家初始化...");
        }
    }
    
    void Update()
    {
        // 确保有玩家
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("找到玩家: " + player.position);
                lastPlayerPosition = player.position;
            }
            else if (Camera.main != null)
            {
                player = Camera.main.transform;
                Debug.Log("找到相机: " + player.position);
                lastPlayerPosition = player.position;
            }
            else
            {
                return; // 如果没有玩家，不执行更新
            }
        }
        
        // 计算玩家移动距离
        distanceMoved += Vector3.Distance(player.position, lastPlayerPosition);
        lastPlayerPosition = player.position;
        
        // 获取玩家当前所在区块
        Vector2Int newChunk = new Vector2Int(
            Mathf.FloorToInt(player.position.x / chunkSize),
            Mathf.FloorToInt(player.position.z / chunkSize)
        );
        
        // 每帧都检查一下玩家位置，确保不会掉出地图
        if (player.position.y < -10)
        {
            Debug.LogWarning("玩家掉出地图，重置位置");
            // 找到GameManager并重置玩家位置
            GameManager manager = FindObjectOfType<GameManager>();
            if (manager != null)
            {
                manager.RespawnPlayer();
            }
        }
        
        // 如果玩家移动到新区块或移动了足够远的距离，更新可见区块
        if (newChunk != currentChunk || distanceMoved > chunkSize / 2)
        {
            Debug.Log($"玩家移动到新区块: {newChunk.x}, {newChunk.y}，原区块: {currentChunk.x}, {currentChunk.y}，位置: {player.position}");
            currentChunk = newChunk;
            distanceMoved = 0;
            UpdateChunks();
        }
        
        // 每5秒强制更新一次区块，确保地形正确生成
        if (Time.frameCount % 150 == 0)
        {
            Debug.Log("定期更新区块，玩家位置: " + player.position);
            UpdateChunks();
        }
    }
    
    void UpdateChunks()
    {
        if (player == null)
        {
            Debug.LogError("更新区块时玩家为空");
            return;
        }
        
        Debug.Log($"更新区块，当前区块: {currentChunk.x}, {currentChunk.y}，玩家位置: {player.position}");
        
        // 记录需要保留的区块
        HashSet<Vector2Int> neededChunks = new HashSet<Vector2Int>();
        
        // 计算需要生成的区块范围
        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int z = -renderDistance; z <= renderDistance; z++)
            {
                Vector2Int chunkPos = new Vector2Int(currentChunk.x + x, currentChunk.y + z);
                neededChunks.Add(chunkPos);
                
                // 如果区块不存在，生成它
                if (!chunks.ContainsKey(chunkPos))
                {
                    GameObject newChunk = new GameObject($"Chunk_{chunkPos.x}_{chunkPos.y}");
                    newChunk.transform.parent = transform;
                    chunks[chunkPos] = newChunk;
                    GenerateChunk(chunkPos, newChunk.transform);
                    Debug.Log($"生成新区块: {chunkPos.x}, {chunkPos.y}，玩家位置: {player.position}");
                }
            }
        }
        
        // 移除不再需要的区块
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        foreach (var chunk in chunks)
        {
            if (!neededChunks.Contains(chunk.Key))
            {
                chunksToRemove.Add(chunk.Key);
            }
        }
        
        foreach (var chunkPos in chunksToRemove)
        {
            Debug.Log($"销毁远处区块: {chunkPos.x}, {chunkPos.y}");
            Destroy(chunks[chunkPos]);
            chunks.Remove(chunkPos);
        }
        
        Debug.Log($"当前区块总数: {chunks.Count}，玩家位置: {player.position}");
    }
    
    void GenerateChunk(Vector2Int chunkPos, Transform parent)
    {
        int startX = chunkPos.x * chunkSize;
        int startZ = chunkPos.y * chunkSize;
        
        // 生成地形
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                int worldX = startX + x;
                int worldZ = startZ + z;
                
                // 使用柏林噪声生成高度
                float height = GenerateHeight(worldX, worldZ);
                int intHeight = Mathf.FloorToInt(height);
                
                // 生成地面方块
                CreateCube(new Vector3(worldX, intHeight, worldZ), GetTerrainColor(intHeight, height), parent);
                
                // 生成水面
                int waterLevel = 3;
                if (intHeight < waterLevel)
                {
                    CreateCube(new Vector3(worldX, waterLevel, worldZ), waterColor, parent, true);
                }
                
                // 随机生成树木
                if (Random.value < 0.02f && intHeight > waterLevel)
                {
                    GenerateTree(new Vector3(worldX, intHeight + 1, worldZ), parent);
                }
            }
        }
    }
    
    float GenerateHeight(int x, int z)
    {
        // 使用多层柏林噪声生成自然地形
        float scale = noiseScale;
        
        float height = 0;
        height += Mathf.PerlinNoise((x + seed) / scale, (z + seed) / scale) * heightScale;
        
        // 添加一些小的细节变化
        height += Mathf.PerlinNoise((x + seed) / (scale * 0.5f), (z + seed) / (scale * 0.5f)) * 2;
        
        return height;
    }
    
    Color GetTerrainColor(int height, float exactHeight)
    {
        // 根据高度返回不同的颜色
        int waterLevel = 3;
        
        if (height < waterLevel - 1)
            return stoneColor; // 水下石头
        else if (height < waterLevel)
            return sandColor; // 沙滩
        else if (height < 8)
            return grassColor; // 草地
        else if (height < 12)
            return dirtColor; // 泥土
        else
            return stoneColor; // 山石
    }
    
    void GenerateTree(Vector3 position, Transform parent)
    {
        // 树干
        int treeHeight = Random.Range(3, 6);
        for (int y = 0; y < treeHeight; y++)
        {
            CreateCube(position + new Vector3(0, y, 0), treeColor, parent);
        }
        
        // 树叶
        int leafSize = Random.Range(2, 4);
        for (int x = -leafSize; x <= leafSize; x++)
        {
            for (int z = -leafSize; z <= leafSize; z++)
            {
                for (int y = 0; y < leafSize; y++)
                {
                    // 创建球形树冠
                    if (x*x + y*y + z*z <= leafSize*leafSize)
                    {
                        Vector3 leafPos = position + new Vector3(x, treeHeight + y, z);
                        CreateCube(leafPos, leafColor, parent);
                    }
                }
            }
        }
    }
    
    void CreateCube(Vector3 position, Color color, Transform parent, bool isTransparent = false)
    {
        GameObject cube = Instantiate(cubePrefab, position, Quaternion.identity, parent);
        
        // 设置方块颜色
        Renderer renderer = cube.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(renderer.material);
            material.color = color;
            
            if (isTransparent)
            {
                material.SetFloat("_Mode", 3); // 透明模式
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
            }
            
            renderer.material = material;
        }
    }
}
