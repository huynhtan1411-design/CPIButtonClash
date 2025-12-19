using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using TemplateSystems;
using DG.Tweening;
using UISystems;

public class DeckBuilder : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private List<HeroCardBuildUI> cardBuildslots;
    [SerializeField] private Transform cardsAvailableListContent; 
    [SerializeField] private Transform cardsLockListContent;
    [SerializeField] private Button cardUIChooseContainerBtn;
    [SerializeField] private HeroCardBuildUI cardUIChoose;
    [SerializeField] private GameObject heroCardUIPrefab; 
    [SerializeField] private PopupUpgradeCardHeros popupUpgradeCardHero;

    private List<HeroCardData> cardDataHeroBuilds = new List<HeroCardData>();
    private List<HeroCardData> cardDataHeroCollections = new List<HeroCardData>(); 
    private List<HeroCardData> cardDataAvailables = new List<HeroCardData>(); 
    private List<HeroCardData> cardDataLockeds = new List<HeroCardData>(); 

    private const int maxCollectionSize = 10;
    private HeroCardData cardDataUpgrade = null;
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
        cardDataHeroCollections = DataManager.Instance.LoadHeroCollection();
        foreach (var cardCl in cardDataHeroCollections)
        { 
            if(cardCl.CardState == CardState.Build)
                cardDataHeroBuilds.Add(cardCl);
            else
                cardDataAvailables.Add(cardCl);
        }
        var allHeroes = DataManager.Instance.HerosInfoData.Data;

        foreach (var hero in allHeroes)
        {
            if (hero.Id == "0") 
                continue;

            string id = int.Parse(hero.Id).ToString("D2");

            if (!cardDataHeroCollections.Exists(card => card.Id == id))
            {
                HeroeType heroType = (HeroeType)System.Enum.Parse(typeof(HeroeType), hero.Id);

                HeroCardData card = new HeroCardData
                {
                    Id = id,
                    HeroType = heroType,
                    CardState = CardState.Lock,
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
        for (int i = 0; i < cardBuildslots.Count; i++)
        {
            if (i < cardDataHeroBuilds.Count)
            {
                cardBuildslots[i].gameObject.SetActive(true);
                cardBuildslots[i].Setup(cardDataHeroBuilds[i], this);
            }
            else
            {
                cardBuildslots[i].gameObject.SetActive(false);
            }
        }
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

    private void CreateHeroCardUI(HeroCardData heroCardData, Transform parent)
    {
        GameObject cardObj = Instantiate(heroCardUIPrefab, parent);
        HeroCardBuildUI cardUI = cardObj.GetComponent<HeroCardBuildUI>();
        cardUI.Setup(heroCardData, this); 
    }

    private void AddToCollection(HeroCardData heroCardData)
    {
        if (cardDataHeroCollections.Count >= maxCollectionSize)
        {
            Debug.LogWarning("Collection is full!");
            return;
        }

        cardDataHeroCollections.Add(heroCardData);
        PopulateUI();
    }

    private void RemoveFromCollection(HeroCardData heroCardData)
    {
        cardDataHeroCollections.Remove(heroCardData);
        PopulateUI();
    }


    private void ClearContent(Transform content)
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
    }

    public void SaveCollection()
    {
        DataManager.Instance.SaveHeroCollection(cardDataHeroCollections);
        Debug.Log("Hero collection saved!");
    }
    public void StartPickReplacement(HeroCardBuildUI selectedCard)
    {
        foreach(HeroCardBuildUI cardSlot in cardBuildslots)
        {
            if (cardSlot != null)
            {
                if (cardSlot.CardState == CardState.Build)
                {
                    cardSlot.StartWaitReplace();
                    cardSlot.OnClickAction = () =>
                    {
                        SwapCards(selectedCard, cardSlot);
                    };
                }
            }
        }
        cardUIChoose.Setup(selectedCard.GetCardData(), this);
        cardUIChooseContainerBtn.gameObject.SetActive(true);
        UIManager.instance.MenuLevelCtr.LockMenuUI();
    }

    private void SwapCards(HeroCardBuildUI availableCard, HeroCardBuildUI buildCard)
    {
        HeroCardData tempData = buildCard.GetCardData();

        int buildIndex = cardBuildslots.IndexOf(buildCard);
        int availableIndex = cardDataAvailables.FindIndex(card => card.Id == availableCard.GetCardData().Id);

        if (buildIndex >= 0 && availableIndex >= 0)
        {
            cardDataHeroBuilds[buildIndex] = availableCard.GetCardData();
            cardDataAvailables[availableIndex] = tempData;
            cardDataHeroBuilds[buildIndex].CardState = CardState.Build;
            cardDataAvailables[availableIndex].CardState = CardState.Available;
            DataManager.Instance.SaveHeroCollection(DataManager.Instance.CardsCollectionData);
        }

        buildCard.Setup(availableCard.GetCardData(), this);
        availableCard.Setup(tempData, this);
        cardUIChoose.Setup(tempData, this);
        availableCard.gameObject.SetActive(false);

        foreach (var cardSlot in cardBuildslots)
        {
            if (cardSlot.CardState == CardState.WaitReplace)
            {
                cardSlot.StopWaitReplace();
            }
        }
        Vector3 poscardUIChoose = cardUIChoose.transform.position;
        Vector3 poscardUIBuild = buildCard.transform.position;
        Vector3 posBuild = buildCard.transform.position;
        Vector3 posAvailable = availableCard.transform.position;
        cardUIChoose.transform.DOMove(posBuild, 0.2f).SetEase(Ease.Linear).OnComplete(() => {
            availableCard.gameObject.SetActive(true);
            cardUIChooseContainerBtn.gameObject.SetActive(false);
            cardUIChoose.transform.position = poscardUIChoose;
            UIManager.instance.MenuLevelCtr.UnLockMenuUI();
        });
        buildCard.transform.DOMove(posAvailable, 0.2f).SetEase(Ease.Linear).OnComplete(() => {
            buildCard.transform.position = poscardUIBuild;
            buildCard.gameObject.SetActive(true);
        });
    }

    public void ShowUpgradePopup(HeroCardBuildUI ui, HeroCardData cardData, DeckBuilder deckBuilder)
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