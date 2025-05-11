using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public class PlayerSetup : MonoBehaviour
{
    [ContextMenu("创建玩家预制体")]
    public void CreatePlayerPrefab()
    {
        // 创建玩家对象
        GameObject player = new GameObject("Player");
        
        // 添加角色控制器
        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 1.8f;
        controller.radius = 0.5f;
        controller.center = new Vector3(0, 0.9f, 0);
        
        // 添加玩家控制脚本
        player.AddComponent<PlayerController>();
        
        // 创建相机
        GameObject cameraObj = new GameObject("PlayerCamera");
        cameraObj.transform.parent = player.transform;
        cameraObj.transform.localPosition = new Vector3(0, 1.6f, 0); // 相机位置在玩家头部
        
        // 添加相机组件
        Camera camera = cameraObj.AddComponent<Camera>();
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 1000f;
        
        // 添加音频监听器
        cameraObj.AddComponent<AudioListener>();
        
        // 保存预制体
        string prefabPath = "Assets/Prefabs/PlayerPrefab.prefab";
        
        // 确保目录存在
        if (!System.IO.Directory.Exists("Assets/Prefabs"))
        {
            System.IO.Directory.CreateDirectory("Assets/Prefabs");
        }
        
        // 创建预制体
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(player, prefabPath);
        
        if (prefab != null)
        {
            Debug.Log("玩家预制体创建成功: " + prefabPath);
        }
        else
        {
            Debug.LogError("玩家预制体创建失败!");
        }
        
        // 销毁临时对象
        DestroyImmediate(player);
    }
}
#endif
