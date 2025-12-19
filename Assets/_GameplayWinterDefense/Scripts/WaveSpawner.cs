using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WD;
using CLHoma.Combat;
using UISystems;

[System.Serializable]
public class EnemyGroupConfig
{
    public EnemyData enemyData;
    public int count;
    public float spawnInterval;
    public SpawnDirection spawnDirection;
    [Tooltip("If true, next group will start spawning immediately without waiting for this group to finish")]
    public bool spawnWithNextGroup = false;
    [Tooltip("Delay before this group starts spawning")]
    public float delayBeforeSpawn = 0f;
}

public class WaveSpawner : MonoBehaviour
{
    [Tooltip("List of wave configurations for this level")]
    public List<WaveConfig> waves;
    [Tooltip("Spawn point for enemies")]
    public Transform spawnPoint;

    private int currentWaveIndex = -1;
    private WaveConfig currentWave;
    private GameManager gameManager;
    private bool isSpawning = false;
    public System.Action OnAllWavesComplete; // Callback when all waves are done

    public int CurrentWaveIndex { get => currentWaveIndex; set => currentWaveIndex = value; }

    private void Start()
    {
        gameManager = GameManager.Instance;
    }

    /// <summary>
    /// Resets the wave index to start from the first wave again
    /// </summary>
    public void ResetWaves()
    {
        currentWaveIndex = -1;
        isSpawning = false;
        StopAllCoroutines();
    }

    /// <summary>
    /// Starts spawning the next wave if available
    /// </summary>
    public void StartNextWave()
    {
        if (HasNextWave())
        {
            currentWaveIndex++;
            currentWave = waves[currentWaveIndex];
            StartCoroutine(SpawnWave());
        }
    }

    private Vector3 GetSpawnPosition(SpawnDirection direction)
    {
        // Get the center of the play area
        Vector3 center = Vector3.zero;
        float radius = gameManager.RadiusArea;

        // If direction is Random, choose a random direction excluding Random
        if (direction == SpawnDirection.Random)
        {
            SpawnDirection[] possibleDirections = new SpawnDirection[] 
            {
                SpawnDirection.Top, SpawnDirection.Down, SpawnDirection.Left, SpawnDirection.Right,
                SpawnDirection.TopLeft, SpawnDirection.TopRight, SpawnDirection.DownLeft, SpawnDirection.DownRight
            };
            direction = possibleDirections[Random.Range(0, possibleDirections.Length)];
        }

        // Calculate spawn position based on direction
        Vector3 spawnPos = direction switch
        {
            SpawnDirection.Top => new Vector3(0, 0, radius),
            SpawnDirection.Down => new Vector3(0, 0, -radius),
            SpawnDirection.Left => new Vector3(-radius, 0, 0),
            SpawnDirection.Right => new Vector3(radius, 0, 0),
            SpawnDirection.TopLeft => new Vector3(-radius, 0, radius),
            SpawnDirection.TopRight => new Vector3(radius, 0, radius),
            SpawnDirection.DownLeft => new Vector3(-radius, 0, -radius),
            SpawnDirection.DownRight => new Vector3(radius, 0, -radius),
            _ => Vector3.zero
        };

        // Add some random variation to prevent enemies from spawning at exact same spot
        float randomOffset = Random.Range(-0.3f, 0.3f);
        switch (direction)
        {
            case SpawnDirection.Top:
            case SpawnDirection.Down:
                spawnPos.x += randomOffset;
                break;
            case SpawnDirection.Left:
            case SpawnDirection.Right:
                spawnPos.z += randomOffset;
                break;
            case SpawnDirection.TopLeft:
            case SpawnDirection.TopRight:
            case SpawnDirection.DownLeft:
            case SpawnDirection.DownRight:
                spawnPos.x += randomOffset;
                spawnPos.z += randomOffset;
                break;
        }

        // Ensure spawn point is on NavMesh
        //UnityEngine.AI.NavMeshHit hit;
        //if (UnityEngine.AI.NavMesh.SamplePosition(spawnPos, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
        //{
        //    Debug.LogError(" hit.position " + hit.position);
        //    return hit.position;
        //}
        return spawnPos;
    }

    private IEnumerator SpawnWave()
    {
        isSpawning = true;
        List<IEnumerator> activeSpawners = new List<IEnumerator>();

        for (int groupIndex = 0; groupIndex < currentWave.enemyGroups.Count; groupIndex++)
        {
            var enemyGroup = currentWave.enemyGroups[groupIndex];
            
            // Wait for the group's initial delay
            if (enemyGroup.delayBeforeSpawn > 0)
            {
                yield return new WaitForSeconds(enemyGroup.delayBeforeSpawn);
            }

            // Start spawning this group
            var groupSpawner = SpawnEnemyGroup(enemyGroup);
            activeSpawners.Add(groupSpawner);
            StartCoroutine(groupSpawner);

            // If this group should not spawn with next group, wait for it to finish
            //if (!enemyGroup.spawnWithNextGroup && groupIndex < currentWave.enemyGroups.Count - 1)
            //{
            //    // Wait for the current group to finish spawning before starting the next one
            //    while (activeSpawners.Count > 0)
            //    {
            //        yield return null;
            //    }
            //}
        }

        // Wait for all remaining spawners to finish
        //while (activeSpawners.Count > 0)
        //{
        //    yield return null;
        //}

        isSpawning = false;
    }

    private IEnumerator SpawnEnemyGroup(EnemyGroup enemyGroup)
    {
        for (int i = 0; i < enemyGroup.count; i++)
        {
            // Get spawn position based on direction
            Vector3 spawnPosition = GetSpawnPosition(enemyGroup.spawnDirection);
            Vector3 currentRandomOffset = new Vector3(Random.Range(-0.1f, 0.1f), 0,Random.Range(-0.1f, 0.1f)
);
            // Instantiate enemy
            GameObject enemyObj = Instantiate(enemyGroup.enemyData.Prefab, spawnPosition + currentRandomOffset, Quaternion.identity);
            BaseEnemyBehavior baseEnemy = enemyObj.GetComponent<BaseEnemyBehavior>();
            baseEnemy.SetEnemyData(enemyGroup.enemyData);
            baseEnemy.Initialise();
            
            // Register enemy with GameManager
            gameManager.RegisterEnemy(enemyObj);

            // If this is not the last enemy in the group, wait for spawn interval
            if (i < enemyGroup.count - 1)
            {
                float time = Random.Range(enemyGroup.spawnInterval * 0.5f, enemyGroup.spawnInterval * 1.5f);

                yield return new WaitForSeconds(time);
            }
        }
    }

    public bool IsCurrentWaveComplete()
    {
        return !isSpawning;
    }

    public bool HasNextWave()
    {
        return currentWaveIndex < waves.Count - 1;
    }

    public WaveConfig GetNextWave()
    {
        if (HasNextWave())
        {
            StartCoroutine(ShowInfoWaveWithDelay());
            return waves[currentWaveIndex + 1];
        }
        return null;
    }

    private IEnumerator ShowInfoWaveWithDelay()
    {
        yield return new WaitForSeconds(0.3f);
        ShowInfoWave();
    }

    public void ShowInfoWave()
    {
        UIManager.instance.UIGameplayCtr.TextWave.text = "Wave " + (currentWaveIndex + 2) + "/" + waves.Count;
    }
}
