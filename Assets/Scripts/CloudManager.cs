using UnityEngine;

// 这个脚本用于在场景中添加CloudGenerator组件
[DefaultExecutionOrder(-100)] // 确保在其他脚本之前执行
public class CloudManager : MonoBehaviour
{
    void Awake()
    {
        // 检查场景中是否已经有CloudGenerator
        CloudGenerator existingGenerator = FindObjectOfType<CloudGenerator>();
        if (existingGenerator == null)
        {
            // 创建云朵生成器对象
            GameObject cloudGeneratorObj = new GameObject("CloudGenerator");
            CloudGenerator cloudGenerator = cloudGeneratorObj.AddComponent<CloudGenerator>();
            
            // 设置云朵生成器参数
            cloudGenerator.cubePrefab = FindCubePrefab();
            cloudGenerator.cloudCount = 10;
            cloudGenerator.minCloudHeight = 30f;
            cloudGenerator.maxCloudHeight = 50f;
            cloudGenerator.cloudSpawnRadius = 100f;
            cloudGenerator.minCubesPerCloud = 15;
            cloudGenerator.maxCubesPerCloud = 40;
            cloudGenerator.cloudDensity = 0.7f;
            cloudGenerator.cloudColor = Color.white;
            
            Debug.Log("CloudManager: 已创建云朵生成器");
        }
        else
        {
            Debug.Log("CloudManager: 场景中已存在云朵生成器");
        }
    }
    
    // 查找场景中的方块预制体
    private GameObject FindCubePrefab()
    {
        // 尝试从CubeGenerator获取预制体
        CubeGenerator cubeGenerator = FindObjectOfType<CubeGenerator>();
        if (cubeGenerator != null && cubeGenerator.cubePrefab != null)
        {
            Debug.Log("CloudManager: 从CubeGenerator获取到方块预制体");
            return cubeGenerator.cubePrefab;
        }
        
        // 如果找不到，创建一个简单的立方体
        Debug.LogWarning("CloudManager: 无法找到方块预制体，将创建一个默认立方体");
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "CloudCube";
        cube.SetActive(false); // 隐藏原始预制体
        DontDestroyOnLoad(cube); // 防止被销毁
        return cube;
    }
}
