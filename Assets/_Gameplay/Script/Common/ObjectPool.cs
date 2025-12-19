using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : MonoBehaviour
{
    private Dictionary<string, Queue<T>> poolDictionary = new Dictionary<string, Queue<T>>();
    private Transform parentTransform;

    public ObjectPool(Transform parent)
    {
        parentTransform = parent;
    }
    private string GetOriginalPrefabName(T prefab)
    {
        string prefabName = prefab.name;
        int cloneIndex = prefabName.LastIndexOf("(Clone)");
        if (cloneIndex != -1)
        {
            prefabName = prefabName.Substring(0, cloneIndex);
        }
        return prefabName;
    }

    public T GetObject(T prefab, Vector3 position, Quaternion rotation)
    {
        T obj;
        string prefabName = GetOriginalPrefabName(prefab);

        if (poolDictionary.ContainsKey(prefabName) && poolDictionary[prefabName].Count > 0)
        {
            obj = poolDictionary[prefabName].Dequeue();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.gameObject.SetActive(true);
        }
        else
        {
            obj = GameObject.Instantiate(prefab, position, rotation, parentTransform);
        }
        ResetObject(obj);
        return obj;
    }
    public T GetObject(T prefab)
    {
        T obj;
        string prefabName = GetOriginalPrefabName(prefab);

        if (poolDictionary.ContainsKey(prefabName) && poolDictionary[prefabName].Count > 0)
        {
            obj = poolDictionary[prefabName].Dequeue();
            obj.gameObject.SetActive(true);
        }
        else
        {
            obj = GameObject.Instantiate(prefab, parentTransform);
        }
        ResetObject(obj);
        return obj;
    }

    public void ReturnObject(T obj)
    {
        string prefabName = GetOriginalPrefabName(obj);
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(parentTransform);
        if (!poolDictionary.ContainsKey(prefabName))
        {
            poolDictionary[prefabName] = new Queue<T>();
        }

        poolDictionary[prefabName].Enqueue(obj);
    }

    private void ResetObject(T obj)
    {
        obj.transform.localScale = Vector3.one;
    }
}
