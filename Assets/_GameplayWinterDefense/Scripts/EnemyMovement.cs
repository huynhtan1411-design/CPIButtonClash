using UnityEngine;
using CLHoma.Combat;

namespace WD
{
    public class EnemyMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 6f;
        [SerializeField] private float waypointReachDistance = 0.1f;
        [SerializeField] private float randomOffset = 0.1f;
        [SerializeField] private bool lookAtNextWaypoint = true;

        [Header("Target Tracking")]
        [SerializeField] private float detectRadius = 5f;
        [SerializeField] private float targetTrackingFrequency = 0.5f;

        private float nextScanTime;

        private PathMovementWD currentPath;
        private Transform[] waypoints;
        private int currentWaypointIndex;

        private BaseEnemyBehavior enemyBehavior;
        private bool isMoving = true;

        private Vector3 currentRandomOffset;
        private float offsetTimer;
        private const float OFFSET_CHANGE_INTERVAL = 0.5f;

        // =========================
        // INIT
        // =========================
        private void Start()
        {
            enemyBehavior = GetComponent<BaseEnemyBehavior>();
            InitializePath();
        }

        private void InitializePath()
        {
            currentPath = PathMovementController.Instance.GetNearestPath(transform.position);
            if (currentPath == null)
            {
                Debug.LogWarning("No path found for enemy!");
                return;
            }

            waypoints = currentPath.Waypoints;
            currentWaypointIndex = 0;
            PathMovementController.Instance.RegisterEnemyOnPath(gameObject, currentPath);

            Vector3 spawnOffset = new Vector3(
                Random.Range(-randomOffset, randomOffset),
                0,
                Random.Range(-randomOffset, randomOffset)
            );

            transform.position = waypoints[0].position + spawnOffset;
            transform.LookAt(waypoints[0].position);
        }

        // =========================
        // UPDATE
        // =========================
        private float lastAttackTime = 0f;

        private void Update()
        {
            if (enemyBehavior.IsDead)
                return;

            // Periodic target scan
            if (Time.time >= nextScanTime)
            {
                nextScanTime = Time.time + targetTrackingFrequency;
                UpdateTarget();
            }

            // ATTACK MODE
            if (enemyBehavior.Target != null)
            {
                HandleAttack();
                lastAttackTime = Time.time;
                return;
            }
            if(lastAttackTime + 0.8f > Time.time)
            {
                return;
            }   
            // MOVE MODE
            if (!isMoving || waypoints == null || currentWaypointIndex >= waypoints.Length )
                return;

            MoveAlongPath();
        }

        // =========================
        // TARGETING
        // =========================
        private void UpdateTarget()
        {
            // Lose invalid target
            if (enemyBehavior.Target != null)
            {
                float dist = Vector3.Distance(transform.position, enemyBehavior.Target.position);
                if (dist > detectRadius || !enemyBehavior.Target.gameObject.activeInHierarchy)
                    enemyBehavior.Target = null;
            }

            if (enemyBehavior.Target != null)
                return;

            Collider[] hits = Physics.OverlapSphere(
                transform.position,
                detectRadius
            );

            float closestDist = float.MaxValue;
            Transform closest = null;

            foreach (var hit in hits)
            {
                if(hit.transform.CompareTag("Tower") == false)
                    continue;
                float dist = (hit.transform.position - transform.position).sqrMagnitude;
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = hit.transform;
                }
            }

            enemyBehavior.Target = closest;
        }

        // =========================
        // ATTACK
        // =========================
        private void HandleAttack()
        {
            Vector3 dir = enemyBehavior.Target.position - transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion lookRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    lookRot,
                    rotationSpeed * Time.deltaTime
                );
            }

            enemyBehavior.HandleAttack();
        }

        // =========================
        // MOVEMENT
        // =========================
        private void MoveAlongPath()
        {
            Transform waypoint = waypoints[currentWaypointIndex];

            offsetTimer += Time.deltaTime;
            if (offsetTimer >= OFFSET_CHANGE_INTERVAL)
            {
                currentRandomOffset = new Vector3(
                    Random.Range(-randomOffset, randomOffset),
                    0,
                    Random.Range(-randomOffset, randomOffset)
                );
                offsetTimer = 0f;
            }

            Vector3 dir = (waypoint.position - transform.position).normalized;
            Vector3 moveDir = (dir + currentRandomOffset).normalized;

            transform.position += moveDir * moveSpeed * Time.deltaTime;

            if (lookAtNextWaypoint)
            {
                Quaternion rot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    rot,
                    rotationSpeed * Time.deltaTime
                );
            }

            if (Vector3.Distance(transform.position, waypoint.position) < waypointReachDistance)
            {
                currentWaypointIndex++;
                if (currentWaypointIndex >= waypoints.Length)
                    OnPathComplete();
            }

            enemyBehavior.PlayRunAnimation(true);
        }

        // =========================
        // PATH END
        // =========================
        private void OnPathComplete()
        {
            isMoving = false;

            if (currentPath != null)
                PathMovementController.Instance.UnregisterEnemyFromPath(gameObject, currentPath);

            enemyBehavior.PlayAttackAnimation();
        }

        private void OnDestroy()
        {
            if (currentPath != null)
                PathMovementController.Instance.UnregisterEnemyFromPath(gameObject, currentPath);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectRadius);
        }
#endif
    }
}
