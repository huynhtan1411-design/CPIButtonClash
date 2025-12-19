using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;

public class ResourceFlyEffect : MonoBehaviour
{
    [SerializeField] private GameObject resourcePrefab;
    [SerializeField] private int resourceCount = 5;
    [SerializeField] private float flyDuration = 1f;
    [SerializeField] private float delayBetweenResources = 0.1f;
    [SerializeField] private float arcHeight = 2f;
    [SerializeField] private float spreadRadius = 1f;

    private Queue<GameObject> resourcePool;

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

    public void PlayResourceFlyEffect(Transform startTrans, Vector3 endPos)
    {
        StartCoroutine(SpawnResources(startTrans, endPos));
    }

    private IEnumerator SpawnResources(Transform startTrans, Vector3 endPos)
    {
        for (int i = 0; i < resourceCount; i++)
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
                .OnUpdate(() => {
                    // Update the path during animation
                    float elapsedTime = Time.time - startTime;
                    if (elapsedTime < flyDuration)
                    {
                        Vector3 currentPos = resource.transform.position;
                        Vector3 newMidPoint = (currentPos + endPos) / 2f + Vector3.up * arcHeight;
                        Vector3[] newPath = new Vector3[] { currentPos, newMidPoint, endPos };
                        resource.transform.DOPath(newPath, flyDuration - elapsedTime, PathType.CatmullRom);
                    }
                })
                .OnComplete(() => {
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