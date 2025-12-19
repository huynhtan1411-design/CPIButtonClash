using UnityEngine;
using CLHoma.Combat;

namespace WD
{
    public class EnemyMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private float waypointReachDistance = 0.1f;
        [SerializeField] private float randomOffset = 0.1f;
        [SerializeField] private bool lookAtNextWaypoint = true;

        private PathMovementWD currentPath;
        private int currentWaypointIndex;
        private Transform[] waypoints;
        private BaseEnemyBehavior enemyBehavior;
        private bool isMoving = true;
        private Vector3 currentRandomOffset;
        private float newOffsetTimer = 0f;
        private const float OFFSET_CHANGE_INTERVAL = 0.5f;

        private void Start()
        {
            enemyBehavior = GetComponent<BaseEnemyBehavior>();
            InitializePath();
        }

        private void InitializePath()
        {
            // Get the nearest path from the PathMovementController
            currentPath = PathMovementController.Instance.GetNearestPath(transform.position);
            if (currentPath != null)
            {
                waypoints = currentPath.Waypoints;
                currentWaypointIndex = 0;
                PathMovementController.Instance.RegisterEnemyOnPath(gameObject, currentPath);

                // Move to first waypoint position
                Vector3 currentRandomOffset = new Vector3(Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f));
                transform.LookAt(waypoints[0].position + currentRandomOffset);
                transform.position = waypoints[0].position + currentRandomOffset;
            }
            else
            {
                Debug.LogWarning("No path found for enemy!");
            }
        }

        private void Update()
        {
            if (!isMoving || waypoints == null || currentWaypointIndex >= waypoints.Length || enemyBehavior.IsDead)
                return;

            MoveAlongPath();
        }

        private void MoveAlongPath()
        {
            Transform currentWaypoint = waypoints[currentWaypointIndex];
            Vector3 directionToWaypoint = (currentWaypoint.position - transform.position).normalized;

            // Update random offset
            newOffsetTimer += Time.deltaTime;
            if (newOffsetTimer >= OFFSET_CHANGE_INTERVAL)
            {
                currentRandomOffset = new Vector3(
                    Random.Range(-randomOffset, randomOffset),
                    0,
                    Random.Range(-randomOffset, randomOffset)
                );
                newOffsetTimer = 0f;
            }

            // Add random offset to movement
            Vector3 offsetDirection = directionToWaypoint + currentRandomOffset;
            offsetDirection.Normalize();

            // Move towards waypoint with offset
            transform.position += offsetDirection * moveSpeed * Time.deltaTime;

            // Rotate towards movement direction
            if (lookAtNextWaypoint)
            {
                Quaternion targetRotation = Quaternion.LookRotation(offsetDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // Check if waypoint is reached
            float distanceToWaypoint = Vector3.Distance(transform.position, currentWaypoint.position);
            if (distanceToWaypoint < waypointReachDistance)
            {
                currentWaypointIndex++;

                // Check if we reached the end of the path
                if (currentWaypointIndex >= waypoints.Length)
                {
                    OnPathComplete();
                }
            }

            // Update animation
            if (enemyBehavior != null)
            {
                enemyBehavior.PlayRunAnimation(isMoving);
            }
        }

        private void OnPathComplete()
        {
            isMoving = false;
            PathMovementController.Instance.UnregisterEnemyFromPath(gameObject, currentPath);

            // You can add additional logic here when the enemy reaches the end of the path
            // For example, dealing damage to the player's base or destroying the enemy
            if (enemyBehavior != null)
            {
                enemyBehavior.PlayAttackAnimation();
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Wall"))
            {
                OnPathComplete();
            }
        }
        

        public void PauseMovement()
        {
            isMoving = false;
            if (enemyBehavior != null)
            {
                enemyBehavior.PlayRunAnimation(false);
            }
        }

        public void ResumeMovement()
        {
            isMoving = true;
            if (enemyBehavior != null)
            {
                enemyBehavior.PlayRunAnimation(true);
            }
        }

        private void OnDestroy()
        {
            if (currentPath != null)
            {
                PathMovementController.Instance.UnregisterEnemyFromPath(gameObject, currentPath);
            }
        }
    }
}