using UnityEngine;
using TemplateSystems.Battle;
public class Tower : MonoBehaviour
{
    [Tooltip("Attack range of the tower")]
    public float range = 5f;
    [Tooltip("Shots per second")]
    public float fireRate = 1f;
    [Tooltip("Prefab for the projectile to spawn")]
    public GameObject projectilePrefab;
    [Tooltip("Spawn point for projectiles")]
    public Transform firePoint;

    private float fireCountdown = 0f;

    private void Update()
    {
        fireCountdown -= Time.deltaTime;
        if (fireCountdown <= 0f)
        {
            var target = FindNearestEnemy();
            if (target != null)
            {
                Shoot(target);
                fireCountdown = 1f / fireRate;
            }
        }
    }

    /// <summary>
    /// Finds the nearest enemy within range
    /// </summary>
    /// <returns>Transform of the nearest enemy or null if none found</returns>
    private Transform FindNearestEnemy()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearest = null;
        float shortestDist = Mathf.Infinity;

        foreach (var enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < shortestDist && dist <= range)
            {
                shortestDist = dist;
                nearest = enemy.transform;
            }
        }

        return nearest;
    }

    /// <summary>
    /// Shoots a projectile at the specified enemy
    /// </summary>
    private void Shoot(Transform target)
    {
        var projGO = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        var projectile = projGO.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Seek(target);
        }
    }
}