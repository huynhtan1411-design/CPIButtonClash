using UnityEngine;
using System.Collections.Generic;
using CLHoma.Combat;
using System.Linq;
using WD;
using CLHoma;
using System.Collections;
using DG.Tweening;

namespace TemplateSystems.Controllers.Player
{
    /// <summary>
    /// Handles player's attack behavior, including enemy detection, targeting, and projectile shooting
    /// </summary>
    public class PlayerAttackController : MonoBehaviour
    {
        [Header("Attack Settings")]
        [SerializeField] private float attackRange = 10f;        // Maximum range for detecting enemies
        [SerializeField] private float attackCooldown = 1f;      // Time between attacks
        [SerializeField] private float damage = 10f;             // Damage dealt per attack
        [SerializeField] private LayerMask enemyLayer;           // Layer mask for enemy detection
        [SerializeField] private float minAngleToShoot = 15f;    // Minimum angle difference required to shoot

        [Header("Horse Combat Settings")]
        [SerializeField] private float mountedAttackRange = 10f;     // Increased range while mounted
        [SerializeField] private float mountedAttackCooldown = 1f; // Faster attacks while mounted
        [SerializeField] private float mountedMinAngleToShoot = 25f; // Wider angle for mounted combat

        [Header("Projectile Settings")]
        [SerializeField] private GameObject projectilePrefab;    // Prefab of the projectile to shoot
        [SerializeField] private Transform projectileSpawnPoint; // Point where projectiles will spawn
        [SerializeField] private Transform mountedProjectileSpawnPoint; // Spawn point when mounted

        [Header("References")]
        [SerializeField] private PlayerAnimationController animationController; // Reference to animation controller
        [SerializeField] private PlayerMovementController movementController;   // Reference to movement controller
        [SerializeField] private WDPlayerController playerCtr;
        [Header("Extra Weapons")]
        [SerializeField] private GameObject axeObj;
        private float axeSwingDuration = 0.35f;
        private const float SWING_ANGLE = 170f;
        private const float HALF_SWING_ANGLE = SWING_ANGLE / 2f; 
        private const float SLASH_ANGLE = 15f; 
        private bool isAxeSwinging = false;
        private Sequence axeSwingSequence;

        private float lastAttackTime;                            // Tracks the time of the last attack
        private const string ATTACK_TRIGGER = "Attack";          // Animation trigger parameter name
        private List<BaseEnemyBehavior> enemiesInRange = new List<BaseEnemyBehavior>();
        private BaseEnemyBehavior currentTarget;                 // Current attack target
        private Pool bulletPool;
        private void Start()
        {
            // Auto-find controllers if not assigned
            if (movementController == null)
            {
                movementController = GetComponent<PlayerMovementController>();
            }
            if (animationController == null)
            {
                animationController = GetComponent<PlayerAnimationController>();
            }
            bulletPool = new Pool(new PoolSettings(projectilePrefab.name, projectilePrefab.gameObject, 10, true));
        }

        private void OnEnable()
        {
            WD.GameManager.OnGamePauseStateChanged += HandlePauseState;
        }

        private void OnDisable()
        {
            WD.GameManager.OnGamePauseStateChanged -= HandlePauseState;
        }

        private void HandlePauseState(bool isPaused)
        {
            //Debug.LogError("Pause");
           // enabled = !isPaused;
        }

        private void Update()
        {
            if (WD.GameManager.IsPaused() || movementController.IsReadyToMove)
                return;

            UpdateEnemiesInRange();
            UpdateTargeting();
            TryAttack();
        }

        /// <summary>
        /// Updates the list of enemies within attack range
        /// </summary>
        private void UpdateEnemiesInRange()
        {
            enemiesInRange.Clear();
            float currentRange = animationController.IsOnHorse ? mountedAttackRange : attackRange;
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, currentRange, enemyLayer);

            foreach (var hitCollider in hitColliders)
            {
                var enemy = hitCollider.GetComponent<BaseEnemyBehavior>();
                if (enemy != null && !enemy.IsDead)
                {
                    enemiesInRange.Add(enemy);
                }
            }

            // If current target is not in range or is dead, clear it
            if (currentTarget != null && (!enemiesInRange.Contains(currentTarget) || currentTarget.IsDead))
            {
                currentTarget = null;
                if (movementController != null)
                {
                    movementController.Target = null;
                }
            }
        }

