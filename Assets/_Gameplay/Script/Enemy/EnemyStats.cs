using UnityEngine;

namespace CLHoma.Combat
{
    [System.Serializable]
    public class EnemyStats
    {
        [SerializeField] float hp;
        public float Hp => hp;

        [SerializeField] float moveSpeed;
        public float MoveSpeed => moveSpeed;

        [SerializeField] DuoInt damage;
        public DuoInt Damage => damage;

        [SerializeField] int gold;
        public int Gold => gold;

        [SerializeField] float attackRange;
        public float AttackRange => attackRange;

        [SerializeField] float attackCooldown;
        public float AttackCooldown => attackCooldown;
    }

}