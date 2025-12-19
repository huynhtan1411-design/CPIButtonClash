using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CLHoma.Combat
{
    public class HealthBarUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Color Settings")]
        [SerializeField] private Color highHealthColor = Color.green;
        [SerializeField] private Color lowHealthColor = Color.red;
        [SerializeField] private float lowHealthThreshold = 0.3f; // 30% health is considered low

        [Header("Animation")]
        [SerializeField] private float updateSpeed = 5f;
        private float targetFillAmount;

        private IDamageable targetEntity;
        private Camera mainCamera;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
        private void Start()
        {
            mainCamera = Camera.main;
            if (healthText != null)
            {
                healthText.gameObject.SetActive(false); // Hide by default
            }
        }

        private void LateUpdate()
        {
            if (targetEntity != null)
            {
                // Make health bar face camera
                if (mainCamera != null)
                {
                    transform.rotation = mainCamera.transform.rotation;
                }

                // Smoothly update fill amount
                if (healthSlider != null)
                {
                    float currentFill = healthSlider.value;
                    healthSlider.value = Mathf.Lerp(currentFill, targetFillAmount, Time.deltaTime * updateSpeed);
                }
            }
        }

        public void Initialize(IDamageable entity, bool showHealthText = false)
        {
            targetEntity = entity;

            // Subscribe to health change events
            if (entity is BaseDamageableEntity damageableEntity)
            {
                damageableEntity.onHealthChanged.AddListener(UpdateHealth);
                damageableEntity.onDeath.AddListener(OnEntityDeath);
            }

            if (healthText != null)
            {
                healthText.gameObject.SetActive(showHealthText);
            }

            // Initial health update
            UpdateHealth(entity.CurrentHealth);
        }

        public void UpdateHealth(float currentHealth)
        {
            if (targetEntity == null) return;

            float healthPercentage = currentHealth / targetEntity.MaxHealth;
            targetFillAmount = healthPercentage;

            // Update slider
            if (healthSlider != null)
            {
                healthSlider.value = healthPercentage;
            }

            // Update fill color
            if (fillImage != null)
            {
                fillImage.color = Color.Lerp(lowHealthColor, highHealthColor, healthPercentage / lowHealthThreshold);
            }

            // Update text if enabled
            if (healthText != null && healthText.gameObject.activeInHierarchy)
            {
                healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(targetEntity.MaxHealth)}";
            }
        }

        private void OnEntityDeath()
        {

        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (targetEntity is BaseDamageableEntity damageableEntity)
            {
                damageableEntity.onHealthChanged.RemoveListener(UpdateHealth);
                damageableEntity.onDeath.RemoveListener(OnEntityDeath);
            }
        }

        public void SetVisibility(bool isVisible)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = isVisible ? 1f : 0f;
            }
        }
    } 
} 