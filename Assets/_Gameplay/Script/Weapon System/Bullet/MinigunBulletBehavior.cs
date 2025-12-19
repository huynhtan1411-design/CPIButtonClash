using UnityEngine;

namespace CLHoma.Combat
{
    public class MinigunBulletBehavior : PlayerBulletBehavior
    {
        private static readonly int PARTICLE_HIT_HASH = ParticlesController.GetHash("Shotgun Wall Hit");
        private static readonly int PARTICLE_WAll_HIT_HASH = ParticlesController.GetHash("Minigun Wall Hit");

        [SerializeField] TrailRenderer trailRenderer;

        public override void Initialise(float damage, float speed, ElementType element, BaseEnemyBehavior currentTarget, float autoDisableTime, bool autoDisableOnHit = true, float arcHeight = 1)
        {
            base.Initialise(damage, speed, element, currentTarget, autoDisableTime, autoDisableOnHit, arcHeight);

            trailRenderer.Clear();


            Audio_Manager.instance.play("sfx_eff_arrow_shoot");
        }

        protected override void OnEnemyHitted(BaseEnemyBehavior baseEnemyBehavior)
        {
            ParticlesController.PlayParticle(PARTICLE_HIT_HASH).SetPosition(transform.position);

            trailRenderer.Clear();
        }

        protected override void OnObstacleHitted()
        {
            base.OnObstacleHitted();

            //ParticlesController.PlayParticle(PARTICLE_WAll_HIT_HASH).SetPosition(transform.position);
            trailRenderer.Clear();
        }
    }
}