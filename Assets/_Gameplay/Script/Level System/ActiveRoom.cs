using CLHoma.Combat;
using DG.Tweening;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
namespace CLHoma.LevelSystem
{
    public static class ActiveRoom
    {
        private static LevelData levelData;
        private static List<BaseEnemyBehavior> enemies;
        private static int enemiesCount;
        private static bool isSpawningEnemies;
        private static CancellationTokenSource cancellationTokenSource;
        private static ObjectPool<BaseEnemyBehavior> enemyPool;

        private static int enemyKillCount;
        private static int goldCollect;
        private static float progressPercentage;

        private static int chapterIndex;

        // Add events
        public static event System.Action<int, int> OnResourceUpdated; // gold, killCount
        public static event System.Action<float> OnProgressUpdated; // progressPercentage
        
        public static int GoldCollect => goldCollect;
        public static float ProgressPercentage => progressPercentage;
        
        public static void Initialise(GameObject levelObject)
        {
            enemies = new List<BaseEnemyBehavior>();
            enemiesCount = 0;
            isSpawningEnemies = false;
            enemyPool = new ObjectPool<BaseEnemyBehavior>(levelObject.transform);
        }

        public static void Setup()
        {
            goldCollect = 0;
            enemyKillCount = 0;
            progressPercentage = 0f;
            OnResourceUpdated?.Invoke(goldCollect, enemyKillCount);
            OnProgressUpdated?.Invoke(progressPercentage);
        }

        public static void SetLevelData(LevelData levelData, int index)
        {
            ActiveRoom.levelData = levelData;
            chapterIndex = index;
        }

        #region SPAWN ENEMY
        private static BaseEnemyBehavior SpawnEnemy(EnemyData enemyData, EnemyType enemyEntityData, bool isActive, float factorPos = 2.5f)
        {
            BaseEnemyBehavior enemy = enemyPool.GetObject(
                enemyData.Prefab.GetComponent<BaseEnemyBehavior>(),
                Utils.GetRandomPositionAroundX(GameManager.GameConfig.SpawnPointEnemy, factorPos),
                Quaternion.identity
            );
            enemy.transform.localScale = Vector3.one;

            enemy.SetEnemyData(enemyData, chapterIndex + 1);
            if (isActive) enemy.Initialise();
            if (!enemies.Contains(enemy))
            {
                enemies.Add(enemy);
                enemiesCount++;
            }
            return enemy;
        }
        private static async Task WaitForSecondsScaled(float seconds, CancellationToken token)
        {
            float elapsed = 0f;
            while (elapsed < seconds && !token.IsCancellationRequested)
            {
                elapsed += Time.deltaTime;
                await Task.Yield();
            }
        }
        public static async Task StartSpawningEnemiesWithScaling(RoundData[] rounds)
        {
            Debug.LogError("Start Spawning Enemies");
            isSpawningEnemies = true;
            int elapsedTime = 0;
            int waveIndex = 0;
            var token = cancellationTokenSource.Token;

            progressPercentage = 0f;
            OnProgressUpdated?.Invoke(progressPercentage);

            int totalLevelTime = (int)(levelData.TotalLevelTime * 0.9f);

            while (elapsedTime < totalLevelTime)
            {
                if (token.IsCancellationRequested) break;

                await WaitForGameUnpause(token);
                if (token.IsCancellationRequested) break;

                var currentRound = GetNextNonBossRound(rounds, ref waveIndex);
                elapsedTime += await SpawnWave(currentRound, elapsedTime, token, totalLevelTime);
                waveIndex++;
                
                float newProgress = Mathf.Clamp01((float)elapsedTime / totalLevelTime);
                if (newProgress != progressPercentage)
                {
                    progressPercentage = newProgress;
                    OnProgressUpdated?.Invoke(progressPercentage);
                }
            }

            if (!token.IsCancellationRequested)
            {
                SpawnEnemyBoss();
                progressPercentage = 1f;
                OnProgressUpdated?.Invoke(progressPercentage);
            }

            isSpawningEnemies = false;
        }

        private static async Task WaitForGameUnpause(CancellationToken token)
        {
            while (GameManager.Instance.IsGamePaused)
            {
                await Task.Delay(1000, token);
                if (token.IsCancellationRequested) break;
            }
        }

