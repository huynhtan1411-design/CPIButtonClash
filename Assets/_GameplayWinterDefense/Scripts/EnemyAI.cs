using CLHoma.Combat;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using WD;

public enum EnemyMoveType
{
    Ground,
    Flying
}

public class EnemyAI : MonoBehaviour
{
    [Header("Enemy Type Settings")]
    [SerializeField] private EnemyMoveType enemyType = EnemyMoveType.Ground;
    [SerializeField] private float flyingMoveSpeed = 5f;
    
    [Header("Target Settings")]
    [Tooltip("List of target types in order of priority")]
    public TargetType[] targetPriority;
    [Tooltip("If true, will strictly follow target priority order. If false, will find the closest target of any type")]
    [SerializeField] private bool useStrictPriorityOrder = false;
    [Tooltip("Attack range around the target")]
    public float attackRange = 1.5f;
    [SerializeField] private float targetCheckInterval = 0.5f;
    [SerializeField] private float pathCheckDistance = 30f;
    [SerializeField] private LayerMask wallLayer; // Layer for walls
    [SerializeField] private float raycastHeight = 1f; // Height for raycast checks

    private NavMeshAgent agent;
    private Transform currentTarget;
    private BaseEnemyBehavior enemyBehavior;
    private IDamageable currentDamageableTarget;
    private bool isStopped = false;
    private float nextTargetCheckTime;
    private bool isTargetingWall = false;
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
            // Store current state
            if (enemyType == EnemyMoveType.Ground && agent != null)
            {
                wasMovingBeforePause = !agent.isStopped;
                if (wasMovingBeforePause)
                {
                    lastVelocity = agent.velocity;
                }
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }

