using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TemplateSystems;
using UISystems;
using System;

public class PopupUpgradeCardTowers : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image heroIcon;
    [SerializeField] private TextMeshProUGUI heroNameText;
    [SerializeField] private TextMeshProUGUI elementalNameText;
    [SerializeField] private TextMeshProUGUI upgradedHPText;
    [SerializeField] private TextMeshProUGUI upgradedRangeText;
    [SerializeField] private TextMeshProUGUI upgradedSpeedText;
    [SerializeField] private TextMeshProUGUI upgradedDamageText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Slider upgradeProgressSlider;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button equipButton;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private TextMeshProUGUI coinText;
    private BuildingCollectionItem cardData;
    private TowerCardBuildUI cardUI;
    private UpgradeBuilder deckCtr;
    private int currentResources;
    private int requiredResources;
    private int upgradeCost;
    private Action actionCl = null;
    public void Setup(BuildingCollectionItem data, TowerCardBuildUI uiCard, UpgradeBuilder deckBuilderCtr, Action actionComplete = null)
    {
        actionCl = actionComplete;
        cardData = data;
        cardUI = uiCard;
        deckCtr = deckBuilderCtr;
        ShowUI(data);
    }

    public void ShowUI(BuildingCollectionItem data)
    {
        cardData = data;
        currentResources = data.Quantity;
        requiredResources = DataManager.Instance.GetUpgradeCardHeroCount(data.Level);
        upgradeCost = DataManager.Instance.GetUpgradeCardHeroPrice(data.Level);
        int maxLevel = DataManager.Instance.GetMaxLevelUpgradeCard();
        var buildingData = DataManager.Instance.GetBuildingDataByID(data.buildingID);
        buildingData.LevelCard = data.Level;
        var buildingDataBase = buildingData.GetLevelDataUpgrade(1);
        if (buildingData == null)
            Debug.LogError("Null");
        heroIcon.sprite = buildingDataBase.icon;
        heroNameText.text = buildingData.nameBase;
        upgradedDamageText.text = $"{buildingDataBase.AttackDamage}";
        upgradedHPText.text = $"{buildingDataBase.Health}";
        upgradedRangeText.text = $"{buildingDataBase.attackRange}";
        upgradedSpeedText.text = $"{buildingDataBase.attackSpeed}";
        levelText.text = $"{cardData.Level}/{maxLevel}";
        progressText.text = $"{currentResources}/{requiredResources}";
        upgradeProgressSlider.value = (float)currentResources / requiredResources;
        coinText.text = DataManager.COINS.ToString();
        // Configure upgrade button
        if (cardData.Level == maxLevel)
            upgradeButton.gameObject.SetActive(false);
        else
        {
            upgradeButton.gameObject.SetActive(true);
            ConfigureUpgradeButton();
        }
        actionCl?.Invoke();
    }

    private void ConfigureUpgradeButton()
    {
        if (currentResources < requiredResources || DataManager.COINS < upgradeCost)
        {
            upgradeButton.interactable = false;
            if(DataManager.COINS < upgradeCost)
               upgradeCostText.color = Color.red;
        }
        else
        {
            upgradeButton.interactable = true;
            upgradeCostText.color = Color.white;
        }

        upgradeCostText.text = upgradeCost.ToString();

        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(UpgradeCard);
    }

    private void UpgradeCard()
    {

        if (cardData.Quantity >= requiredResources && DataManager.COINS >= upgradeCost)
        {
            // Deduct resources and coins
            DataManager.AddCoins(-upgradeCost);
            cardData.Quantity -= requiredResources;

            // Upgrade card level
            cardData.Level++;
            DataManager.Instance.SaveBuildingCollection();

            // Update UI
            ShowUI(cardData);           
            Audio_Manager.instance.play("Click");
        }
    }
}