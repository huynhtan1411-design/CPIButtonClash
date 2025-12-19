using UnityEngine;

namespace CLHoma.Combat
{
    public interface IDamageable
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }
        bool IsDead { get; }
        void TakeDamage(float damage);
        void Die();
    }
} 