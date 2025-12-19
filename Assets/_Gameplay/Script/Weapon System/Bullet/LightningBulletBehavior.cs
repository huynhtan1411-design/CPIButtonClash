using CLHoma.DGTween;
using DG.Tweening;
using UnityEngine;

namespace CLHoma.Combat
{
    public class LightningBulletBehavior : PlayerBulletBehavior
    {
        [SerializeField] ParticleSystem particle;
        private float explosionRadius;
        private float explosiondamage;

        private TweenCase movementTween;

        private Vector3 prevPosition;

        public void Initialise(float damage, float speed, ElementType element, BaseEnemyBehavior currentTarget, float autoDisableTime, bool autoDisableOnHit, float explosionRadius, float damageExplosion)
        {
            base.Initialise(damage, speed, element, currentTarget, autoDisableTime, autoDisableOnHit);

            this.explosionRadius = explosionRadius;
            this.explosiondamage = damageExplosion;
            if (currentTarget == null)
            {
                gameObject.SetActive(false);
                return;
            }
            Vector3 targetPosition = currentTarget.transform.position;
            this.damage = damage;
            transform.position = targetPosition;
            transform.rotation = Quaternion.identity;
            particle.Play();
        }
        protected override void FixedUpdate()
        {
            if (currentTarget == null)
            {
                Destroy(gameObject);
                return;
            }
        }
        protected override void OnEnemyHitted(BaseEnemyBehavior baseEnemyBehavior)
        {

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);

            for (int i = 0; i < hitColliders.Length; i++)
            {
                if (hitColliders[i].gameObject.layer == PhysicsHelper.LAYER_ENEMY)
                {
                    BaseEnemyBehavior enemy = hitColliders[i].GetComponent<BaseEnemyBehavior>();
                    if (enemy != null && !enemy.IsDead)
                    {
                        enemy.TakeDamage(explosiondamage, transform.position, (transform.position - prevPosition).normalized,HitType.Hit);
                    }
                }
            }
            DOVirtual.DelayedCall(1f, delegate
            {
                gameObject.SetActive(false);
            });
        }
    }
}