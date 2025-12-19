using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using TemplateSystems;
using DG.Tweening;
using UISystems;

public class UpgradeBuilder : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private List<TowerCardBuildUI> cardBuildslots;
    [SerializeField] private Transform cardsAvailableListContent; 
    [SerializeField] private Transform cardsLockListContent;
    [SerializeField] private Button cardUIChooseContainerBtn;
    [SerializeField] private GameObject heroCardUIPrefab; 
    [SerializeField] private PopupUpgradeCardTowers popupUpgradeCardHero;

    private List<HeroCardData> cardDataHeroBuilds = new List<HeroCardData>();
    List<BuildingCollectionItem> cardDataHeroCollections = new List<BuildingCollectionItem>();
    List<BuildingCollectionItem> cardDataAvailables = new List<BuildingCollectionItem>(); 
    private List<BuildingCollectionItem> cardDataLockeds = new List<BuildingCollectionItem> (); 

    private const int maxCollectionSize = 10;
    private BuildingCollectionItem cardDataUpgrade = null;
    void Start()
    {

    }

    private void OnEnable()
    {
        LoadHeroData();
        PopulateUI();
    }
    private void OnDisable()
    {
        
    }
    private void LoadHeroData()
    {
        cardDataHeroBuilds.Clear();
        cardDataAvailables.Clear();
        cardDataLockeds.Clear();
        cardDataHeroCollections = DataManager.Instance.BuildingCollectionData;
        foreach (var cardCl in cardDataHeroCollections)
        {
            cardDataAvailables.Add(cardCl);
        }
        var allBuildings = DataManager.Instance.BuildingConfig.Data;

        foreach (var tower in allBuildings)
        {
            string id = tower.ID;

            if (!cardDataHeroCollections.Exists(card => card.buildingID == id))
            {
                BuildingCollectionItem card = new BuildingCollectionItem
                {
                    buildingID = id,
                    isUnlocked = false,
                    Level = 1
                };
                cardDataLockeds.Add(card);
            }
        }
        UIManager.instance.MenuLevelCtr.UnLockMenuUI();
    }

    private void PopulateUI()
    {
        coinText.text = DataManager.COINS.ToString();
        ClearContent(cardsAvailableListContent);
        ClearContent(cardsLockListContent);
        foreach (var heroCard in cardDataAvailables)
        {
            CreateHeroCardUI(heroCard, cardsAvailableListContent);
        }

        foreach (var heroCard in cardDataLockeds)
        {
            CreateHeroCardUI(heroCard, cardsLockListContent);
        }
        cardUIChooseContainerBtn.onClick.RemoveAllListeners();
        cardUIChooseContainerBtn.onClick.AddListener(CloseChooseCard);
    }

    private void CreateHeroCardUI(BuildingCollectionItem cardData, Transform parent)
    {
        GameObject cardObj = Instantiate(heroCardUIPrefab, parent);
        TowerCardBuildUI cardUI = cardObj.GetComponent<TowerCardBuildUI>();
        cardUI.Setup(cardData, this); 
    }

    //private void AddToCollection(HeroCardData heroCardData)
    //{
    //    if (cardDataHeroCollections.Count >= maxCollectionSize)
    //    {
    //        Debug.LogWarning("Collection is full!");
    //        return;
    //    }

    //    cardDataHeroCollections.Add(heroCardData);
    //    PopulateUI();
    //}

    //private void RemoveFromCollection(HeroCardData heroCardData)
    //{
    //    cardDataHeroCollections.Remove(heroCardData);
    //    PopulateUI();
    //}


    private void ClearContent(Transform content)
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
    }

    //public void SaveCollection()
    //{
    //    DataManager.Instance.SaveHeroCollection(cardDataHeroCollections);
    //    Debug.Log("Hero collection saved!");
    //}
    //public void StartPickReplacement(TowerCardBuildUI selectedCard)
    //{
    //    foreach(TowerCardBuildUI cardSlot in cardBuildslots)
    //    {
    //        if (cardSlot != null)
    //        {
    //            if (cardSlot.CardState == CardState.Build)
    //            {
    //                cardSlot.StartWaitReplace();
    //                cardSlot.OnClickAction = () =>
    //                {
    //                    SwapCards(selectedCard, cardSlot);
    //                };
    //            }
    //        }
    //    }
    //    cardUIChoose.Setup(selectedCard.GetCardData(), this);
    //    cardUIChooseContainerBtn.gameObject.SetActive(true);
    //    UIManager.instance.MenuLevelCtr.LockMenuUI();
    //}

    //private void SwapCards(TowerCardBuildUI availableCard, TowerCardBuildUI buildCard)
    //{
    //    HeroCardData tempData = buildCard.GetCardData();

    //    int buildIndex = cardBuildslots.IndexOf(buildCard);
    //    int availableIndex = cardDataAvailables.FindIndex(card => card.Id == availableCard.GetCardData().Id);

    //    if (buildIndex >= 0 && availableIndex >= 0)
    //    {
    //        cardDataHeroBuilds[buildIndex] = availableCard.GetCardData();
    //        cardDataAvailables[availableIndex] = tempData;
    //        cardDataHeroBuilds[buildIndex].CardState = CardState.Build;
    //        cardDataAvailables[availableIndex].CardState = CardState.Available;
    //        DataManager.Instance.SaveHeroCollection(DataManager.Instance.CardsCollectionData);
    //    }

    //    buildCard.Setup(availableCard.GetCardData(), this);
    //    availableCard.Setup(tempData, this);
    //    cardUIChoose.Setup(tempData, this);
    //    availableCard.gameObject.SetActive(false);

    //    foreach (var cardSlot in cardBuildslots)
    //    {
    //        if (cardSlot.CardState == CardState.WaitReplace)
    //        {
    //            cardSlot.StopWaitReplace();
    //        }
    //    }
    //    Vector3 poscardUIChoose = cardUIChoose.transform.position;
    //    Vector3 poscardUIBuild = buildCard.transform.position;
    //    Vector3 posBuild = buildCard.transform.position;
    //    Vector3 posAvailable = availableCard.transform.position;
    //    cardUIChoose.transform.DOMove(posBuild, 0.2f).SetEase(Ease.Linear).OnComplete(() => {
    //        availableCard.gameObject.SetActive(true);
    //        cardUIChooseContainerBtn.gameObject.SetActive(false);
    //        cardUIChoose.transform.position = poscardUIChoose;
    //        UIManager.instance.MenuLevelCtr.UnLockMenuUI();
    //    });
    //    buildCard.transform.DOMove(posAvailable, 0.2f).SetEase(Ease.Linear).OnComplete(() => {
    //        buildCard.transform.position = poscardUIBuild;
    //        buildCard.gameObject.SetActive(true);
    //    });
    //}

    public void ShowUpgradePopup(TowerCardBuildUI ui, BuildingCollectionItem cardData, UpgradeBuilder deckBuilder)
    {
        // Show the upgrade popup
        cardDataUpgrade = cardData;
        if (popupUpgradeCardHero != null)
        {
            popupUpgradeCardHero.Setup(cardData, ui, deckBuilder, () => {
                ui.Setup(cardData, this);
            });
            popupUpgradeCardHero.gameObject.SetActive(true);
        }
    }

    public void CloseChooseCard()
    {
        foreach (var cardSlot in cardBuildslots)
        {
            if (cardSlot.CardState == CardState.WaitReplace)
            {
                cardSlot.StopWaitReplace();
            }
        }
        cardUIChooseContainerBtn.gameObject.SetActive(false);
        UIManager.instance.MenuLevelCtr.UnLockMenuUI();
    }
    public void ResetClickCard()
    {
        foreach (var cardSlot in cardBuildslots)
        {
            cardSlot.ResetClick();
        }

    }
    public void CloseUpgradePopup(HeroCardData cardData)
    {

    }
}