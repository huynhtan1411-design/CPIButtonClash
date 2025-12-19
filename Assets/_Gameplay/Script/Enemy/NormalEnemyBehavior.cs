using UnityEngine;

namespace CLHoma.Combat
{
    public class NormalEnemyBehavior : BaseEnemyBehavior
    {
        [SerializeField] private float meleeAttackDelay = 0.5f; 

        private void Start()
        {
            attackAnimationDelay = meleeAttackDelay; 
        }

        protected override void PerformAttackAction()
        {
            if (target == null) return;

            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            if (distanceToTarget <= stats.AttackRange)
            {
                var enemyAI = GetComponent<EnemyAI>();
                if (enemyAI != null)
                {
                    enemyAI.PerformAttack();          
                }
            }
        }
    }
}
