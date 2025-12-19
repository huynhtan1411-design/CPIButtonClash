using System.Collections.Generic;
using UnityEngine;

namespace WD
{
    public class PathMovementController : MonoBehaviour
    {
        private static PathMovementController instance;
        public static PathMovementController Instance => instance;

        [SerializeField] private PathMovementWD[] paths;
        private Dictionary<PathMovementWD, List<GameObject>> enemiesOnPath;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            enemiesOnPath = new Dictionary<PathMovementWD, List<GameObject>>();
            foreach (var path in paths)
            {
                enemiesOnPath[path] = new List<GameObject>();
            }
        }

        public PathMovementWD GetNearestPath(Vector3 position)
        {
            PathMovementWD nearestPath = null;
            float shortestDistance = float.MaxValue;

            foreach (var path in paths)
            {
                float distanceToStart = path.GetDistanceToStart(position);
                if (distanceToStart < shortestDistance)
                {
                    shortestDistance = distanceToStart;
                    nearestPath = path;
                }
            }

            return nearestPath;
        }

        public void RegisterEnemyOnPath(GameObject enemy, PathMovementWD path)
        {
            if (enemiesOnPath.ContainsKey(path))
            {
                enemiesOnPath[path].Add(enemy);
            }
        }

        public void UnregisterEnemyFromPath(GameObject enemy, PathMovementWD path)
        {
            if (enemiesOnPath.ContainsKey(path))
            {
                enemiesOnPath[path].Remove(enemy);
            }
        }

        public List<GameObject> GetEnemiesOnPath(PathMovementWD path)
        {
            if (enemiesOnPath.ContainsKey(path))
            {
                return enemiesOnPath[path];
            }
            return new List<GameObject>();
        }
    }
}