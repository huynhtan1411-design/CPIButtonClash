using UnityEngine;
namespace CLHoma.Combat
{
    public class BoomingEnemyBehavior : BaseEnemyBehavior
    {
        [SerializeField] private ParticleSystem explosionEffect;
        [SerializeField] private float meleeAttackDelay = 0.5f;

        private void Start()
        {
            attackAnimationDelay = meleeAttackDelay;
        }
        //public override void Attack()
        //{
        //    if (target == null)
        //        return;
        //    PlayerController.StatsManager.TakeDamage(GetCurrentDamage());
        //    TakeDamage(CurrentHealth, Vector3.zero, Vector3.zero, HitType.Hit);
        //    explosionEffect.Play();
        //    Audio_Manager.instance.play("sfx_zombie_explode");
        //}

        protected override void PerformAttackAction()
        {
            if (target == null) return;

            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            if (distanceToTarget <= stats.AttackRange)
            {
                var enemyAI = GetComponent<EnemyAI>();
                if (enemyAI != null)
                {
                    ParticlesController.PlayParticle("Lava Hit").SetPosition(transform.position);
                    //explosionEffect.Play();
                    Audio_Manager.instance.play("sfx_zombie_explode");
                    enemyAI.PerformAttack();
                    OnDeath();
                }
            }
        }
    }
}
