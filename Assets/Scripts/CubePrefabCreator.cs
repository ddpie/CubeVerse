using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public class CubePrefabCreator : MonoBehaviour
{
    public GameObject cubePrefab;
    
    [ContextMenu("创建方块预制体")]
    public void CreateCubePrefab()
    {
        // 确保有一个立方体对象
        if (cubePrefab == null)
        {
            cubePrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cubePrefab.name = "CubePrefab";
        }
        
        // 保存预制体
        string prefabPath = "Assets/Prefabs/CubePrefab.prefab";
        
        // 确保目录存在
        if (!System.IO.Directory.Exists("Assets/Prefabs"))
        {
            System.IO.Directory.CreateDirectory("Assets/Prefabs");
        }
        
        // 创建预制体
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(cubePrefab, prefabPath);
        
        if (prefab != null)
        {
            Debug.Log("方块预制体创建成功: " + prefabPath);
        }
        else
        {
            Debug.LogError("方块预制体创建失败!");
        }
        
        // 如果是临时创建的立方体，销毁它
        if (cubePrefab.scene.IsValid())
        {
            DestroyImmediate(cubePrefab);
        }
    }
}
#endif
