using UnityEngine;
using UnityEngine.Events;

namespace CLHoma.Combat
{
    public abstract class BaseDamageableEntity : MonoBehaviour, IDamageable
    {
        [SerializeField] protected float maxHealth = 100f;
        protected float currentHealth;
        
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsDead { get; protected set; }
        public bool IsFullHealth => currentHealth == maxHealth;

        public UnityEvent onDeath;
        public UnityEvent<float> onHealthChanged;

        protected virtual void Start()
        {
            currentHealth = maxHealth;
            IsDead = false;
            // Trigger initial health update
            onHealthChanged?.Invoke(currentHealth);
        }
        public virtual void Initialize(float initialHealth)
        {
            maxHealth = initialHealth;
            currentHealth = initialHealth;
            IsDead = false;
            onHealthChanged?.Invoke(currentHealth);
        }
        public virtual void TakeDamage(float damage)
        {
            if (IsDead) return;

            currentHealth = Mathf.Max(0, currentHealth - damage);
            
            // Notify UI and other listeners about health change
            onHealthChanged?.Invoke(currentHealth);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public virtual void Die()
        {
            if (IsDead) return;
            
            IsDead = true;
            onDeath?.Invoke();
        }

        public virtual void HealAll()
        {
            currentHealth = maxHealth;
            IsDead = false;
            onHealthChanged?.Invoke(currentHealth);
        }
    }
} 