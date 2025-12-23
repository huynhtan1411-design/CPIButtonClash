using UnityEngine;

namespace CLHoma.Combat
{
    public class NormalEnemyBehavior : BaseEnemyBehavior
    {
        [SerializeField] private float meleeAttackDelay = 0.5f;
        private IDamageable currentDamageableTarget;

        private void Start()
        {
            attackAnimationDelay = meleeAttackDelay;
        }
        protected override void PerformAttackAction()
        {
            if (target == null) return;
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            if (distanceToTarget <= Stats.AttackRange)
            {
                target.TryGetComponent<IDamageable>(out currentDamageableTarget);
                if (currentDamageableTarget != null)
                    currentDamageableTarget.TakeDamage(Stats.Damage.firstValue);
            }
        }
    }
}
