using UnityEngine;

namespace TemplateSystems.Battle
{
    public class Projectile : MonoBehaviour
    {
        [Tooltip("Movement speed of the projectile")]
        public float speed = 10f;
        [Tooltip("Damage dealt upon impact")]
        public int damage = 20;

        private Transform target;

        /// <summary>
        /// Instructs the projectile which target to seek
        /// </summary>
        public void Seek(Transform _target)
        {
            target = _target;
        }

        private void Update()
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            var direction = target.position - transform.position;
            float distanceThisFrame = speed * Time.deltaTime;

            if (direction.magnitude <= distanceThisFrame)
            {
                HitTarget();
                return;
            }

            transform.Translate(direction.normalized * distanceThisFrame, Space.World);
        }

        /// <summary>
        /// Handles the impact: apply damage and destroy projectile
        /// </summary>
        private void HitTarget()
        {
            Debug.Log($"Projectile hit {target.name} for {damage} damage.");
            // Example: target.GetComponent<Health>().TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}

