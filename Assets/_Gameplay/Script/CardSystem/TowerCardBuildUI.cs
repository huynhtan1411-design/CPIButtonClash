using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using TemplateSystems;
using UISystems;
using System;
using static Cinemachine.DocumentationSortingAttribute;

public class TowerCardBuildUI : MonoBehaviour
{
    public Action OnClickAction;
    [SerializeField] bool isNoEventClick = false;
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image iconImageElemental;
    [SerializeField] private GameObject iconNotiNew;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI elementalText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Slider upgradeProgressSlider;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button cardButton;
    [SerializeField] private GameObject buttonContainer;
    [SerializeField] private Button upgradeButton;

    private BuildingCollectionItem cardData;
    private CardState cardState;
    private UpgradeBuilder deckBuilder;
    private int currentResources;
    private int requiredResources;
    private Tweener tweenerWait = null;
    public CardState CardState { get => cardState; set => cardState = value; }

    public void Setup(BuildingCollectionItem data, UpgradeBuilder builder)
    {
        cardData = data;
        deckBuilder = builder;
        currentResources = cardData.Quantity;
        Debug.LogError("Level " + cardData.Level);
        requiredResources = DataManager.Instance.GetUpgradeCardHeroCount(cardData.Level);
        int maxLevel = DataManager.Instance.GetMaxLevelUpgradeCard();
        var buildingData = DataManager.Instance.GetBuildingDataByID(data.buildingID);
        var buildingBaseData = buildingData.GetLevelData(1);
        if (buildingData == null)
            Debug.LogError("Null");
        iconImage.sprite = buildingBaseData.icon;
        nameText.text = buildingData.nameBase;
        levelText.text = $"Lv.{data.Level}";
        progressText.text = $"{currentResources}/{requiredResources}";
        upgradeProgressSlider.value = (float)currentResources / requiredResources;
        buttonContainer.SetActive(false);
        if (!isNoEventClick)
        {
            if (data.isUnlocked)
            {
                cardButton.onClick.RemoveAllListeners();
                cardButton.onClick.AddListener(() =>
                {
                    OnClick();
                });
            }
            else
            {
                iconImageElemental.gameObject.SetActive(false);
                upgradeProgressSlider.gameObject.SetActive(false);
                progressText.gameObject.SetActive(false);
                levelText.gameObject.SetActive(false);
            }
        }

        if (tweenerWait != null && tweenerWait.IsActive())
        {
            tweenerWait.Kill();
            tweenerWait = null;
        }
    }

    private void OnClick()
    {
        deckBuilder.ShowUpgradePopup(this, cardData, deckBuilder);
        //if (cardState == CardState.WaitReplace)
        //{
        //    OnClickAction?.Invoke();
        //}
        //else
        //{
        //    if (buttonContainer.activeSelf)
        //    {
        //        buttonContainer.SetActive(false);
        //    }
        //    else
        //    {
        //        deckBuilder.ResetClickCard();
        //        ConfigureButtons();
        //    }
        //}
    }

    private void ConfigureButtons()
    {
        buttonContainer.SetActive(true);
        if (cardState == CardState.Build)
        {

            upgradeButton.gameObject.SetActive(true);
        }
        else if (cardState == CardState.Available)
        {
            upgradeButton.gameObject.SetActive(true);
        }

        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(() => {
        buttonContainer.SetActive(false);
        deckBuilder.ShowUpgradePopup(this, cardData, deckBuilder);
        });
    }

    public void SetState(CardState state)
    {
        cardState = state;
        ConfigureButtons();
    }

public void StartWaitReplace()
{
        if (tweenerWait == null || !tweenerWait.IsActive())
        {
            tweenerWait = transform.DOShakePosition(0.5f, 5f, 10, 90, false, true).SetLoops(-1, LoopType.Restart);
        }
        cardState = CardState.WaitReplace;
}

    public void StopWaitReplace()
    {
        if (tweenerWait != null && tweenerWait.IsActive())
        {
            tweenerWait.Kill();
            tweenerWait = null;
        }

        cardState = CardState.Build;
        OnClickAction = null;
    }

    public void ResetClick()
    {
        if (buttonContainer.activeSelf)
        {
            buttonContainer.SetActive(false);
        }
    }

    public BuildingCollectionItem GetCardData()
    {
        return cardData;
    }
}