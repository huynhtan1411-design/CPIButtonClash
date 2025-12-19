using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CLHoma.Combat;
using System;
namespace WD
{
    public class UIBuildingItem : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private TextMeshProUGUI rangeText;
        [SerializeField] private TextMeshProUGUI attackSpeedText;
        [SerializeField] private TextMeshProUGUI upgradeCostText;
        [SerializeField] private TextMeshProUGUI unlockConditionText;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private GameObject activeObjects;
        [SerializeField] private GameObject disbleObjects;

        [Header("Button Components")]
        [SerializeField] private Button buildButton;
        [SerializeField] private Image buildButtonImage;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI buttonText;
        [SerializeField] private Sprite buttonNormalSprite;
        [SerializeField] private Sprite buttonDisabledSprite;

        private Color textNormalColor = Color.white;
        private Color textDisabledColor = Color.red;

        public Action OnBuildClickEvent;

        private void UpdateButtonVisuals(bool canAfford)
        {
            if (buildButton != null)
            {
                buildButton.interactable = canAfford;

                if (buildButtonImage != null)
                {
                    buildButtonImage.sprite = canAfford ? buttonNormalSprite : buttonDisabledSprite;
                }

                if (priceText != null)
                {
                    priceText.color = canAfford ? textNormalColor : textDisabledColor;
                }
            }
        }
        private void ToggleByCondition(int levelUnlockCondition)
        {
            bool isUnlocked = SafeZoneController.Instance.CurrentZoneLevel >= levelUnlockCondition;
            
            if (activeObjects != null)
            {
                activeObjects.SetActive(isUnlocked);
                iconImage.color = isUnlocked ? Color.white : Color.black;
            }
            if (disbleObjects != null)
            {
                disbleObjects.SetActive(!isUnlocked);
                iconImage.color = !isUnlocked ? Color.black : Color.white;
            }
        }
        public void SetupInfo(BuildingLevelData data, int currentLevel)
        {
            if (data == null) return;
            OnBuildClickEvent = null;
            // Set icon
            if (iconImage != null && data.icon != null)
            {
                iconImage.sprite = data.icon;
            }

            // Set texts
            if (levelText != null)
                nameText.text = $"{data.name}";
            if (levelText != null)
                levelText.text = $"LV. {currentLevel}";
            
            if (healthText != null)
                healthText.text = $"{data.Health}";
            
            if (damageText != null)
                damageText.text = $"{data.AttackDamage}";
            
            if (rangeText != null)
                rangeText.text = $"{data.attackRange}";
            
            if (attackSpeedText != null)
                attackSpeedText.text = $"{data.attackSpeed}";
            
            if (upgradeCostText != null)
                upgradeCostText.text = $"{data.upgradeCost}";
            
            if (unlockConditionText != null)
                unlockConditionText.text = $"Unlock After Castle level {data.levelUnlockCondition}";
            if (description != null)
                description.text = $"{data.description}";
            ToggleByCondition(data.levelUnlockCondition);
            
            buttonText.text = "Build";
            if (buildButton != null)
            {
                bool canAfford = BuildingManager.Instance.CanAffordBuilding(data.upgradeCost);
                UpdateButtonVisuals(canAfford);
            }
        }

        public void SetupUpgradeInfo(BuildingLevelData levelPrevious, BuildingLevelData levelNext, int currentLevel)
        {
            Debug.LogError("SetupUpgradeInfo");
            if (levelPrevious == null || levelNext == null) return;

            // Set icon and name
            if (iconImage != null && levelNext.icon != null)
                iconImage.sprite = levelNext.icon;
            if (nameText != null)
                nameText.text = $"{levelNext.name}";
            if (levelText != null)
                levelText.text = $"LV. {currentLevel} >> {currentLevel + 1}";

            // Set stats with upgrade indicators only when values change
            if (healthText != null)
                healthText.text = levelNext.Health != levelPrevious.Health 
                    ? $"{levelPrevious.Health} >> <color=green>{levelNext.Health}</color>"
                    : $"{levelPrevious.Health}";
            
            if (damageText != null)
                damageText.text = levelNext.AttackDamage != levelPrevious.AttackDamage
                    ? $"{levelPrevious.AttackDamage} >> <color=green>{levelNext.AttackDamage}</color>"
                    : $"{levelPrevious.AttackDamage}";
            
            if (rangeText != null)
                rangeText.text = levelNext.attackRange != levelPrevious.attackRange
                    ? $"{levelPrevious.attackRange} >> <color=green>{levelNext.attackRange}</color>"
                    : $"{levelPrevious.attackRange}";
            
            if (attackSpeedText != null)
                attackSpeedText.text = levelNext.attackSpeed != levelPrevious.attackSpeed
                    ? $"{levelPrevious.attackSpeed} >> <color=green>{levelNext.attackSpeed}</color>"
                    : $"{levelPrevious.attackSpeed}";
            
            if (upgradeCostText != null)
                upgradeCostText.text = $"{levelNext.upgradeCost}";
            
            if (unlockConditionText != null)
                unlockConditionText.text = $"Unlock After Castle level {levelNext.levelUnlockCondition}";
            if (description != null)
                description.text = $"{levelNext.description}";
            buttonText.text = "Upgrade";
            // Enable/disable button based on player's gold
            if (buildButton != null)
            {
                bool canAfford = BuildingManager.Instance.CanAffordBuilding(levelNext.upgradeCost);
                UpdateButtonVisuals(canAfford);
            }
            ToggleByCondition(levelNext.levelUnlockCondition);
        }
        public void OnBuildClick()
        {
            OnBuildClickEvent?.Invoke();
        }


    }
} 