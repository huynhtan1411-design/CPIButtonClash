using UnityEngine;

namespace WD
{
    public class PathMovementWD : MonoBehaviour
    {
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private Color pathColor = Color.yellow;
        [SerializeField] private bool showGizmos = true;

        public Transform[] Waypoints => waypoints;
        public Transform StartPoint => waypoints.Length > 0 ? waypoints[0] : null;
        public Transform EndPoint => waypoints.Length > 0 ? waypoints[waypoints.Length - 1] : null;

        private void Reset()
        {
            // Get all child transforms
            Transform[] children = GetComponentsInChildren<Transform>();
            // Remove the first transform (which is this object's transform)
            waypoints = new Transform[children.Length - 1];
            for (int i = 0; i < waypoints.Length; i++)
            {
                waypoints[i] = children[i + 1];
            }
        }
        
        private void OnDrawGizmos()
        {
            if (!showGizmos || waypoints == null || waypoints.Length == 0) return;

            Gizmos.color = pathColor;

            // Draw lines between waypoints
            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                if (waypoints[i] != null && waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                    Gizmos.DrawWireSphere(waypoints[i].position, 0.3f);
                }
            }

            // Draw sphere for the last waypoint
            if (waypoints[waypoints.Length - 1] != null)
            {
                Gizmos.DrawWireSphere(waypoints[waypoints.Length - 1].position, 0.3f);
            }
        }

        public float GetDistanceToStart(Vector3 position)
        {
            return Vector3.Distance(position, StartPoint.position);
        }
    }
}