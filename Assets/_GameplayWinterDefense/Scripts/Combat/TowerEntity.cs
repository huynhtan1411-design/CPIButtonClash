using UnityEngine;

namespace CLHoma.Combat
{
    public class TowerEntity : BaseDamageableEntity
    {
        [SerializeField] private GameObject destroyedEffectPrefab;
        [SerializeField] private HealthBarUI healthBarUI;
        [SerializeField] protected GameObject lightObject;
        [SerializeField] private GameObject fireSmokeEffectPrefab;
        protected override void Start()
        {
            base.Start();
            
            // Initialize health bar if assigned
            if (healthBarUI != null)
            {
                healthBarUI.Initialize(this, false); // Don't show health text for tower
                healthBarUI.SetVisibility(false);
            }
            WD.GameManager.Instance.OnCombatPhaseStart.AddListener(() => {
                if (lightObject != null)
                    lightObject.SetActive(true);
            });

            WD.GameManager.Instance.OnBuildPhaseStart.AddListener(() => {
                if (lightObject != null)
                    lightObject.SetActive(false);
            });
        }

        public override void Die()
        {
            base.Die();
            
            // Spawn destroyed effect if assigned
            if (destroyedEffectPrefab != null)
            {
                GameObject obj = Instantiate(destroyedEffectPrefab, transform.position, Quaternion.identity);
                BuildingManager.Instance.destroyEffectObjects.Add(obj);
            }

            // Destroy the tower object
            Destroy(gameObject);
        }
        public override void TakeDamage(float damage)
        {
            base.TakeDamage(damage);
            if(healthBarUI)
              healthBarUI.SetVisibility(true);

            if ((currentHealth / maxHealth) <= 0.6f)
            {
                if (fireSmokeEffectPrefab != null)  
                {
                    GameObject fire = Instantiate(fireSmokeEffectPrefab, transform.position, fireSmokeEffectPrefab.transform.rotation, transform);
                    BuildingManager.Instance.destroyEffectObjects.Add(fire);
                }
            }
        }
        public override void HealAll()
        {
            base.HealAll();
            healthBarUI.SetVisibility(false);
        }
    }
} 