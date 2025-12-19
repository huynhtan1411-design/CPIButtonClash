//using CLHoma.DGTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace CLHoma.Combat
{

    [System.Serializable]
    public class HealthbarBehaviour : MonoBehaviour
    {
        [SerializeField] Transform healthBarTransform;
        public Transform HealthBarTransform => healthBarTransform;

        [SerializeField] Vector3 healthbarOffset;
        public Vector3 HealthbarOffset => healthbarOffset;

        [Space]
        [SerializeField] CanvasGroup healthBarCanvasGroup;
        [SerializeField] Image healthFillImage;
        [SerializeField] Image maskFillImage;
        [SerializeField] TextMeshProUGUI healthText;
        [SerializeField] Image shieldImage;

        [Space]
        [SerializeField] Color standartHealthbarColor;
        [SerializeField] Color specialHealthbarColor;
        [SerializeField] Color standartShieldColor;
        [SerializeField] Color specialShieldColor;

        [Header("Display Settings")]
        [SerializeField] private float hideDelay = 1f;           // Time before auto-hiding the health bar
        [SerializeField] private float fadeOutDuration = 0.3f;   // Duration of fade out animation

        private IHealth targetHealth;
        private Transform parentTransform;
        private bool showAlways;

        private Vector3 defaultOffset;

        private bool isInitialised;
        private bool isPanelActive;
        private bool isDisabled;
        private int level;

        private Tween maskTween;
        private Tween panelTween;
        private Tween autoHideTween;

        private void LateUpdate()
        {
            if (isInitialised && healthBarTransform != null && Camera.main != null)
            {
                // Update position
                healthBarTransform.position = parentTransform.position + healthbarOffset;
                
                // Make health bar face the camera
                healthBarTransform.rotation = Camera.main.transform.rotation;
            }
        }

        public void Initialise(Transform parentTransform, IHealth targetHealth, bool showAlways, Vector3 defaultOffset, int level = -1, bool isSpecial = false)
        {
            this.targetHealth = targetHealth;
            this.parentTransform = parentTransform;
            this.defaultOffset = defaultOffset;
            this.level = level;
            this.showAlways = showAlways;

            isDisabled = false;
            isPanelActive = false;

            // Reset bar parent
            healthBarTransform.SetParent(null); // Unparent to prevent rotation issues
            healthBarTransform.gameObject.SetActive(true);

            if (isSpecial)
            {
                healthFillImage.color = specialHealthbarColor;
                shieldImage.color = specialShieldColor;
            }
            else
            {
                healthFillImage.color = standartHealthbarColor;
                shieldImage.color = standartShieldColor;
            }

            // Redraw health
            RedrawHealth();

            // Initially hide the health bar
            healthBarCanvasGroup.alpha = 0f;
     
            isInitialised = true;
        }

        public void OnHealthChanged()
        {
            if (isDisabled || targetHealth == null)
                return;

            // Update fill amount
            healthFillImage.fillAmount = targetHealth.CurrentHealth / targetHealth.MaxHealth;

            // Kill previous mask tween if exists
            if (maskTween != null)
            {
                maskTween.Kill();
                maskTween = null;
            }

            // Create new tween for mask fill
            maskTween = maskFillImage.DOFillAmount(healthFillImage.fillAmount, 0.3f)
                .SetEase(Ease.InQuint);

            // Update health text
            if (level == -1)
            {
                healthText.text = targetHealth.CurrentHealth.ToString("F0");
            }

            // Show health bar
            ShowHealthBar();
        }

        private void ShowHealthBar()
        {
            // Kill any existing tweens safely
            if (panelTween != null)
            {
                panelTween.Kill();
                panelTween = null;
            }
            if (autoHideTween != null)
            {
                autoHideTween.Kill();
                autoHideTween = null;
            }

            // Make sure the health bar is active
            healthBarTransform.gameObject.SetActive(true);

            // Fade in the health bar
            panelTween = healthBarCanvasGroup.DOFade(1f, 0.2f).SetEase(Ease.OutQuad);

            // Set up auto-hide
            if (!showAlways)
            {
                autoHideTween = DOVirtual.DelayedCall(hideDelay, () =>
                {
                    // Only hide if we're not disabled
                    if (!isDisabled)
                    {
                        panelTween = healthBarCanvasGroup.DOFade(0f, fadeOutDuration)
                            .SetEase(Ease.InQuad);
                    }
                });
            }
        }

        public void DisableBar()
        {
            if (isDisabled)
                return;

            isDisabled = true;

            // Kill any existing tweens safely
            if (panelTween != null)
            {
                panelTween.Kill();
                panelTween = null;
            }
            if (autoHideTween != null)
            {
                autoHideTween.Kill();
                autoHideTween = null;
            }

            if (healthBarCanvasGroup.isActiveAndEnabled)
            {
                panelTween = healthBarCanvasGroup.DOFade(0f, fadeOutDuration)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() => healthBarTransform.gameObject.SetActive(false));
            }
        }

        public void EnableBar()
        {
            if (!isDisabled)
                return;

            isDisabled = false;
            healthBarTransform.gameObject.SetActive(true);
            ShowHealthBar();
        }

        public void RedrawHealth()
        {
            healthFillImage.fillAmount = targetHealth.CurrentHealth / targetHealth.MaxHealth;
            maskFillImage.fillAmount = healthFillImage.fillAmount;

            if (level == -1)
            {
                shieldImage.gameObject.SetActive(false);
                healthText.text = targetHealth.CurrentHealth.ToString("F0");
            }
            else
            {
                shieldImage.gameObject.SetActive(true);
                healthText.text = level.ToString();
            }
        }

        public void ForceDisable()
        {
            isDisabled = true;

            // Kill any existing tweens
            if (maskTween != null && maskTween.IsPlaying()) maskTween.Kill();
            if (panelTween != null && panelTween.IsPlaying()) panelTween.Kill();
            if (autoHideTween != null && autoHideTween.IsPlaying()) autoHideTween.Kill();

            healthBarTransform.gameObject.SetActive(false);
            healthBarCanvasGroup.gameObject.SetActive(false);
        }

        public void Destroy()
        {
            isDisabled = true;

            // Kill all tweens safely
            if (maskTween != null)
            {
                maskTween.Kill();
                maskTween = null;
            }
            if (panelTween != null)
            {
                panelTween.Kill();
                panelTween = null;
            }
            if (autoHideTween != null)
            {
                autoHideTween.Kill();
                autoHideTween = null;
            }

            Destroy(healthBarTransform.gameObject);
        }

        private void OnDestroy()
        {
            // Kill all tweens safely
            if (maskTween != null)
            {
                maskTween.Kill();
                maskTween = null;
            }
            if (panelTween != null)
            {
                panelTween.Kill();
                panelTween = null;
            }
            if (autoHideTween != null)
            {
                autoHideTween.Kill();
                autoHideTween = null;
            }
        }
    }

    public interface IHealth
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }

        public void TakeDamage(float damage, Vector3 projectilePosition, Vector3 projectileDirection, HitType hitType);
    }
}