using UnityEngine;

namespace CLHoma.Combat
{
    public class BoomerangBulletBehavior : Boomerang
    {
        private static readonly int PARTICLE_HIT_HASH =
            ParticlesController.GetHash("Shotgun Wall Hit");

        [SerializeField] TrailRenderer trailRenderer;

        public override void Initialise(
            float damage,
            float speed,
            ElementType element,
            Vector3 castDirection,
            float sideSign
        )
        {
            base.Initialise(damage, speed, element, castDirection, sideSign);

            trailRenderer?.Clear();
            Audio_Manager.instance.play("sfx_eff_arrow_shoot");
        }

        protected override void OnEnemyHitted(BaseEnemyBehavior enemy)
        {
            ParticlesController
                .PlayParticle(PARTICLE_HIT_HASH)
                .SetPosition(transform.position);
        }
    }
}
