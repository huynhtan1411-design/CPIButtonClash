using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TemplateSystems;
using UISystems;
using System;

public class PopupUpgradeCardHeros : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image heroIcon;
    [SerializeField] private TextMeshProUGUI heroNameText;
    [SerializeField] private TextMeshProUGUI elementalNameText;
    [SerializeField] private TextMeshProUGUI currentDamageText;
    [SerializeField] private TextMeshProUGUI upgradedDamageText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Slider upgradeProgressSlider;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button equipButton;
    [SerializeField] private TextMeshProUGUI upgradeCostText;

    private HeroCardData cardData;
    private HeroCardBuildUI cardUI;
    private DeckBuilder deckCtr;
    private int currentResources;
    private int requiredResources;
    private int upgradeCost;
    private Action actionCl = null;
    public void Setup(HeroCardData data, HeroCardBuildUI uiCard, DeckBuilder deckBuilderCtr, Action actionComplete = null)
    {
        actionCl = actionComplete;
        cardData = data;
        cardUI = uiCard;
        deckCtr = deckBuilderCtr;
        ShowUI(data);
    }

    public void ShowUI(HeroCardData data)
    {
        cardData = data;
        currentResources = data.Quantity;
        requiredResources = DataManager.Instance.GetUpgradeCardHeroCount(data.Level);
        upgradeCost = DataManager.Instance.GetUpgradeCardHeroPrice(data.Level);
        int maxLevel = DataManager.Instance.GetMaxLevelUpgradeCard();
        string idHero = ((int)cardData.HeroType).ToString();
        HerosInfoData infoData = DataManager.Instance.GetInfoDataHero(idHero);
        // Load UI elements
        UIManager.instance.LoadIcon("Hero_" + (int)data.HeroType, heroIcon);
        heroNameText.text = data.HeroType.ToString();
        elementalNameText.text = DataManager.Instance.GetElementalHero(data.HeroType).ToString();
        float baseDamage = infoData.Damage;
        float currentDamage = baseDamage + (baseDamage * (cardData.Level - 1) * 10f / 100f);
        int roundedUpgradedDamage = Mathf.RoundToInt(currentDamage);
        upgradedDamageText.text = $"{roundedUpgradedDamage}";
        levelText.text = $"{cardData.Level}/{maxLevel}";
        progressText.text = $"{currentResources}/{requiredResources}";
        upgradeProgressSlider.value = (float)currentResources / requiredResources;

        // Configure upgrade button
        if (cardData.Level == maxLevel)
            upgradeButton.gameObject.SetActive(false);
        else
        {
            upgradeButton.gameObject.SetActive(true);
            ConfigureUpgradeButton();
        }


        if (data.CardState == CardState.Build)
            equipButton.gameObject.SetActive(false);
        else
        {
            equipButton.gameObject.SetActive(true);
            equipButton.onClick.RemoveAllListeners();
            equipButton.onClick.AddListener(() => {
                gameObject.SetActive(false);
                deckCtr.StartPickReplacement(cardUI);
            });
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
            DataManager.Instance.SaveHeroCollection(DataManager.Instance.CardsCollectionData);

            // Update UI
            ShowUI(cardData);

            Audio_Manager.instance.play("Click");
        }
    }
}