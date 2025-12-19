using CLHoma.DGTween;
using UnityEngine;

namespace CLHoma.Combat
{
    public class GeneralBulletBehavior : PlayerBulletBehavior
    {
        private static readonly int PARTICLE_HIT_HASH = ParticlesController.GetHash("Shotgun Hit");
        private static readonly int PARTICLE_WALL_HIT_HASH = ParticlesController.GetHash("Shotgun Wall Hit");


        public override void Initialise(float damage, float speed, ElementType element, BaseEnemyBehavior currentTarget, float autoDisableTime, bool autoDisableOnHit = true, float arcHeight = 1)
        {
            base.Initialise(damage, speed, element, currentTarget, autoDisableTime, autoDisableOnHit);

            //Audio_Manager.instance.play("sfx_eff_arrow_shoot");
        }

        protected override void OnEnemyHitted(BaseEnemyBehavior baseEnemyBehavior)
        {
            ParticlesController.PlayParticle(PARTICLE_HIT_HASH).SetPosition(transform.position);


            //Audio_Manager.instance.play("sfx_eff_flesh_hit");
        }

        protected override void OnObstacleHitted()
        {
            base.OnObstacleHitted();

            ParticlesController.PlayParticle(PARTICLE_WALL_HIT_HASH).SetPosition(transform.position);
        }
    }
}