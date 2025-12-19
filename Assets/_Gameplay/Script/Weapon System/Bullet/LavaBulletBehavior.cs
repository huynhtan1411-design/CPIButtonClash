using CLHoma.DGTween;
using UnityEngine;

namespace CLHoma.Combat
{
    public class LavaBulletBehavior : PlayerBulletBehavior
    {
        [SerializeField] private bool isIce = false;
        private readonly static int SPLASH_PARTICLE_HASH = ParticlesController.GetHash("Lava Hit");
        private readonly static int SPLASH_PARTICLE_HASH_2 = ParticlesController.GetHash("Ice Hit");
        private readonly static int WALL_SPLASH_PARTICLE_HASH = ParticlesController.GetHash("Lava Wall Hit");

        [SerializeField] private float explosionRadius;
        [SerializeField] private float damageExplosion;
        private CharacterBehaviour characterBehaviour;

        private TweenCase movementTween;

        private Vector3 position;
        private Vector3 prevPosition;

        public void Initialise(float damage, float speed, ElementType element, BaseEnemyBehavior currentTarget, float autoDisableTime, bool autoDisableOnHit, float explosionRadius, float damageExplosion)
        {
            base.Initialise(damage, speed, element, currentTarget, autoDisableTime, autoDisableOnHit);

            this.explosionRadius = explosionRadius;
            this.damageExplosion = damageExplosion;
            if (!isIce)
                Audio_Manager.instance.play("sfx_eff_fireball_shoot");
            // else
            //    Audio_Manager.instance.play("sfx_eff_frost_shoot");
        }


        private void Update()
        {
            prevPosition = position;
            position = transform.position;
        }


        protected override void OnEnemyHitted(BaseEnemyBehavior baseEnemyBehavior)
        {
            if (movementTween != null && !movementTween.isCompleted)
                movementTween.Kill();

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);

            for (int i = 0; i < hitColliders.Length; i++)
            {
                if (hitColliders[i].gameObject.layer == PhysicsHelper.LAYER_ENEMY)
                {
                    BaseEnemyBehavior enemy = hitColliders[i].GetComponent<BaseEnemyBehavior>();
                    if (enemy != null && !enemy.IsDead)
                    {
                        // Deal damage to enemy
                        enemy.TakeDamage(damageExplosion, transform.position, (transform.position - prevPosition).normalized, HitType.Hit);
                    }
                }
            }

            // Disable projectile
            gameObject.SetActive(false);

            // Spawn splash particle
            if (!isIce)
                ParticlesController.PlayParticle(SPLASH_PARTICLE_HASH).SetPosition(transform.position);
            else
                ParticlesController.PlayParticle(SPLASH_PARTICLE_HASH_2).SetPosition(transform.position);


            if (!isIce)
                Audio_Manager.instance.play("sfx_eff_fireball_explode");
            // else
            //     Audio_Manager.instance.play("sfx_eff_frost_explode");
        }

        protected override void OnObstacleHitted()
        {
            base.OnObstacleHitted();

            ParticlesController.PlayParticle(SPLASH_PARTICLE_HASH).SetPosition(transform.position);

        }
    }
}