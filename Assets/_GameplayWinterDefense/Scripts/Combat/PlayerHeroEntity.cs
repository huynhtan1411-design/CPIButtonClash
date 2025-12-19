using UISystems;
using UnityEngine;

namespace CLHoma.Combat
{
    public class PlayerHeroEntity : BaseDamageableEntity
    {
        [SerializeField] private GameObject gameOverUI;
        [SerializeField] private float invulnerabilityTime = 0.5f;
        [SerializeField] private HealthBarUI healthBarUI;
        private float lastDamageTime;
        private WDPlayerController playerController;

        protected override void Start()
        {
            base.Start();
            
            if (healthBarUI != null)
            {
                healthBarUI.Initialize(this, true); // Show health text for player
            }
            playerController = GetComponent<WDPlayerController>();
        }

        public override void TakeDamage(float damage)
        {
            // Check invulnerability
            if (Time.time - lastDamageTime < invulnerabilityTime)
                return;

            base.TakeDamage(damage);
            lastDamageTime = Time.time;

            UIManager.instance.UIGameplayCtr.ShowHealthWarning();
        }

        public override void Die()
        {
            base.Die();
            
            // Show game over UI
            if (gameOverUI != null)
            {
                gameOverUI.SetActive(true);
            }
            if (healthBarUI != null)
                healthBarUI.gameObject.SetActive(false);
            // Deactivate the player object
            if (playerController != null)
            {
                playerController.ShowGhost();
                playerController.RespawnPlayer(true);
            }
        }

        public void ResetHealth()
        {
            if (healthBarUI != null)
                healthBarUI.gameObject.SetActive(true);
            Initialize(maxHealth);
        }

        public void AddHealth(float healthAdd)
        {
            if (currentHealth < MaxHealth)
            {
                currentHealth += healthAdd;
                if(currentHealth > maxHealth)
                    currentHealth = maxHealth;
                onHealthChanged?.Invoke(currentHealth);
            }
        }
    }
} 