using System;
using System.Collections;
using TMPro;
using UISystems;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
namespace CLHoma
{
    [System.Serializable]
    public class ComboData
    {
        public int clicksRequired;
        public float multiplier;
        public Color Color;
    }

    public class ClickerController : ManualSingletonMono<ClickerController>
    {
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _experienceText;
        [SerializeField] private Slider _experienceSlider;
        [SerializeField] private RectTransform _buttonHandle;
        [SerializeField] private UIAnimationAutoClicker uIAnimationAutoClicker;
        [SerializeField] private UIComboSlider uiComboSlider;

        private double experiencePerClick;
        private PlayerStatsManager playerStats;

        private float lastClickTime;
        private bool onTutorial = false;

        public void Initialize()
        {
            playerStats = PlayerController.StatsManager;
            playerStats.OnChangeExperience += UpdateUIInfo;
            playerStats.OnChangeStats += UpdateExperiencePerClick;

            if (uiComboSlider != null)
            {
                uiComboSlider.SetComboDatas(GameManager.GameConfig.comboTiers);
            }

            UpdateExperiencePerClick();
            UpdateUIInfo();
            lastClickTime = Time.time;
            StartCoroutine(AutoClickRoutine());
        }
        public void Reload()
        {
            uiComboSlider.ResetCombo();
            uIAnimationAutoClicker.ResetAnimation();
        }
        public void OnClick()
        {
            if (onTutorial) return;
            if (uiComboSlider.GetCurrentStreak() > 1 && TutorialManager.GetTutorial(0) == 0)
            {
                onTutorial = true;
                DOVirtual.DelayedCall(3f, delegate
                {
                    onTutorial = false;
                });
            }
            if (uiComboSlider != null)
            {
                uiComboSlider.UpdateCombo(uiComboSlider.GetCurrentStreak() + 1f);
            }
            lastClickTime = Time.time;
            Audio_Manager.instance.play("sfx_ui_button_resource_B");
            HandleIncreaseExp();
        }

        private void HandleIncreaseExp()
        {
            //double experienceGained = experiencePerClick * uiComboSlider.GetCurrentMultiplier();
            //playerStats.AddExperience(experienceGained);
            //UpdateUIInfo();
            //ShowFloatingText($"+{experienceGained}");
        }

        private void FixedUpdate()
        {
            if (GameManager.Instance.IsGamePaused) return;
            float timeSinceLastClick = Time.time - lastClickTime;
            if (timeSinceLastClick > 1f)
            {
                if (uiComboSlider != null)
                {
                    float currentStreak = uiComboSlider.GetCurrentStreak();
                    float currentMultiplier = uiComboSlider.GetCurrentMultiplier();
                    float decayRate = (5f * currentMultiplier);
                    float newStreak = currentStreak - (decayRate * Time.deltaTime);
                    if (newStreak < 0f) newStreak = 0f;
                    uiComboSlider.UpdateCombo(newStreak);
                }
            }
        }

        private void UpdateComboLevel()
        {
            if (uiComboSlider != null)
            {
                uiComboSlider.UpdateCombo(uiComboSlider.GetCurrentStreak());
            }
        }

        private void UpdateUIInfo()
        {
            UpdateLevelText();
            UpdateExperienceText();
            UpdateExperienceSlider();
        }

        private void UpdateLevelText()
        {
            if (_levelText != null)
            {
                _levelText.text = $"{playerStats.Level}";
            }
        }

        private void UpdateExperienceText()
        {
            if (_experienceText == null) return;

            double requiredExperience = playerStats.GetExperienceRequiredForNextLevel();
            _experienceText.text = string.Format("{0} / {1}", playerStats.CurrentExperience, requiredExperience);
        }

        private void UpdateExperienceSlider()
        {
            if (_experienceSlider == null) return;

            double requiredExperience = playerStats.GetExperienceRequiredForNextLevel();
            _experienceSlider.value = (float)(playerStats.CurrentExperience / requiredExperience);
        }

        private void ShowFloatingText(string text)
        {
            Vector3 target = Utils.GetRandomPositionAround(_buttonHandle.transform.position, 100f);
            UIFloatingTextController.Instance.ShowText(text, target, _buttonHandle);
        }

        private void UpdateExperiencePerClick()
        {
            float basePerClick = GameManager.GameConfig.baseExperiencePerClick;
            experiencePerClick = Math.Round(basePerClick * playerStats.increaseResourcePerTap * playerStats.resourceMultiplier, 1);
        }

        private IEnumerator AutoClickRoutine()
        {
            while (true)
            {
                if (GameManager.Instance.IsGamePaused)
                {
                    yield return new WaitForSeconds(0.2f);
                    continue;
                }

                int clicks = playerStats.autoClick;
                if (clicks <= 0)
                {
                    uIAnimationAutoClicker.Disable();
                    yield return new WaitForSeconds(1f);
                    continue;
                }

                uIAnimationAutoClicker.Setup(clicks);

                float interval = 1f / clicks;
                for (int i = 0; i < clicks; i++)
                {
                    HandleIncreaseExp();
                    yield return new WaitForSeconds(interval);
                }
            }
        }
    }
}