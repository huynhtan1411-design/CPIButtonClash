using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using CLHoma.Combat;
using UnityEngine.UI;
namespace WD
{
    public class BarrackBuildingBehaviour : BaseBuildingBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject allyPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float[] spawnInterval;
        [SerializeField] private int[] maxAlliesPerWave;
        [SerializeField] private float spawnRadius = 2f;
        [SerializeField] private Image trainingSlider;
        [Header("Ally Stats")]
        [SerializeField] private AllyStats allyStats;

        private List<BaseAllyBehavior> activeAllies = new List<BaseAllyBehavior>();
        private bool isSpawning = false;
        private Coroutine spawnCoroutine;

        public override void Initialize(BuildingData config)
        {
            base.Initialize(config);
            UpdateAllyStats();

            trainingSlider.fillAmount = 0f;
            StartSpawning();
            GameManager.Instance.OnBuildPhaseStart.AddListener(() => {
                StopSpawning();
            });
        }
        protected override void OnDestroyBuilding()
        {
            base.OnDestroyBuilding();
            StopSpawning();
        }

        private void UpdateAllyStats()
        {
            // Update ally stats based on building level
            var levelConfig = buildingConfig.levelConfigs[currentLevel - 1];
            
            allyStats.Health.SetBaseValue(levelConfig.Health);
            allyStats.AttackCooldown = levelConfig.attackSpeed;
        }

        public override void Upgrade()
        {
            base.Upgrade();
            UpdateAllyStats();

            // Upgrade existing allies
            foreach (var ally in activeAllies)
            {
                if (ally != null && !ally.IsDead)
                {
                    ally.SetAllyData(new AllyStats(allyStats));
                }
            }
        }

        private void StartSpawning()
        {
            if (!isSpawning)
            {
                isSpawning = true;
                for (int i = 0; i < maxAlliesPerWave[currentLevel - 1]; i++)
                {
                    SpawnAlly();
                }

                spawnCoroutine = StartCoroutine(SpawnRoutine());
            }
        }

        private void StopSpawning()
        {
            if (isSpawning)
            {
                isSpawning = false;
                if (spawnCoroutine != null)
                {
                    StopCoroutine(spawnCoroutine);
                }
            }
            
            // Remove all allies when build phase starts
            RemoveAllAllies();
            trainingSlider.fillAmount = 0f;
        }

        private void RemoveAllAllies()
        {
            for (int i = activeAllies.Count - 1; i >= 0; i--)
            {
                if (activeAllies[i] != null)
                {
                    Destroy(activeAllies[i].gameObject);
                }
            }
            activeAllies.Clear();
        }

        private IEnumerator SpawnRoutine()
        {
            while (isSpawning)
            {
                activeAllies.RemoveAll(ally => ally == null || ally.IsDead);

                if (activeAllies.Count >= maxAlliesPerWave[currentLevel - 1])
                {
                    if (trainingSlider != null)
                    {
                        trainingSlider.fillAmount = 0f;
                    }
                    yield return new WaitForSeconds(0.5f);
                    continue;
                }

                if (trainingSlider != null)
                {
                    trainingSlider.fillAmount = 0f;
                }

                float elapsedTime = 0f;
                float interval = spawnInterval[currentLevel - 1];

                while (elapsedTime < interval)
                {
                    if (trainingSlider != null)
                    {
                        trainingSlider.fillAmount = elapsedTime / interval;
                    }
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                // Set slider to full when training completes
                if (trainingSlider != null)
                {
                    trainingSlider.fillAmount = 1f;
                }

                SpawnAlly();
            }
        }

        private void SpawnAlly()
        {
            Vector3 spawnPosition = spawnPoint.position + Random.insideUnitSphere * spawnRadius;
            spawnPosition.y = spawnPoint.position.y;
            GameObject allyObject = Instantiate(allyPrefab, spawnPosition, Quaternion.identity);
            BaseAllyBehavior ally = allyObject.GetComponent<BaseAllyBehavior>();

            if (ally != null)
            {
                // Initialize ally with stats
                ally.SetAllyData(new AllyStats(allyStats));
                ally.Initialize();

                // Update AllyAI settings after stats are set
                AllyAI allyAI = allyObject.GetComponent<AllyAI>();
                if (allyAI != null)
                {
                    allyAI.UpdateSettings();
                }

                activeAllies.Add(ally);
            }
        }


        private void OnDestroy()
        {
            StopSpawning();
        }
    }
}