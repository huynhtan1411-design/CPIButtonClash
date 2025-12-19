using CLHoma.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AFKCombatController : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Transform projectileSpawnPoint;
    
    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 10f;
    
    [Header("Enemy")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private float enemyMoveSpeed = 2f;
    
    private bool isAttacking = false;
    private bool isEnabled = false;
    private Coroutine spawnCoroutine;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private List<GameObject> activeProjectiles = new List<GameObject>();
    private Dictionary<GameObject, GameObject> projectileTargets = new Dictionary<GameObject, GameObject>();


    private void OnEnable()
    {
        // Enable the controller when the game starts
        Enable();
    }
    private void OnDisable()
    {
        // Disable the controller when the game stops
        Disable();
    }
    public void Enable()
    {
        if (isEnabled) return;

        isAttacking = false;
        isEnabled = true;
        spawnCoroutine = StartCoroutine(SpawnEnemyRoutine());
    }

    public void Disable()
    {
        if (!isEnabled) return;
        
        isEnabled = false;
        
        // Stop spawning
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
        
        // Clear all active objects
        ClearAllObjects();
    }

    private void ClearAllObjects()
    {
        // Clear enemies
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        activeEnemies.Clear();
        
        // Clear projectiles
        foreach (var projectile in activeProjectiles)
        {
            if (projectile != null)
            {
                Destroy(projectile);
            }
        }
        activeProjectiles.Clear();
        projectileTargets.Clear();
    }

    private void Update()
    {
        if (!isEnabled) return;
        
        if (!isAttacking)
        {
            StartCoroutine(PlayerAttackRoutine());
        }

        MoveEnemies();
        MoveProjectiles();
    }

    private IEnumerator SpawnEnemyRoutine()
    {
        while (isEnabled)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab != null && spawnPoint != null)
        {
            Vector3 pos = Utils.GetRandomPositionAroundX(spawnPoint.position, 1.5f);
            GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
            enemy.transform.SetParent(transform);
            activeEnemies.Add(enemy);
        }
    }

    private IEnumerator PlayerAttackRoutine()
    {
        isAttacking = true;
        
        // Spawn projectile
        GameObject nearestEnemy = FindNearestEnemy();
        if (nearestEnemy != null)
        {
            SpawnProjectile(nearestEnemy);
            // Play attack animation
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger("Shoot");
            }
        }
        
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    private void SpawnProjectile(GameObject targetEnemy)
    {
        if (projectilePrefab != null && projectileSpawnPoint != null && targetEnemy != null)
        {
            Vector3 direction = (targetEnemy.transform.position - projectileSpawnPoint.position).normalized;
            Quaternion rotation = Quaternion.LookRotation(direction);
            
            GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, rotation);
            activeProjectiles.Add(projectile);
            projectileTargets[projectile] = targetEnemy;
        }
    }

    private void MoveEnemies()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            GameObject enemy = activeEnemies[i];
            if (enemy == null)
            {
                activeEnemies.RemoveAt(i);
                continue;
            }

            Vector3 direction = (playerTransform.position - enemy.transform.position).normalized;
            enemy.transform.position += direction * enemyMoveSpeed * Time.deltaTime;
            
            // Update rotation to face player
            enemy.transform.LookAt(playerTransform);
        }
    }

    private void MoveProjectiles()
    {
        for (int i = activeProjectiles.Count - 1; i >= 0; i--)
        {
            GameObject projectile = activeProjectiles[i];
            if (projectile == null)
            {
                activeProjectiles.RemoveAt(i);
                continue;
            }

            projectile.transform.position += projectile.transform.forward * projectileSpeed * Time.deltaTime;
            
            // Check if projectile reached target
            if (projectileTargets.TryGetValue(projectile, out GameObject targetEnemy))
            {
                if (targetEnemy == null)
                {
                    // Target was destroyed, remove projectile
                    activeProjectiles.RemoveAt(i);
                    projectileTargets.Remove(projectile);
                    Destroy(projectile);
                    continue;
                }

                Vector3 projectilePos = projectile.transform.position;
                Vector3 targetPos = targetEnemy.transform.position;
                // Ignore Y axis in distance calculation
                float distance = Vector2.Distance(
                    new Vector2(projectilePos.x, projectilePos.z),
                    new Vector2(targetPos.x, targetPos.z)
                );
                
                if (distance < 0.2f)
                {
                    // Destroy target enemy
                    if (targetEnemy != null)
                    {
                        activeEnemies.Remove(targetEnemy);
                        targetEnemy.GetComponentInChildren<Animator>().SetTrigger("IsDead");
                        Destroy(targetEnemy, 2f);
                    }
                    // Destroy projectile
                    activeProjectiles.RemoveAt(i);
                    projectileTargets.Remove(projectile);
                    Destroy(projectile);
                }
            }
        }
    }

    private GameObject FindNearestEnemy()
    {
        GameObject nearest = null;
        float minDistance = 10f;

        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            GameObject enemy = activeEnemies[i];
            if (enemy == null)
            {
                activeEnemies.RemoveAt(i);
                continue;
            }

            Vector3 playerPos = playerTransform.position;
            Vector3 enemyPos = enemy.transform.position;
            // Ignore Y axis in distance calculation
            float distance = Vector2.Distance(
                new Vector2(playerPos.x, playerPos.z),
                new Vector2(enemyPos.x, enemyPos.z)
            );
            
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = enemy;
            }
        }

        return nearest;
    }

    private void OnDestroy()
    {
        ClearAllObjects();
    }
}
