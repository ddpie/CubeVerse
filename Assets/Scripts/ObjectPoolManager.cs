using UnityEngine;
using System.Collections.Generic;

// 这个脚本提供了一个通用的对象池系统，可以替代直接使用Instantiate和Destroy
public class ObjectPoolManager : MonoBehaviour
{
    private static ObjectPoolManager instance;
    public static ObjectPoolManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("ObjectPoolManager");
                instance = go.AddComponent<ObjectPoolManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }
    
    // 对象池字典，键是预制体，值是对象池
    private Dictionary<GameObject, List<GameObject>> poolDictionary = new Dictionary<GameObject, List<GameObject>>();
    
    // 从对象池获取对象
    public GameObject GetObjectFromPool(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        // 如果这个预制体还没有对象池，创建一个
        if (!poolDictionary.ContainsKey(prefab))
        {
            poolDictionary[prefab] = new List<GameObject>();
        }
        
        // 查找可用的对象
        List<GameObject> pool = poolDictionary[prefab];
        GameObject objectToUse = null;
        
        // 查找未激活的对象
        foreach (GameObject obj in pool)
        {
            if (!obj.activeInHierarchy)
            {
                objectToUse = obj;
                break;
            }
        }
        
        // 如果没有找到可用对象，创建一个新的
        if (objectToUse == null)
        {
            objectToUse = Instantiate(prefab);
            pool.Add(objectToUse);
        }
        
        // 设置对象位置和旋转
        objectToUse.transform.position = position;
        objectToUse.transform.rotation = rotation;
        objectToUse.SetActive(true);
        
        return objectToUse;
    }
    
    // 返回对象到池中
    public void ReturnObjectToPool(GameObject obj)
    {
        obj.SetActive(false);
    }
    
    // 预热对象池（提前创建对象）
    public void PrewarmPool(GameObject prefab, int count)
    {
        if (!poolDictionary.ContainsKey(prefab))
        {
            poolDictionary[prefab] = new List<GameObject>();
        }
        
        List<GameObject> pool = poolDictionary[prefab];
        
        // 创建指定数量的对象
        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            pool.Add(obj);
        }
    }
}
