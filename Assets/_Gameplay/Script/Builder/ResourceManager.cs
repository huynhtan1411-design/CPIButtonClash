using UnityEngine;
using System.Collections.Generic;
using WD;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [SerializeField] private ResourceItem resourcePrefab;
    [SerializeField] private int poolSize = 20;
    
    private Queue<ResourceItem> resourcePool;
    private int totalResources = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePool();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePool()
    {
        resourcePool = new Queue<ResourceItem>();
        
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewResourceItem();
        }
    }

    private void CreateNewResourceItem()
    {
        ResourceItem newItem = Instantiate(resourcePrefab, transform);
        newItem.gameObject.SetActive(false);
        resourcePool.Enqueue(newItem);
    }

    public ResourceItem SpawnResource(Vector3 position, int amount)
    {
        if (resourcePool.Count == 0)
        {
            CreateNewResourceItem();
        }

        ResourceItem item = resourcePool.Dequeue();
        item.transform.position = position;
        item.gameObject.SetActive(true);
        item.Initialize(amount);
        return item;
    }

    public void ReturnToPool(ResourceItem item)
    {
        if (item == null) return;
        
        item.gameObject.SetActive(false);
        resourcePool.Enqueue(item);
        Debug.Log("Resource returned to pool");
    }

    public void CollectResource(int amount)
    {
        totalResources += amount;
        Debug.Log($"Resource collected! Amount: {amount}, Total: {totalResources}");
        
        // Notify UI or other systems about resource collection
        if (WD.GameManager.Instance != null)
        {
            WD.GameManager.Instance.AddGold(amount);
        }
    }

    public int GetTotalResources()
    {
        return totalResources;
    }
} 
