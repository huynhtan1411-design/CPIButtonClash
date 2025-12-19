using CLHoma.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using WD;

public class AllyAI : MonoBehaviour
{
    [Tooltip("Attack range around the target")]
    public float attackRange = 1.5f;
    [SerializeField] private float targetCheckInterval = 0.5f;
    [SerializeField] private float detectionRange = 10f; // Range to detect enemies

    private float separationRadius = 0.1f; // Minimum distance between allies
    private NavMeshAgent agent;
    private Transform currentTarget;
    private BaseAllyBehavior allyBehavior;
    private IHealth currentHealthTarget;
    private bool isStopped = false;
    private float nextTargetCheckTime;
    private Vector3 lastVelocity;
    private bool wasMovingBeforePause;

    private void OnEnable()
    {
        GameManager.OnGamePauseStateChanged += HandlePauseState;
    }

    private void OnDisable()
    {
        GameManager.OnGamePauseStateChanged -= HandlePauseState;
    }

    private void HandlePauseState(bool isPaused)
    {
        if (isPaused)
        {
            wasMovingBeforePause = !agent.isStopped;
            if (wasMovingBeforePause)
            {
                lastVelocity = agent.velocity;
            }
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            allyBehavior.PlayRunAnimation(false);
        }
        else
        {
            if (wasMovingBeforePause)
            {
                agent.isStopped = false;
                agent.velocity = lastVelocity;
                allyBehavior.PlayRunAnimation(true);
            }
        }
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        allyBehavior = GetComponent<BaseAllyBehavior>();
        
        if (agent == null)
        {
            return;
        }
        
        if (allyBehavior == null)
        {
            return;
        }

        // Configure NavMeshAgent for proper obstacle avoidance
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.avoidancePriority = 50;
        
        // Initialize NavMeshAgent settings from ally stats
        if (allyBehavior.Stats != null)
        {
            agent.speed = allyBehavior.Stats.MoveSpeed;
            attackRange = allyBehavior.Stats.AttackRange;
            detectionRange = allyBehavior.Stats.DetectionRange;
        }

        allyBehavior.onDeath = () => {
            agent.isStopped = true;
            agent.ResetPath();
            isStopped = true;
        };
        nextTargetCheckTime = Time.time;
        
    }

    private void Update()
    {
        if (isStopped || GameManager.IsPaused())
            return;

        if (Time.time >= nextTargetCheckTime)
        {
            FindAndTargetNearestEnemy();
            ApplySeparationBehavior();
            nextTargetCheckTime = Time.time + targetCheckInterval;
        }

        if (currentTarget == null)
        {
            allyBehavior.PlayRunAnimation(false);
            return;
        }

        float distance = Vector3.Distance(transform.position, currentTarget.position);

        if (distance <= attackRange)
        {
            StopMovementAndAttack();
        }
        else
        {
            StartMovementToTarget();
        }
    }

    private void StopMovementAndAttack()
    {
        if (GameManager.IsPaused())
            return;

        agent.isStopped = true;
        agent.ResetPath();
        allyBehavior.PlayRunAnimation(false);
        
        Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(directionToTarget);

        if (allyBehavior != null && !allyBehavior.IsDead)
        {
            allyBehavior.HandleAttack();
        }
    }

    private void StartMovementToTarget()
    {
        if (GameManager.IsPaused())
            return;

        if (allyBehavior != null)
        {
            allyBehavior.StopAttackAnimation();
        }

        agent.isStopped = false;
        agent.SetDestination(currentTarget.position);
        allyBehavior.PlayRunAnimation(true);
    }

    public void PerformAttack()
    {
        if (GameManager.IsPaused())
            return;

        if (currentTarget != null && allyBehavior != null && currentHealthTarget != null)
        {
            currentHealthTarget.TakeDamage(allyBehavior.Stats.Damage.firstValue, transform.position, (currentTarget.position - transform.position).normalized, HitType.Hit);
        }
    }

    public void UpdateSettings()
    {
        if (agent != null && allyBehavior != null && allyBehavior.Stats != null)
        {
            agent.speed = allyBehavior.Stats.MoveSpeed;
            attackRange = allyBehavior.Stats.AttackRange;
            detectionRange = allyBehavior.Stats.DetectionRange;
            
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            agent.avoidancePriority = 50;
        }
    }

    private void FindAndTargetNearestEnemy()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRange);
        Transform nearestEnemy = null;
        IHealth nearestHealth = null;
        float nearestDistance = float.MaxValue;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                var enemy = hitCollider.gameObject;
                var health = enemy.GetComponent<IHealth>();
                
                if (health != null)
                {
                    float distance = Vector3.Distance(transform.position, enemy.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestEnemy = enemy.transform;
                        nearestHealth = health;
                    }
                }
            }
        }

        if (nearestEnemy != null)
        {
            currentTarget = nearestEnemy;
            currentHealthTarget = nearestHealth;
            allyBehavior.Target = nearestEnemy;
        }
        else
        {
            currentTarget = null;
            currentHealthTarget = null;
            allyBehavior.Target = null;
        }
    }

    private void ApplySeparationBehavior()
    {
        if (!agent.isOnNavMesh || agent.isStopped)
            return;

        Collider[] nearbyAllies = Physics.OverlapSphere(transform.position, separationRadius);
        Vector3 separationMove = Vector3.zero;
        int neighborCount = 0;

        foreach (var collider in nearbyAllies)
        {
            if (collider.gameObject != gameObject && collider.GetComponent<AllyAI>() != null)
            {
                Vector3 moveDirection = transform.position - collider.transform.position;
                float distance = moveDirection.magnitude;
                
                if (distance < separationRadius)
                {
                    separationMove += moveDirection.normalized / distance;
                    neighborCount++;
                }
            }
        }

        if (neighborCount > 0)
        {
            separationMove /= neighborCount;
            Vector3 targetPosition = transform.position + separationMove.normalized;
            
            if (currentTarget != null)
            {
                // Blend between separation and target following
                targetPosition = Vector3.Lerp(currentTarget.position, targetPosition, 0.3f);
            }
            
            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        }    
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        }
    }
} 