        private static async Task<int> SpawnWave(RoundData currentRound, int currentElapsedTime, CancellationToken token, int totalLevelTime)
        {
            int baseEnemyCount = currentRound.EnemiesAmount;
            float enemyIncreaseFactor = (float)currentElapsedTime / totalLevelTime;
            int currentEnemyCount = Mathf.RoundToInt(baseEnemyCount * (1 + enemyIncreaseFactor * 5));
            int totalDelay = 0;

            for (int i = 0; i < currentEnemyCount; i++)
            {
                if (token.IsCancellationRequested) break;

                await WaitForGameUnpause(token);
                if (token.IsCancellationRequested) break;

                int spawnDelay = CalculateSpawnDelay(enemyIncreaseFactor);
                totalDelay += spawnDelay;
                await SpawnEnemyInWave(currentRound, enemyIncreaseFactor, token, spawnDelay);
            }

            int roundDelayMs = CalculateRoundDelay(enemyIncreaseFactor);
            totalDelay += roundDelayMs;
            await WaitForSecondsScaled(roundDelayMs / 1000f, token);

            return totalDelay;
        }

        private static async Task SpawnEnemyInWave(RoundData currentRound, float enemyIncreaseFactor, CancellationToken token, int spawnDelayMs)
        {
            var enemyData = Utils.GetRandomFromList(currentRound.EnemyEntities);
            SpawnEnemy(enemyData, enemyData.EnemyType, true);

            
            await WaitForSecondsScaled(spawnDelayMs / 1000f, token);
        }

        private static RoundData GetNextNonBossRound(RoundData[] rounds, ref int waveIndex)
        {
            var round = rounds[waveIndex % rounds.Length];
            if (round.Type == LevelType.Boss)
            {
                waveIndex++;
                return rounds[waveIndex % rounds.Length];
            }
            return round;
        }

        private static int CalculateSpawnDelay(float increaseFactor)
        {
            int delay = levelData.IntervalEachEnemy;
            return delay / Mathf.RoundToInt(1 + increaseFactor * 2);
        }

        private static int CalculateRoundDelay(float increaseFactor)
        {
            int delay = levelData.IntervalEachRound;
            return delay / Mathf.RoundToInt(1 + increaseFactor * 2);
        }
        private static void SpawnEnemyBoss()
        {
            foreach (var round in levelData.RoundDatas)
            {
                if (round.Type == LevelType.Boss)
                {
                    foreach (var enemy in round.EnemyEntities)
                    {
                        SpawnEnemy(enemy, enemy.EnemyType, true, GameManager.GameConfig.FactorPos);
                    }
                    break;
                }
            }

            UISystems.UIManager.instance.UIGameplayCtr.ToggleUIBossWarning(true);
        }

        public static async Task StartSpawningEnemies(RoundData[] rounds)
        {
            cancellationTokenSource = new CancellationTokenSource();
            await StartSpawningEnemiesWithScaling(rounds);
        }


        #endregion


        #region ENEMY CONTROLLER
        public static void ClearEnemies()
        {
            for (int i = 0; i < enemiesCount; i++)
            {
                if (enemies[i] != null)
                {
                    enemyPool.ReturnObject(enemies[i]);
                }
            }
            enemies.Clear();
            enemiesCount = 0;
            CancelSpawningEnemies();
            
            progressPercentage = 0f;
            OnProgressUpdated?.Invoke(progressPercentage);
        }

        public static List<BaseEnemyBehavior> GetAliveEnemies()
        {
            List<BaseEnemyBehavior> result = new List<BaseEnemyBehavior>();
            for (int i = 0; i < enemiesCount; i++)
            {
                if (!enemies[i].IsDead)
                {
                    result.Add(enemies[i]);
                }
            }
            return result;
        }

        public static bool AreAllEnemiesDead()
        {
            for (int i = 0; i < enemiesCount; i++)
            {
                if (!enemies[i].IsDead)
                {
                    return false;
                }
            }
            return true;
        }

        public static void CheckWinCondition(System.Action action)
        {
            if (!isSpawningEnemies && AreAllEnemiesDead())
            {
                action?.Invoke();
            }
        }

        public static void CancelSpawningEnemies()
        {
            if (isSpawningEnemies)
            {
                cancellationTokenSource.Cancel();
                isSpawningEnemies = false;
            }
        }

        public static void HandleEnemyDied(BaseEnemyBehavior enemy)
        {
            enemyKillCount++;
            int goldAdd = (int)(enemy.Gold * (1 + PlayerController.StatsManager.goldGain));
            //Debug.LogError(PlayerController.StatsManager.goldGain+ " goldGain Add" + goldAdd);
            goldCollect += goldAdd;
            if (enemies.Contains(enemy))
            {
                enemies.Remove(enemy);
                enemiesCount--;

                DOVirtual.DelayedCall(2f, delegate
                {
                    enemyPool.ReturnObject(enemy);
                });
            }
            OnResourceUpdated?.Invoke(goldCollect, enemyKillCount);
        }

        #endregion
    }
}
