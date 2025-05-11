using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("游戏设置")]
    public GameObject playerPrefab;
    public Vector3 spawnPosition = new Vector3(0, 20, 0); // 玩家生成位置，高度设置高一些以便让玩家落到地面上
    
    private GameObject player;
    
    void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        InitializeGame();
    }
    
    void InitializeGame()
    {
        // 生成玩家
        SpawnPlayer();
    }
    
    void SpawnPlayer()
    {
        if (playerPrefab != null)
        {
            player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            player.tag = "Player"; // 确保设置标签
            Debug.Log("玩家已生成在位置: " + spawnPosition);
            
            // 确保只有一个AudioListener
            RemoveExtraAudioListeners();
        }
        else
        {
            Debug.LogError("未设置玩家预制体！");
        }
    }
    
    public void RespawnPlayer()
    {
        if (player != null)
        {
            // 保存玩家旋转，这样重生后视角不会改变
            Quaternion rotation = player.transform.rotation;
            Destroy(player);
            player = Instantiate(playerPrefab, spawnPosition, rotation);
            player.tag = "Player"; // 确保设置标签
            Debug.Log("玩家已重生在位置: " + spawnPosition);
            
            // 确保只有一个AudioListener
            RemoveExtraAudioListeners();
        }
        else
        {
            SpawnPlayer();
        }
    }
    
    void RemoveExtraAudioListeners()
    {
        // 查找所有AudioListener
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        
        if (listeners.Length > 1)
        {
            Debug.LogWarning("场景中有多个AudioListener，正在修复...");
            
            // 保留玩家相机上的AudioListener，删除其他的
            AudioListener playerListener = null;
            
            // 查找玩家相机上的AudioListener
            if (player != null)
            {
                Camera playerCamera = player.GetComponentInChildren<Camera>();
                if (playerCamera != null)
                {
                    playerListener = playerCamera.GetComponent<AudioListener>();
                }
            }
            
            // 如果没有找到玩家相机上的AudioListener，保留第一个找到的
            if (playerListener == null && listeners.Length > 0)
            {
                playerListener = listeners[0];
            }
            
            // 删除其他AudioListener
            foreach (AudioListener listener in listeners)
            {
                if (listener != playerListener)
                {
                    Debug.Log("删除多余的AudioListener: " + listener.gameObject.name);
                    Destroy(listener);
                }
            }
        }
    }
}
