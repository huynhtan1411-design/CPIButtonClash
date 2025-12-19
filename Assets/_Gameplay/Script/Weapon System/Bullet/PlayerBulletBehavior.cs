using CLHoma.DGTween;
using UnityEngine;
using WD;

namespace CLHoma.Combat
{
    // base class for player bullets
    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    public abstract class PlayerBulletBehavior : MonoBehaviour
    {
        [SerializeField] protected float damage;
        [SerializeField] protected float speed;
        [SerializeField] protected float arcHeight = 1f; // Maximum height of the arc
        private bool autoDisableOnHit;

        private TweenCase disableTweenCase;
        private Vector3 arcStartPosition;
        private float arcJourneyLength;
        private float arcStartTime;

        protected BaseEnemyBehavior currentTarget;
        protected ElementType elementType;
        protected bool isHoming;
        protected Vector3 direction;
        private static readonly int PARTICLE_HIT_HASH = ParticlesController.GetHash("Shotgun Wall Hit");

        private Vector3 originPos;
        public virtual void Initialise(float damage, float speed, ElementType element, BaseEnemyBehavior currentTarget, float autoDisableTime = 0, bool autoDisableOnHit = true,float arcHeight =1)
        {
            if(damage != -1)
                this.damage = damage;
            if (speed != -1)
                this.speed = speed;
            this.autoDisableOnHit = autoDisableOnHit;
            this.arcHeight = arcHeight;
            this.currentTarget = currentTarget;
            this.elementType = element;
            
            // Initialize arc trajectory
            arcStartPosition = transform.position;
            arcStartTime = Time.time;
            if (currentTarget != null)
            {
                arcJourneyLength = Vector3.Distance(arcStartPosition, currentTarget.transform.position);
            }

            if (autoDisableTime > 0)
            {
                disableTweenCase = Tween.DelayedCall(autoDisableTime, delegate
                {
                    // Disable bullet
                    gameObject.SetActive(false);
                });
            }

            originPos = currentTarget.transform.position;
        }

        protected virtual void FixedUpdate()
        {
            if (currentTarget == null)
            {
                Destroy(gameObject);
                return;
            }

            // Calculate the current position on the arc
            Vector3 targetPos = currentTarget.transform.position;
            float distanceCovered = (Time.time - arcStartTime) * speed;
            float journeyFraction = distanceCovered / arcJourneyLength;

            // Clamp the fraction to prevent overshooting
            journeyFraction = Mathf.Clamp01(journeyFraction);

            // Calculate the current position
            Vector3 currentPos = Vector3.Lerp(arcStartPosition, targetPos, journeyFraction);

            // Modify the arc height calculation for a more pronounced arc
            float heightMultiplier = Mathf.Sin(journeyFraction * Mathf.PI); // Creates a smoother arc
            currentPos.y += heightMultiplier * arcHeight;

            // Update position
            transform.position = currentPos;

            // Calculate direction for rotation
            if (journeyFraction < 0.99f)
            {
                // Calculate next position for smooth rotation
                float nextFraction = Mathf.Clamp01(journeyFraction + 0.1f);
                Vector3 nextPos = Vector3.Lerp(arcStartPosition, targetPos, nextFraction);
                float nextHeightMultiplier = Mathf.Sin(nextFraction * Mathf.PI);
                nextPos.y += nextHeightMultiplier * arcHeight;
                
                direction = (nextPos - transform.position).normalized;
                
                // Apply rotation with smoother interpolation
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
            }

            // If we've reached the target, destroy the projectile
            if (journeyFraction >= 1)
            {
                OnObstacleHitted();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == PhysicsHelper.LAYER_ENEMY)
            {
                BaseEnemyBehavior baseEnemyBehavior = other.GetComponent<BaseEnemyBehavior>();
                if (baseEnemyBehavior != null)
                {
                    if (!baseEnemyBehavior.IsDead)
                    {
                        if (disableTweenCase != null && !disableTweenCase.isCompleted)
                            disableTweenCase.Kill();

                        // Disable bullet
                        if (autoDisableOnHit)
                            gameObject.SetActive(false);
                        (float damage, HitType hitType) damageAfter = Utils.CalculateElementalDamage(damage, elementType, baseEnemyBehavior.ElementType, WD.GameManager.Instance.GameConfig.resistanceFactor, WD.GameManager.Instance.GameConfig.weaknessFactor);
                        baseEnemyBehavior.TakeDamage(damageAfter.damage, transform.position, transform.forward, damageAfter.hitType);
                        OnEnemyHitted(baseEnemyBehavior);
                    }
                }
            }
            else
            {
                //OnObstacleHitted();
            }
        }

        private void OnDisable()
        {
            if (disableTweenCase != null && !disableTweenCase.isCompleted)
                disableTweenCase.Kill();
        }

        private void OnDestroy()
        {
            if (disableTweenCase != null && !disableTweenCase.isCompleted)
                disableTweenCase.Kill();
        }

        protected virtual void OnEnemyHitted(BaseEnemyBehavior baseEnemyBehavior)
        {
            Audio_Manager.instance.play("sfx_eff_flesh_hit");
            ParticlesController.PlayParticle(PARTICLE_HIT_HASH).SetPosition(transform.position);
        }

        protected virtual void OnObstacleHitted()
        {
            if (disableTweenCase != null && !disableTweenCase.isCompleted)
                disableTweenCase.Kill();

            gameObject.SetActive(false);
        }
    }
}
