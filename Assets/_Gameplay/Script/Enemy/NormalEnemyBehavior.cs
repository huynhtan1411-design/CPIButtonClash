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
            Debug.Log($"{gameObject.name} is performing attack action. To target: {target?.name}");
            if (target == null) return;

            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            Debug.Log($"{gameObject.name} is attempting to attack {target.name} at distance {distanceToTarget}.");
            if (distanceToTarget <= Stats.AttackRange)
            {
                target.TryGetComponent<IDamageable>(out currentDamageableTarget);
                if (currentDamageableTarget != null)
                    currentDamageableTarget.TakeDamage(Stats.Damage.firstValue);
                Debug.Log($"{gameObject.name} attacked {target.name} for {Stats.Damage.firstValue} damage.");
            }
        }
    }
}
