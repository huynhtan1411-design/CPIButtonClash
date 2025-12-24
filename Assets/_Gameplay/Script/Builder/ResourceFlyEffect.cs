using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;

public class ResourceFlyEffect : MonoBehaviour
{
    [SerializeField] private GameObject resourcePrefab;
    [SerializeField] public int resourceCount = 5;
    [SerializeField] private float flyDuration = 1f;
    [SerializeField] private float delayBetweenResources = 0.1f;
    [SerializeField] private float arcHeight = 2f;
    [SerializeField] private float spreadRadius = 1f;

    private Queue<GameObject> resourcePool;

    public int ResourceCount => resourceCount;

    private void Awake()
    {
        resourcePool = new Queue<GameObject>();
        for (int i = 0; i < resourceCount; i++)
        {
            var resource = Instantiate(resourcePrefab, transform);
            resource.SetActive(false);
            resourcePool.Enqueue(resource);
        }
    }

    public void PlayResourceFlyEffect(Transform startTrans, Vector3 endPos, int resourceAmount = -1, System.Action<int> onArrive = null)
    {
        Debug.Log($"[ResourceFlyEffect] PlayResourceFlyEffect from {startTrans?.name} to {endPos} amount={(resourceAmount > 0 ? resourceAmount : resourceCount)}");
        StartCoroutine(SpawnResources(startTrans, endPos, resourceAmount, onArrive));
    }

    private IEnumerator SpawnResources(Transform startTrans, Vector3 endPos, int resourceAmount, System.Action<int> onArrive)
    {
        int count = resourceAmount > 0 ? resourceAmount : resourceCount;
        for (int i = 0; i < count; i++)
        {
            var resource = GetResourceFromPool();
            float startTime = Time.time;

            // Random start position within spread radius
            Vector3 randomOffset = Random.insideUnitSphere * spreadRadius;
            Vector3 spawnPos = startTrans.position + Vector3.up * 0.5f;
            resource.transform.position = spawnPos;

            // Calculate arc path
            Vector3 midPoint = (spawnPos + endPos) / 2f + Vector3.up * arcHeight;
            
            // Create path with 3 points
            Vector3[] path = new Vector3[] { spawnPos, midPoint, endPos };

            GameObject obj = resource.gameObject;
            // Animate along path
            resource.transform.DOPath(path, flyDuration, PathType.CatmullRom)
                .SetEase(Ease.OutQuad)
                .OnStart(() => { obj.SetActive(true); Audio_Manager.instance.play("Jump"); })
                .OnComplete(() => {
                    onArrive?.Invoke(1);
                    ReturnResourceToPool(resource);
                });

            yield return new WaitForSeconds(delayBetweenResources);
        }
    }

    private GameObject GetResourceFromPool()
    {
        if (resourcePool.Count > 0)
        {
            return resourcePool.Dequeue();
        }
        
        var newResource = Instantiate(resourcePrefab, transform);
        return newResource;
    }

    private void ReturnResourceToPool(GameObject resource)
    {
        resource.SetActive(false);
        resourcePool.Enqueue(resource);
    }
} 