        /// <summary>
        /// Updates the current target and notifies the movement controller
        /// </summary>
        private void UpdateTargeting()
        {
            // Only find new target if we don't have one
            if (currentTarget == null)
            {
                BaseEnemyBehavior nearestEnemy = GetNearestEnemy();
                if (nearestEnemy != currentTarget)
                {
                    currentTarget = nearestEnemy;
                    if (movementController != null)
                    {
                        movementController.Target = currentTarget != null ? currentTarget.transform : null;
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to perform an attack if conditions are met
        /// </summary>
        private void TryAttack()
        {
            if (playerCtr.PlayerHeroEntity.IsDead)
                return;
            float currentCooldown = animationController.IsOnHorse ? mountedAttackCooldown : attackCooldown;
            // Check if we have a valid target and attack is off cooldown
            if (currentTarget == null || Time.time < lastAttackTime + currentCooldown)
            {
                return;
            }
            // If we're facing the target, execute the attack
            if (IsPlayerFacingTarget())
            {
                ExecuteAttack(currentTarget);
            }
        }

        /// <summary>
        /// Returns the closest enemy from the enemies in range
        /// </summary>
        private BaseEnemyBehavior GetNearestEnemy()
        {
            return enemiesInRange
                .OrderBy(enemy => Vector3.Distance(transform.position, enemy.transform.position))
                .FirstOrDefault();
        }

        /// <summary>
        /// Performs the attack action on the target
        /// </summary>
        private void ExecuteAttack(BaseEnemyBehavior target)
        {
            if (WD.GameManager.IsPaused())
                return;

            lastAttackTime = Time.time;

            // Trigger attack animation
            if (animationController != null)
            {
                animationController.PlayAttackAnimation();
            }

            // Create and initialize the projectile
            if (projectilePrefab != null)
            {
                Transform spawnPoint = animationController.IsOnHorse ? mountedProjectileSpawnPoint : projectileSpawnPoint;
                if (spawnPoint == null) spawnPoint = projectileSpawnPoint; // Fallback to default spawn point
                PlayerBulletBehavior projectile = bulletPool
                .GetPooledObject(new PooledObjectSettings()
                .SetPosition(spawnPoint.position))
                //.SetEulerRotation(Vector3.zero))
                .GetComponent<PlayerBulletBehavior>();
                //GameObject projectileObj = Instantiate(projectilePrefab, spawnPoint.position, transform.rotation);
                //PlayerBulletBehavior projectile = projectileObj.GetComponent<PlayerBulletBehavior>();
                projectile.Initialise(damage, -1, ElementType.None, target, animationController.IsOnHorse ? 3 : 2); // More damage when mounted
            }
        }

        /// <summary>
        /// Visualizes the attack range in the Unity Editor
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            // Draw normal attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Draw mounted attack range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, mountedAttackRange);

            // Draw attack angle visualization
            if (Application.isPlaying && currentTarget != null)
            {
                Gizmos.color = IsPlayerFacingTarget() ? Color.green : Color.yellow;
                Vector3 direction = transform.forward * (animationController.IsOnHorse ? mountedAttackRange : attackRange);
                Gizmos.DrawRay(transform.position, direction);
            }
        }

        // Public method to check if we're currently targeting an enemy
        public bool HasTarget()
        {
            return currentTarget != null;
        }

        // Public method to get the current target position
        public Vector3? GetTargetPosition()
        {
            return currentTarget != null ? currentTarget.transform.position : null;
        }

        private bool IsPlayerFacingTarget()
        {
            if (currentTarget == null) return false;

            Vector3 directionToTarget = (currentTarget.transform.position - transform.position).normalized;
            directionToTarget.y = 0;
            
            float currentMinAngle = animationController.IsOnHorse ? mountedMinAngleToShoot : minAngleToShoot;
            float angle = Vector3.Angle(transform.forward, directionToTarget);
            return angle < currentMinAngle;
        }

        public void UseAxe()
        {
            if (axeObj != null && !isAxeSwinging)
            {
                axeObj.SetActive(true);
                isAxeSwinging = true;

                if (axeSwingSequence != null)
                {
                    axeSwingSequence.Kill();
                    axeSwingSequence = null;
                }

                axeSwingSequence = DOTween.Sequence();
                Vector3 originalRotation = axeObj.transform.localEulerAngles;

                axeObj.transform.localEulerAngles = new Vector3(
                    originalRotation.x,
                    originalRotation.y - HALF_SWING_ANGLE,
                    originalRotation.z
                );

                axeSwingSequence.Append(
                    axeObj.transform.DOLocalRotate(
                        new Vector3(originalRotation.x + SLASH_ANGLE, originalRotation.y + HALF_SWING_ANGLE, originalRotation.z),
                        axeSwingDuration / 2
                    ).SetEase(Ease.OutQuad)
                );

                axeSwingSequence.Append(
                    axeObj.transform.DOLocalRotate(
                        originalRotation,
                        axeSwingDuration / 2
                    ).SetEase(Ease.OutQuad)
                );

                axeSwingSequence.OnComplete(() =>
                {
                    isAxeSwinging = false;
                    axeObj.SetActive(false);
                });
            }
        }

        private void OnDestroy()
        {
            if (axeSwingSequence != null)
            {
                axeSwingSequence.Kill();
                axeSwingSequence = null;
            }
        }
    }
} 