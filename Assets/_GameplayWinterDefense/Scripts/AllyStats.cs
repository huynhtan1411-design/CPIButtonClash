using UnityEngine;
using CLHoma.Combat;

namespace WD
{
    [System.Serializable]
    public class AllyStats
    {
        [Header("Combat Stats")]
        public StatValue Health = new StatValue(100f);
        public StatValue Damage = new StatValue(10f);
        public float AttackCooldown = 1f;
        public float AttackRange = 1.5f;
        public float MoveSpeed = 5f;

        [Header("Combat Settings")]
        public float DetectionRange = 10f;
        public float AttackDelay = 0.5f;

        public AllyStats()
        {
            // Default constructor
        }

        public AllyStats(float health, float damage, float attackCooldown, float attackRange, float moveSpeed)
        {
            Health = new StatValue(health);
            Damage = new StatValue(damage);
            AttackCooldown = attackCooldown;
            AttackRange = attackRange;
            MoveSpeed = moveSpeed;
        }

        // Deep copy constructor
        public AllyStats(AllyStats other)
        {
            if (other == null) return;

            Health = new StatValue(other.Health.firstValue);
            Damage = new StatValue(other.Damage.firstValue);
            AttackCooldown = other.AttackCooldown;
            AttackRange = other.AttackRange;
            MoveSpeed = other.MoveSpeed;
            DetectionRange = other.DetectionRange;
            AttackDelay = other.AttackDelay;
        }
    }
} 