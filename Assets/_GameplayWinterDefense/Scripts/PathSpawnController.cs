using System.Collections;
using UnityEngine;
using CLHoma.Combat;

namespace WD
{
    public class PathSpawnController : MonoBehaviour
    {
        [Header("Spawn Setup")]
        [SerializeField] private EnemyData enemyData;
        [SerializeField] private GameObject enemyPrefabOverride;
        [SerializeField] private int enemyLevel = 1;
        [SerializeField] private PathMovementWD[] paths;
        [SerializeField] private float randomOffset = 0.1f;
        [SerializeField] private float spawnInterval = 1f;
        [SerializeField] private int spawnCountPerTick = 1;

        [Header("Elite Spawn")]
        [SerializeField] private EnemyData eliteEnemyData;
        [SerializeField] private GameObject elitePrefabOverride;
        [SerializeField] private int eliteLevel = 1;
        [SerializeField] private Transform eliteSpawnPoint;
        [SerializeField] private float eliteRandomOffset = 0.15f;

        private Coroutine[] spawnCoroutines;
        private bool[] isSpawning;

        // Chống toggle 2 lần trong cùng 1 frame (hay gặp khi có 2 HotkeyManager/2 nguồn input)
        private int[] lastToggleFrame;

        private void OnEnable()
        {
            InitialiseState();
        }

        private void OnDisable()
        {
            StopAllSpawns();
        }

        private void InitialiseState()
        {
            int count = paths != null ? paths.Length : 0;
            spawnCoroutines = new Coroutine[count];
            isSpawning = new bool[count];

            lastToggleFrame = new int[count];
            for (int i = 0; i < count; i++)
                lastToggleFrame[i] = -1;
        }

        public void SpawnAtIndex(int index)
        {
            if (!IsValidIndex(index))
            {
                Debug.LogWarning($"Invalid path index: {index}", this);
                return;
            }

            SpawnAtPath(paths[index]);
        }

        public void ToggleSpawnAtIndex(int index)
        {
            EnsureState();

            if (!IsValidIndex(index))
            {
                Debug.LogWarning($"Invalid path index: {index}", this);
                return;
            }

            // Prevent double-toggle in same frame
            if (lastToggleFrame != null && index < lastToggleFrame.Length)
            {
                if (lastToggleFrame[index] == Time.frameCount) return;
                lastToggleFrame[index] = Time.frameCount;
            }

            if (isSpawning[index]) StopSpawn(index);
            else StartSpawn(index);
        }

        // Dùng cho kiểu "giữ phím để spawn"
        public void StartSpawnAtIndex(int index)
        {
            EnsureState();
            if (!IsValidIndex(index))
            {
                Debug.LogWarning($"Invalid path index: {index}", this);
                return;
            }

            StartSpawn(index);
        }

        public void StopSpawnAtIndex(int index)
        {
            EnsureState();
            if (!IsValidIndex(index))
            {
                Debug.LogWarning($"Invalid path index: {index}", this);
                return;
            }

            StopSpawn(index);
        }

        public void SpawnAtPath(PathMovementWD path)
        {
            GameObject prefabToSpawn = ResolvePrefab(enemyPrefabOverride, enemyData);

            if (prefabToSpawn == null)
            {
                Debug.LogWarning("Enemy prefab is not assigned (set EnemyData or PrefabOverride).", this);
                return;
            }

            if (path == null || path.StartPoint == null)
            {
                Debug.LogWarning("Path or StartPoint is missing.", this);
                return;
            }

            Vector3 offset = new Vector3(
                Random.Range(-randomOffset, randomOffset),
                0f,
                Random.Range(-randomOffset, randomOffset)
            );

            SpawnEnemy(prefabToSpawn, enemyData, enemyLevel, path.StartPoint.position + offset);
        }

        public void SpawnEliteOnce()
        {
            GameObject prefabToSpawn = ResolvePrefab(elitePrefabOverride, eliteEnemyData);
            if (prefabToSpawn == null)
            {
                Debug.LogWarning("Elite prefab is not assigned (set EliteEnemyData or ElitePrefabOverride).", this);
                return;
            }

            if (eliteSpawnPoint == null)
            {
                Debug.LogWarning("Elite spawn point is not assigned.", this);
                return;
            }

            Vector3 offset = new Vector3(
                Random.Range(-eliteRandomOffset, eliteRandomOffset),
                0f,
                Random.Range(-eliteRandomOffset, eliteRandomOffset)
            );

            SpawnEnemy(prefabToSpawn, eliteEnemyData, eliteLevel, eliteSpawnPoint.position + offset);
        }

        private void StartSpawn(int index)
        {
            if (isSpawning[index])
                return;

            spawnCoroutines[index] = StartCoroutine(SpawnLoop(index));
            isSpawning[index] = true;
        }

        private void StopSpawn(int index)
        {
            if (!isSpawning[index])
                return;

            if (spawnCoroutines[index] != null)
                StopCoroutine(spawnCoroutines[index]);

            spawnCoroutines[index] = null;
            isSpawning[index] = false;
        }

        public void StopAllSpawns()
        {
            if (spawnCoroutines == null)
                return;

            for (int i = 0; i < spawnCoroutines.Length; i++)
                StopSpawn(i);
        }

        private IEnumerator SpawnLoop(int index)
        {
            var wait = new WaitForSecondsRealtime(spawnInterval);

            while (true)
            {
                int count = Mathf.Max(1, spawnCountPerTick);
                for (int i = 0; i < count; i++)
                    SpawnAtIndex(index);

                yield return wait;
            }
        }

        private bool IsValidIndex(int index)
        {
            return paths != null && index >= 0 && index < paths.Length;
        }

        private void EnsureState()
        {
            if (spawnCoroutines == null
                || isSpawning == null
                || lastToggleFrame == null
                || (paths != null && spawnCoroutines.Length != paths.Length))
            {
                InitialiseState();
            }
        }

        private GameObject ResolvePrefab(GameObject prefabOverride, EnemyData data)
        {
            if (prefabOverride != null)
                return prefabOverride;
            if (data != null)
                return data.Prefab;
            return null;
        }

        private void SpawnEnemy(GameObject prefabToSpawn, EnemyData data, int level, Vector3 position)
        {
            GameObject enemyObj = Instantiate(prefabToSpawn, position, Quaternion.identity);

            BaseEnemyBehavior baseEnemy = enemyObj.GetComponent<BaseEnemyBehavior>();
            if (baseEnemy == null)
            {
                Debug.LogWarning("Spawned enemy is missing BaseEnemyBehavior.", enemyObj);
                Destroy(enemyObj);
                return;
            }

            if (data != null)
                baseEnemy.SetEnemyData(data, level);

            baseEnemy.Initialise();

            if (GameManager.Instance != null)
                GameManager.Instance.RegisterEnemy(enemyObj);
        }
    }
}
