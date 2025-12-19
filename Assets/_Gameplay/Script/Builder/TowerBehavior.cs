using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using CLHoma.Combat;
using CLHoma;
namespace WD
{
    public class TowerBehavior : BaseBuildingBehaviour, IEnemyDetector
    {
        [SerializeField] protected Transform projectileSpawnPoint;
        [SerializeField] protected EnemyDetector enemyDetector;
        [SerializeField] protected AimRingBehavior aimRingBehavior;
        [SerializeField] protected bool canTargetAirUnits = false; // Whether this tower can target flying enemies
        
        protected int maxTargets = 1; // Maximum number of targets that can be attacked simultaneously
        protected float currentAttackDamage;
        protected float currentAttackRange;
        protected float currentAttackSpeed;
        protected float attackCooldown;
        protected List<BaseEnemyBehavior> currentTargets = new List<BaseEnemyBehavior>(); // List of current targets
        protected GameObject projectilePrefab;
        protected Pool bulletPool;

        private bool wasTrackingBeforePause;
        private Vector3 lastTrackingDirection;

        public override void Initialize(BuildingData config)
        {
            base.Initialize(config);
            InitializeEnemyDetector();
            ApplyLevelStats();

            if (aimRingBehavior != null)
            {
                aimRingBehavior.Init(transform);
            }

            GameManager.OnGamePauseStateChanged += HandlePauseState;
        }

        private void OnDestroy()
        {
            if (buildingGraphics != null)
            {
                buildingGraphics.StopAllRotations();
            }
            
            if (enemyDetector != null && gameObject.activeInHierarchy)
            {
                enemyDetector.ClearZombiesList();
            }

            GameManager.OnGamePauseStateChanged -= HandlePauseState;
        }

        private void HandlePauseState(bool isPaused)
        {
            if (isPaused)
            {
                // Store tracking state
                wasTrackingBeforePause = currentTargets.Count > 0;
                if (wasTrackingBeforePause && buildingGraphics != null)
                {
                    lastTrackingDirection = (currentTargets[0].transform.position - transform.position).normalized;
                    buildingGraphics.StopTrackingTarget();
                }
            }
            else
            {
                // Resume tracking if we were tracking before
                if (wasTrackingBeforePause && currentTargets.Count > 0 && buildingGraphics != null)
                {
                    buildingGraphics.StartTrackingTarget(lastTrackingDirection);
                }
            }
        }

        private void Update()
        {
            if (GameManager.IsPaused())
                return;

            if (attackCooldown > 0)
            {
                attackCooldown -= Time.deltaTime;
            }
            if (enemyDetector == null)
            {
                return;
            }
            // Clean up invalid targets and filter flying enemies
            currentTargets = enemyDetector.GetRandomTargets(maxTargets);
            currentTargets.RemoveAll(target => target == null || 
                                             !target.gameObject.activeInHierarchy || 
                                             target.IsDead ||
                                             (target.IsFlying && !canTargetAirUnits));

            // Attack all current targets when cooldown is ready
            if (currentTargets.Count > 0 && attackCooldown <= 0)
            {
                foreach (var target in currentTargets.ToArray())
                {
                    AttackEnemy(target);
                }
                Audio_Manager.instance.play("sfx_eff_arrow_shoot");
                attackCooldown = currentAttackSpeed;
            }
        }

        protected void InitializeEnemyDetector()
        {
            if (enemyDetector != null)
            {
                enemyDetector.Initialise(this);
                enemyDetector.SetRadius(currentAttackRange);
            }
        }

        public void OnCloseEnemyChanged(BaseEnemyBehavior newTarget)
        {
            if (!currentTargets.Contains(newTarget))
            {
                if (newTarget != null)
                {
                    // Add new target if we haven't reached max targets
                    if (currentTargets.Count < maxTargets)
                    {
                        currentTargets.Add(newTarget);
                        Vector3 direction = (newTarget.transform.position - transform.position).normalized;
                        if (buildingGraphics != null)
                        {
                            buildingGraphics.StartTrackingTarget(direction);
                        }
                    }
                }
            }
            
            // Clean up any null or inactive targets
            currentTargets.RemoveAll(target => target == null || !target.gameObject.activeInHierarchy);
            
            // If no targets remain, stop tracking
            if (currentTargets.Count == 0 && buildingGraphics != null)
            {
                buildingGraphics.StopTrackingTarget();
            }
        }

        protected virtual void ApplyLevelStats()
        {
            var levelData = buildingConfig.GetLevelDataUpgrade(currentLevel);
            //var levelbaseData = buildingConfig.levelConfigs[0];
            //float damageUpgrade = levelData.GetDamageUpgrade(levelbaseData. ttackDamage, buildingCard.Level);
            //float HealtUpgrade = levelData.GetHealthUpgrade(levelbaseData.health, buildingCard.Level);
            currentAttackDamage = levelData.AttackDamage;
            currentAttackRange = levelData.attackRange;
            currentAttackSpeed = levelData.attackSpeed;
            baseDamageableEntity.Initialize(levelData.Health);
            maxTargets = levelData.maxTargets;
            if (enemyDetector != null)
            {
                enemyDetector.SetRadius(currentAttackRange);
            }
            if (aimRingBehavior != null)
            {
                aimRingBehavior.SetRadius(currentAttackRange);
            }
        }

        protected virtual void AttackEnemy(BaseEnemyBehavior enemy)
        {
            if (GameManager.IsPaused() || attackCooldown > 0) 
                return;

            if (buildingGraphics != null)
            {
                Vector3 direction = (enemy.transform.position - transform.position).normalized;
                buildingGraphics.StartTrackingTarget(direction, delegate
                {
                    //ShootProjectile(enemy);
                });
                ShootProjectile(enemy);
            }
        }

        protected virtual void ShootProjectile(BaseEnemyBehavior target)
        {
            if (GameManager.IsPaused() || projectilePrefab == null || projectileSpawnPoint == null) 
                return;

            Vector3 spawnPosition = projectileSpawnPoint.position;
            GameObject projectileObj = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
            projectileObj.transform.LookAt(target.transform.position);
            var projectile = projectileObj.GetComponent<PlayerBulletBehavior>();


            if (projectile != null)
            {
                projectile.Initialise(currentAttackDamage, 20f, ElementType.None, target, 3f, true, 0.1f) ;
            }
        }

        protected override void SpawnModel()
        {
            base.SpawnModel();
            if (buildingGraphics != null)
            {
                projectileSpawnPoint = buildingGraphics.ProjectileSpawnPoint;
                projectilePrefab = buildingGraphics.ProjectilePrefab;
                bulletPool = new Pool(new PoolSettings(projectilePrefab.name, projectilePrefab, 5, true));
            }
        }

        public override void Upgrade()
        {
            base.Upgrade();
            ApplyLevelStats();
        }

        public void ToggleAimRing(bool isActive)
        {
            if (aimRingBehavior != null)
            {
                if (isActive == true)
                    aimRingBehavior.Show();
                else
                    aimRingBehavior.Hide();
            }
        }
    }
} 