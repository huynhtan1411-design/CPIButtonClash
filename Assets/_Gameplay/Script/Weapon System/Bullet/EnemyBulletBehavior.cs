using CLHoma.DGTween;
using UnityEngine;

namespace CLHoma.Combat
{
    public class EnemyBulletBehavior : MonoBehaviour
    {
        private static readonly int PARTICLE_HIT_HASH = ParticlesController.GetHash("Shotgun Hit");
        private static readonly int PARTICLE_WALL_HIT_HASH = ParticlesController.GetHash("Shotgun Wall Hit");

        protected float damage;
        protected float speed;
        protected Transform targetPosition;
        protected float arcHeight = 1f; // Maximum height of the arc

        protected float selfDestroyDistance;
        protected float distanceTraveled = 0;

        protected TweenCase disableTweenCase;
        private Vector3 startPosition;
        private float journeyLength;
        private float startTime;

        public virtual void Initialise(float damage, float speed, float selfDestroyDistance, Transform targetPosition = null)
        {
            this.damage = damage;
            this.speed = speed;
            this.targetPosition = targetPosition;
            this.selfDestroyDistance = selfDestroyDistance;
            
            // Initialize arc trajectory
            startPosition = transform.position;
            startTime = Time.time;
            distanceTraveled = 0;

            if (targetPosition != null)
            {
                journeyLength = Vector3.Distance(startPosition, targetPosition.position);
            }

            gameObject.SetActive(true);
        }

        protected virtual void FixedUpdate()
        {
            if (targetPosition == null)
            {
                SelfDestroy();
                return;
            }

            // Calculate the current position on the arc
            Vector3 targetPos = targetPosition.position;
            float distanceCovered = (Time.time - startTime) * speed;
            float journeyFraction = distanceCovered / journeyLength;

            // Update total distance traveled for self-destroy check
            distanceTraveled = journeyFraction * journeyLength;

            // Clamp the fraction to prevent overshooting
            journeyFraction = Mathf.Clamp01(journeyFraction);

            // Calculate the current position
            Vector3 currentPos = Vector3.Lerp(startPosition, targetPos, journeyFraction);

            // Add arc height using parabolic motion
            float parabola = (1 - 4 * (journeyFraction - 0.5f) * (journeyFraction - 0.5f)); // Parabola formula
            currentPos.y += parabola * arcHeight;

            // Update position
            transform.position = currentPos;

            // Calculate direction for rotation
            if (journeyFraction < 0.99f) // Don't update rotation at the very end
            {
                Vector3 nextPos = Vector3.Lerp(startPosition, targetPos, Mathf.Clamp01(journeyFraction + 0.1f));
                nextPos.y += (1 - 4 * ((journeyFraction + 0.1f) - 0.5f) * ((journeyFraction + 0.1f) - 0.5f)) * arcHeight;
                Vector3 direction = (nextPos - transform.position).normalized;
                transform.forward = direction;
            }

            // Check if we should self-destroy based on distance
            if (selfDestroyDistance != -1 && distanceTraveled >= selfDestroyDistance)
            {
                SelfDestroy();
            }

            // If we've reached the target, destroy the projectile
            if (journeyFraction >= 1)
            {
                SelfDestroy();
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == PhysicsHelper.LAYER_PLAYER)
            {
                PlayerController.StatsManager.TakeDamage(damage);
                SelfDestroy();
                ParticlesController.PlayParticle(PARTICLE_WALL_HIT_HASH).SetPosition(other.gameObject.transform.position);
            }
            else if (other.gameObject.layer == PhysicsHelper.LAYER_OBSTACLE)
            {
                SelfDestroy();
                ParticlesController.PlayParticle(PARTICLE_WALL_HIT_HASH).SetPosition(other.gameObject.transform.position);
            }
        }

        public void SelfDestroy()
        {
            // Disable bullet
            gameObject.SetActive(false);
        }
    }
}