            // Stop animations
            enemyBehavior.PlayRunAnimation(false);
        }
        else
        {
            // Resume previous state
            if (wasMovingBeforePause)
            {
                if (enemyType == EnemyMoveType.Ground && agent != null)
                {
                    agent.isStopped = false;
                    agent.velocity = lastVelocity;
                }
                enemyBehavior.PlayRunAnimation(true);
            }
        }
    }

    private void Start()
    {
        enemyBehavior = GetComponent<BaseEnemyBehavior>();
        enemyBehavior.onDeath = () => {
            if (agent != null)
                agent.isStopped = true;
            isStopped = true;
        };
        nextTargetCheckTime = Time.time;

        // Only initialize NavMeshAgent for ground units
        if (enemyType == EnemyMoveType.Ground)
        {
            agent = GetComponent<NavMeshAgent>();
            // Set wall layer mask with validation
            int wallLayerIndex = LayerMask.NameToLayer("Wall");
            if (wallLayerIndex != -1)
            {
                wallLayer = 1 << wallLayerIndex;
            }
            else
            {
                Debug.LogError("Wall layer not found! Please create a layer named 'Wall' in Unity.");
            }
        }
        else
        {
            // Disable NavMeshAgent if it exists on flying units
            var existingAgent = GetComponent<NavMeshAgent>();
            if (existingAgent != null)
            {
                existingAgent.enabled = false;
            }
        }
        
        ChooseTargetAndMove();
    }

    private void Update()
    {
        if (isStopped || GameManager.IsPaused())
            return;

        if (Time.time >= nextTargetCheckTime)
        {
            ChooseTargetAndMove();
            nextTargetCheckTime = Time.time + targetCheckInterval;
        }

        if (currentTarget == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, currentTarget.position);

        // Handle state transitions based on distance
        if (distance <= attackRange)
        {
            // Enter attack state
            StopMovementAndAttack();
        }
        else
        {
            // Enter movement state
            StartMovementToTarget();
        }
    }

    private void StartMovementToTarget()
    {
        if (GameManager.IsPaused())
            return;

        if (enemyType == EnemyMoveType.Ground && agent != null)
        {
            // Ground movement using NavMeshAgent
            agent.isStopped = false;
            agent.SetDestination(currentTarget.position);
        }
        else
        {
            // Flying movement - direct movement towards target
            Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
            transform.position += directionToTarget * flyingMoveSpeed * Time.deltaTime;
            
            // Optional: Smooth rotation towards target
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
        
        enemyBehavior.PlayRunAnimation(true);
    }

    private void StopMovementAndAttack()
    {
        if (GameManager.IsPaused())
            return;

        // Stop movement
        if (enemyType == EnemyMoveType.Ground && agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
        
        // Update animations and rotation
        enemyBehavior.PlayRunAnimation(false);
        
        // Face the target
        Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(directionToTarget);

        // Handle attack
        if (enemyBehavior != null && !enemyBehavior.IsDead)
        {
            enemyBehavior.HandleAttack();
        }
    }

    /// <summary>
    /// Called by BaseEnemyBehavior when it's time to perform an attack
    /// </summary>
    public void PerformAttack()
    {
        if (GameManager.IsPaused())
            return;

        if (currentTarget != null && enemyBehavior != null && currentDamageableTarget != null)
        {
            // Deal damage to the target
            currentDamageableTarget.TakeDamage(enemyBehavior.Stats.Damage.firstValue);
        }
    }

    /// <summary>
    /// Checks if there is a valid path to the target considering walls and obstacles
    /// </summary>
    private bool HasPathToTarget(Vector3 targetPosition)
    {
        NavMeshPath path = new NavMeshPath();
        if (agent.CalculatePath(targetPosition, path))
        {
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                // Check if path is within reasonable distance
                float pathLength = CalculatePathLength(path);
                if (pathLength > pathCheckDistance)
                {
                    return false;
                }

                // Check for walls along the path using raycasts
                return !IsPathBlockedByWalls(path, targetPosition);
            }
        }
        return false;
    }

    /// <summary>
    /// Check if there are any walls blocking the path using raycasts
    /// </summary>
    private bool IsPathBlockedByWalls(NavMeshPath path, Vector3 targetPosition)
    {
        Vector3[] corners = path.corners;
        
        // Raise the raycast start and end points slightly to avoid ground collision
        for (int i = 0; i < corners.Length - 1; i++)
        {
            Vector3 start = corners[i] + Vector3.up * raycastHeight;
            Vector3 end = corners[i + 1] + Vector3.up * raycastHeight;
            Vector3 direction = end - start;
            float distance = direction.magnitude;

            // Debug ray to visualize the path checking
            Debug.DrawLine(start, end, Color.red, targetCheckInterval);

            // Check for walls between path corners
            if (Physics.Raycast(start, direction, distance, wallLayer))
            {
                return true; // Path is blocked
            }
        }

        // Also check direct line to target for the last segment
        if (corners.Length > 0)
        {
            Vector3 finalStart = corners[corners.Length - 1] + Vector3.up * raycastHeight;
            Vector3 finalEnd = targetPosition + Vector3.up * raycastHeight;
            Vector3 finalDirection = finalEnd - finalStart;
            float finalDistance = finalDirection.magnitude;

            Debug.DrawLine(finalStart, finalEnd, Color.yellow, targetCheckInterval);

            if (Physics.Raycast(finalStart, finalDirection, finalDistance, wallLayer))
            {
                return true; // Path is blocked
            }
        }
        return false; // No walls blocking the path
    }

    private float CalculatePathLength(NavMeshPath path)
    {
        float length = 0;
        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            length += Vector3.Distance(path.corners[i], path.corners[i + 1]);
        }
        return length;
    }

    /// <summary>
    /// Finds the nearest wall that blocks path to primary target
    /// </summary>
    private Transform FindNearestBlockingWall(Vector3 primaryTargetPos)
    {
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        Transform nearestWall = null;
        float shortestDistance = float.MaxValue;

        Vector3 startPos = transform.position + Vector3.up * raycastHeight;
        Vector3 targetPos = primaryTargetPos + Vector3.up * raycastHeight;
        Vector3 directionToTarget = (targetPos - startPos).normalized;

        // Debug ray to show the checking direction
        Debug.DrawRay(startPos, directionToTarget * pathCheckDistance, Color.blue, targetCheckInterval);

        foreach (GameObject wall in walls)
        {
            float distanceToWall = Vector3.Distance(transform.position, wall.transform.position);
            
            // Only consider walls that are closer to us than the primary target
            if (distanceToWall < Vector3.Distance(transform.position, primaryTargetPos))
            {
                // Check if this wall is actually in our path
                Vector3 wallPos = wall.transform.position;
                Vector3 directionToWall = (wallPos - transform.position).normalized;
                float dotProduct = Vector3.Dot(directionToTarget, directionToWall);

                // Wall is roughly in the direction of our target (within 90 degrees)
                if (dotProduct > 0)
                {
                    if (distanceToWall < shortestDistance)
                    {
                        // Verify wall is actually blocking with a raycast
                        RaycastHit hit;
                        if (Physics.Raycast(startPos, directionToTarget, out hit, pathCheckDistance, wallLayer))
                        {
                            if (hit.collider.gameObject == wall)
                            {
                                shortestDistance = distanceToWall;
                                nearestWall = wall.transform;
                            }
                        }
                    }
                }
            }
        }

        return nearestWall;
    }

    /// <summary>
    /// Selects the closest target from all available target types
    /// </summary>
    private void ChooseTargetAndMove()
    {
        Transform bestTarget = null;
        IDamageable bestDamageable = null;
        float closestDistance = float.MaxValue;
        Vector3? primaryTargetPos = null; // Store position of primary target for wall detection

        if (useStrictPriorityOrder)
        {
            // Strict priority order: Only choose lower priority targets if no higher priority targets exist
            foreach (TargetType type in targetPriority)
            {
                if (enemyType == EnemyMoveType.Ground && type == TargetType.Wall) continue;

                var candidates = GameObject.FindGameObjectsWithTag(type.ToString());
                if (candidates.Length > 0)
                {
                    // Find the closest target of this type
                    foreach (var candidate in candidates)
                    {
                        float distance = Vector3.Distance(transform.position, candidate.transform.position);
                        var damageable = candidate.GetComponent<IDamageable>();

                        if (damageable != null && (bestTarget == null || distance < closestDistance))
                        {
                            if (enemyType == EnemyMoveType.Ground)
                            {
                                if (HasPathToTarget(candidate.transform.position))
                                {
                                    closestDistance = distance;
                                    bestTarget = candidate.transform;
                                    bestDamageable = damageable;
                                    isTargetingWall = false;
                                }
                            }
                            else
                            {
                                closestDistance = distance;
                                bestTarget = candidate.transform;
                                bestDamageable = damageable;
                                isTargetingWall = false;
                            }
                        }
                    }

                    // If we found a target of this priority, stop searching lower priorities
                    if (bestTarget != null)
                        break;
                }
            }
        }
        else
        {
            // Original behavior: Find closest target regardless of priority
            foreach (TargetType type in targetPriority)
            {
                if (type == TargetType.Wall) continue; // Skip walls initially

                var candidates = GameObject.FindGameObjectsWithTag(type.ToString());
                
                foreach (var candidate in candidates)
                {
                    float distance = Vector3.Distance(transform.position, candidate.transform.position);
                    var damageable = candidate.GetComponent<IDamageable>();
                    
                    if (distance < closestDistance && damageable != null)
                    {
                        // Store the position of the first found target for wall detection
                        if (!primaryTargetPos.HasValue)
                        {
                            primaryTargetPos = candidate.transform.position;
                        }

                        if (enemyType == EnemyMoveType.Ground)
                        {
                            if (HasPathToTarget(candidate.transform.position))
                            {
                                closestDistance = distance;
                                bestTarget = candidate.transform;
                                bestDamageable = damageable;
                                isTargetingWall = false;
                            }
                        }
                        else
                        {
                            closestDistance = distance;
                            bestTarget = candidate.transform;
                            bestDamageable = damageable;
                            isTargetingWall = false;
                        }
                    }
                }
            }

            // For ground units, if no path to target is found and we have a primary target position,
            // look for walls that might be blocking the path
            if (enemyType == EnemyMoveType.Ground && bestTarget == null && primaryTargetPos.HasValue)
            {
                Transform nearestWall = FindNearestBlockingWall(primaryTargetPos.Value);
                if (nearestWall != null)
                {
                    bestTarget = nearestWall;
                    bestDamageable = nearestWall.GetComponent<IDamageable>();
                    isTargetingWall = true;
                }
            }
        }

        // Only ground units need to check for walls if no other target found in strict priority mode
        if (useStrictPriorityOrder && enemyType == EnemyMoveType.Ground && bestTarget == null)
        {
            Transform nearestWall = FindNearestBlockingWall(currentTarget != null ? currentTarget.position : transform.position);
            if (nearestWall != null)
            {
                bestTarget = nearestWall;
                bestDamageable = nearestWall.GetComponent<IDamageable>();
                isTargetingWall = true;
            }
        }

        if (bestTarget != null)
        {
            currentTarget = bestTarget;
            currentDamageableTarget = bestDamageable;
            enemyBehavior.Target = bestTarget;
        }
    }
}
