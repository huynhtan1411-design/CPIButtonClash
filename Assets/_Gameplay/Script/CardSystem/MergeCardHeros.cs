using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TemplateSystems;
using DG.Tweening;
using CLHoma;
using UnityEngine.UI;
using UISystems;

public class MergeCardHeros : MonoBehaviour
{
    [Header("Merge Settings")]
    [Tooltip("Set the number of cards required to merge (2 or 3).")]
    [Range(2, 3)] // Allows only values 2 or 3 in the Inspector
    [SerializeField] private int countMerge = 3;
    public GameObject cardPrefab; // Prefab for hero cards
    public Transform conveyorBelt; // Parent object for cards
    public Transform cardDrag; 
    public RectTransform cardEraseArea;
    public RectTransform pointInit;
    public RectTransform pointEnd;
    public Slider spawnProgressBar; // UI Slider for progress bar
    public float cardSpacing = 180f; // Spacing between cards
    public float moveSpeed = 200f; // Speed of card movement
    public int maxCards = 30; // Maximum number of cards
    public float spawnInterval = 6f; // Interval for spawning cards

    [Space(10)]
    [Header("Init Animation")]
    [SerializeField] private RectTransform maskRect;
    [SerializeField] private RectTransform rectTransform;

    private List<GameObject> cards = new List<GameObject>();
    private List<HeroCardData> cardDataList = new List<HeroCardData>(); // List of hero card data
    private int cardCount = 0;
    private bool isMoveCardsPaused = false;
    public List<GameObject> Cards { get => cards; set => cards = value; }
    private bool isMerging = false;
    private void Awake()
    {
        UIManager.onLoadGame += Init;
        UIManager.onGameSet += Init;
    }

    void Start()
    {

    }

    private void OnDestroy()
    {
        UIManager.onLoadGame -= Init;
        UIManager.onGameSet -= Init;
    }

    public void Init()
    {
        StartCoroutine(DelayedInit());
    }

    private IEnumerator DelayedInit()
    {
        StartAnimation();
        yield return new WaitForSeconds(2f);
        // Reset cards
        ResetCards();

        if (spawnProgressBar != null)
        {
            spawnProgressBar.maxValue = spawnInterval;
            spawnProgressBar.value = 0;
        }
        // Spawn 5 initial cards first
        SpawnInitialCards(5);
        
        // After the initial cards are spawned, start the regular spawning
        StartCoroutine(SpawnCards());
        ResumeMoveCards();
    }

    //public void Init()
    //{
    //    ResetCards();
    //    if (spawnProgressBar != null)
    //    {
    //        spawnProgressBar.maxValue = spawnInterval;
    //        spawnProgressBar.value = 0;
    //    }
        //for (int i = 0; i < maxCards; i++)
        //{
        //    HeroCardData randomCardData = GenerateRandomHeroCardData();
        //    cardDataList.Add(randomCardData);
        //}
        //HeroCardData randomCardData1 = new HeroCardData
        //{
        //    Id = "0100",
        //    HeroType = HeroeType.LightningMaster,
        //    RarityType = TypeRarityHero.Common
        //};
        //HeroCardData randomCardData2 = new HeroCardData
        //{
        //    Id = "0100",
        //    HeroType = HeroeType.LightningMaster,
        //    RarityType = TypeRarityHero.Common
        //};
        //HeroCardData randomCardData3 = new HeroCardData
        //{
        //    Id = "0300",
        //    HeroType = HeroeType.Ninja,
        //    RarityType = TypeRarityHero.Common
        //};
        //HeroCardData randomCardData4 = new HeroCardData
        //{
        //    Id = "0100",
        //    HeroType = HeroeType.LightningMaster,
        //    RarityType = TypeRarityHero.Common
        //};
        //cardDataList.Add(randomCardData1);
        //cardDataList.Add(randomCardData2);
        //cardDataList.Add(randomCardData3);
        //cardDataList.Add(randomCardData4);
        //foreach (var card in cardDataList)
        //{
        //    SpawnCard(card);
        //}
        // Start spawning cards
    //    StartCoroutine(SpawnCards());
    //    ResumeMoveCards();
    //}

    public void ResetCards()
    {
        // Pause card movement to avoid conflicts during reset
        PauseMoveCards();

        // Destroy all existing cards on the conveyor belt
        foreach (GameObject card in cards)
        {
            Destroy(card);
        }
        cards.Clear();

        // Clear the card data list
        cardDataList.Clear();

        // Reset card count
        cardCount = 0;

        // Reset the progress bar
        if (spawnProgressBar != null)
        {
            spawnProgressBar.value = 0;
        }

        // Stop any ongoing spawning coroutine
        StopAllCoroutines();
    }

    IEnumerator SpawnCards()
    {
        yield return new WaitForSeconds(1f);
        while (true)
        {
            while (cards.Count > 8)
            {
                yield return null; 
            }

            if (spawnProgressBar != null)
            {
                spawnProgressBar.value = 0;
            }
            HeroCardData randomCardData = GenerateRandomHeroCardData2();
            cardDataList.Add(randomCardData);
            SpawnCard(randomCardData);
            cardCount++;
            float elapsedTime = 0f;
            while (elapsedTime < spawnInterval)
            {
                elapsedTime += Time.deltaTime;

                if (spawnProgressBar != null)
                {
                    spawnProgressBar.value = elapsedTime;
                }
                yield return null;
            }
        }
    }
    private void SpawnInitialCards(int count)
    {
        float fastCardDuration = 1.5f;
        float lastDelayTime = 0f;
        PauseMoveCards();
        for (int i = 0; i < count; i++)
        {
            HeroCardData randomCardData = GenerateRandomHeroCardData();
            cardDataList.Add(randomCardData);

            // Spawn all cards at the same position
            GameObject newCard = Instantiate(cardPrefab, conveyorBelt);
            newCard.transform.position = pointInit.position;
            newCard.GetComponent<HeroCardUI>().Setup(randomCardData, this);
            newCard.name = "Card" + cardCount;
            cards.Add(newCard);
            cardCount++;

            RectTransform cardRect = newCard.GetComponent<RectTransform>();
            Vector2 targetAnchoredPos = pointEnd.anchoredPosition + new Vector2(i * cardSpacing, 0);

            float delayTime = i * 0.25f; // Adjusted delay for medium speed
            lastDelayTime = delayTime;

            Sequence jumpSequence = DOTween.Sequence();
            cardRect.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            jumpSequence.AppendInterval(delayTime);

            jumpSequence.Append(cardRect.DOAnchorPos(targetAnchoredPos + new Vector2(0, 50f), fastCardDuration * 0.3f)
                .SetEase(Ease.OutExpo)); // More explosive up motion

            jumpSequence.Append(cardRect.DOAnchorPos(targetAnchoredPos, fastCardDuration * 0.25f)
                .SetEase(Ease.OutBack, 2.2f)); // Medium overshoot for balanced bounce

            // Scale up during the jump - medium speed
            jumpSequence.Insert(delayTime, cardRect.DOScale(1.2f, fastCardDuration * 0.35f)
                .SetEase(Ease.OutBack, 1.9f));

            jumpSequence.Insert(delayTime + fastCardDuration * 0.35f, cardRect.DOScale(1f, fastCardDuration * 0.2f)
                .SetEase(Ease.OutQuad));

            CanvasGroup canvasGroup = newCard.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0.7f;
                jumpSequence.Insert(delayTime, canvasGroup.DOFade(1f, fastCardDuration * 0.15f));
            }

            jumpSequence.InsertCallback(delayTime, () => {
                if (Audio_Manager.instance != null)
                {
                    Audio_Manager.instance.play("Collect");
                }
            });
            
            // Call AnimationEnd after the last card's animation is complete
            if (i == count - 1)
            {
                float totalDuration = delayTime * fastCardDuration;
                DOVirtual.DelayedCall(totalDuration, AnimationEnd);
                ResumeMoveCards();
            }
        }
    }
    public void StartAnimation()
    {
        maskRect.gameObject.SetActive(true);
        Vector2 position = rectTransform.anchoredPosition;
        position.y = 0;
        rectTransform.anchoredPosition = position;
    }
    private void AnimationEnd()
    {
        maskRect.gameObject.SetActive(false);
        rectTransform.DOAnchorPosY(-800f, 0.5f).SetEase(Ease.InOutBack);
        UIManager.instance.SetTutorial(0);
    }

    void SpawnCard(HeroCardData cardData)
    {
        GameObject newCard = Instantiate(cardPrefab, conveyorBelt);
        newCard.transform.position = pointInit.position; // Spawn at the right edge
        newCard.GetComponent<HeroCardUI>().Setup(cardData, this); // Initialize card with data
        newCard.name = "Card" + cardCount;
        cards.Add(newCard);
    }

    public void PauseMoveCards()
    {
        isMoveCardsPaused = true;
    }

    public void ResumeMoveCards()
    {
        isMoveCardsPaused = false;
    }

    void Update()
    {
        MoveCards();
        CheckForMerge();
    }
    void MoveCards()
    {
        if (isMoveCardsPaused) return;

        float maxDuration = 4f; 
        Vector2 firstCardTargetPos = pointEnd.anchoredPosition;
        Vector2 firstCardPos = pointInit.anchoredPosition;
        float maxDistance = Vector2.Distance(firstCardPos, firstCardTargetPos);

        for (int i = 0; i < cards.Count; i++)
        {
            RectTransform cardRect = cards[i].GetComponent<RectTransform>();
            if (cardRect == null) continue;

            Vector2 targetAnchoredPos = pointEnd.anchoredPosition + new Vector2(i * cardSpacing, 0);
            float distance = Vector2.Distance(cardRect.anchoredPosition, targetAnchoredPos);

            if (distance > 0.05f)
            {
                DOTween.Kill(cardRect);

                float duration = maxDuration * (distance / maxDistance);

                cardRect.DOAnchorPos(targetAnchoredPos, duration)
                        .SetEase(Ease.Linear)
                        .SetId(cardRect);
            }
        }
    }


    void CheckForMerge()
    {
        if (isMoveCardsPaused) return;
        if (isMoveCardsPaused || isMerging) return;
        Vector2 startAnchoredPos = pointEnd.anchoredPosition;

        for (int i = 0; i < cards.Count - 2; i++)
        {
            HeroCardUI card1 = cards[i].GetComponent<HeroCardUI>();
            HeroCardUI card2 = cards[i + 1].GetComponent<HeroCardUI>();
            HeroCardUI card3 = cards[i + 2].GetComponent<HeroCardUI>();
            if (card1.CardData.RarityType == TypeRarityHero.Godlike)
                continue;
            if (card1.DraggableCard.IsDrag || card2.DraggableCard.IsDrag)
                continue;
            RectTransform rect1 = cards[i].GetComponent<RectTransform>();
            RectTransform rect2 = cards[i + 1].GetComponent<RectTransform>();
            RectTransform rect3 = cards[i + 2].GetComponent<RectTransform>();

            if (countMerge == 3)
            {
                Vector2 target1 = startAnchoredPos + new Vector2(i * cardSpacing, 0);
                Vector2 target2 = startAnchoredPos + new Vector2((i + 1) * cardSpacing, 0);
                Vector2 target3 = startAnchoredPos + new Vector2((i + 2) * cardSpacing, 0);
                if (Vector2.Distance(rect1.anchoredPosition, target1) <= 0.05f &&
                    Vector2.Distance(rect2.anchoredPosition, target2) <= 0.05f
                    && Vector2.Distance(rect3.anchoredPosition, target3) <= 0.05f)
                {
                    if (card1.CardData.Id == card2.CardData.Id &&
                        card2.CardData.Id == card3.CardData.Id)
                    {
                        MergeCards(i, card1.CardData);
                        break;
                    }
                }
            }
            else
            {
                Vector2 target1 = startAnchoredPos + new Vector2(i * cardSpacing, 0);
                Vector2 target2 = startAnchoredPos + new Vector2((i + 1) * cardSpacing, 0);
                if (Vector2.Distance(rect1.anchoredPosition, target1) <= 0.05f && Vector2.Distance(rect2.anchoredPosition, target2) <= 1f)
                {
                    if (card1.CardData.Id == card2.CardData.Id && card1.CardData.RarityType == card2.CardData.RarityType)
                    {
                        MergeCards(i, card1.CardData);
                        break;
                    }
                }
            }
        }
    }

    //void MergeCards(int startIndex, HeroCardData baseCardData)
    //{
    //    // Destroy the three cards
    //    for (int i = 0; i < countMerge; i++)
    //    {
    //        Destroy(cards[startIndex].gameObject);
    //        cards.RemoveAt(startIndex);
    //    }

    //    HeroeType heroType = baseCardData.HeroType;
    //    TypeRarityHero rarityType = (TypeRarityHero)((int)baseCardData.RarityType + 1);
    //    string id = ((int)heroType).ToString("D2") + ((int)rarityType).ToString("D2");

    //    // Create a new card with higher rarity
    //    HeroCardData newCardData = new HeroCardData
    //    {
    //        Id = id,
    //        HeroType = heroType,
    //        RarityType = rarityType
    //    };

    //    // Spawn the new card at the merge position
    //    GameObject newCard = Instantiate(cardPrefab, conveyorBelt);
    //    newCard.GetComponent<HeroCardUI>().Setup(newCardData, this);

    //    // Handle positioning of the new card
    //    if (startIndex == 0)
    //    {
    //        // If merging the first three cards, place the new card at pointEnd
    //        newCard.transform.position = pointEnd.position;
    //    }
    //    else
    //    {
    //        // Otherwise, place the new card at the position of the previous card
    //        newCard.transform.position = cards[startIndex - 1].transform.position + new Vector3(cardSpacing, 0, 0);
    //    }

    //    // Insert the new card into the list
    //    cardDataList[startIndex] = newCardData;
    //    cards.Insert(startIndex, newCard);

    //    // Add merge effect using DoTween
    //    newCard.transform.DOScale(1.2f, 0.2f) // Scale up slightly
    //        .OnComplete(() =>
    //        {
    //            newCard.transform.DOScale(1f, 0.2f); // Return to original size
    //        });
    //}
    void MergeCards(int startIndex, HeroCardData baseCardData)
    {
        isMerging = true;
        List<GameObject> cardsMerge = new List<GameObject>();
        for (int i = 0; i < countMerge; i++)
        {
            GameObject card = cards[startIndex + i];
            cardsMerge.Add(card);
            HeroCardUI cardUI = card.GetComponent<HeroCardUI>();
            if (cardUI != null && cardUI.GlowUnder != null)
            {
                cardUI.GlowUnder.SetActive(true);
                cardUI.DraggableCard.enabled = false;
            }
        }

        StartCoroutine(MergeEffectCoroutine(startIndex, baseCardData));
    }

    private IEnumerator MergeEffectCoroutine(int startIndex, HeroCardData baseCardData)
    {
        yield return new WaitForSeconds(0.2f);
        Vector3 posNewCard = cards[startIndex].transform.position;
        for (int i = 0; i < countMerge; i++)
        {
            Destroy(cards[startIndex].gameObject);
            cards.RemoveAt(startIndex);
        }
        TypeRarityHero rarityType = (TypeRarityHero)((int)baseCardData.RarityType + 1);
        HeroCardData newCardData = null;
        if (baseCardData.CardType == TypeCard.Hero)
        {
            HeroeType heroType = baseCardData.HeroType;
            string id = ((int)heroType).ToString("D2") + ((int)rarityType).ToString("D2");
            newCardData = new HeroCardData
            {
                Id = id,
                HeroType = heroType,
                RarityType = rarityType,
                CardType = TypeCard.Hero,
                SkillData = baseCardData.SkillData
            };
        }
        else
        {
            newCardData = new HeroCardData
            {
                Id = baseCardData.Id,
                RarityType = rarityType, 
                SkillData = baseCardData.SkillData,
                CardType = TypeCard.Skill
            };
        }

        GameObject newCard = Instantiate(cardPrefab, conveyorBelt);
        newCard.GetComponent<HeroCardUI>().Setup(newCardData, this);


        if (startIndex == 0)
        {
            newCard.transform.position = pointEnd.position;
        }
        else
        {
            newCard.transform.position = posNewCard;
        }


        cardDataList[startIndex] = newCardData;
        cards.Insert(startIndex, newCard);

        HeroCardUI newCardUI = newCard.GetComponent<HeroCardUI>();
        if (newCardUI != null && newCardUI.GlowWhite != null)
        {
            newCardUI.GlowWhite.SetActive(true); 
            newCard.transform.DOScale(1.2f, 0.2f) 
                .OnComplete(() =>
                {
                    newCard.transform.DOScale(1f, 0.2f);
                    Audio_Manager.instance.play("sfx_card_merge");
                    UIManager.instance.CloseTutorial(TutorialStepID.merge_card_more);
                });
        }
        isMerging = false;
        yield return new WaitForSeconds(1f);
        if(newCardUI != null)
          newCardUI.GlowWhite.SetActive(false);
    }
    void SpawnCardAtIndex(int index, HeroCardData cardData)
    {
        GameObject newCard = Instantiate(cardPrefab, conveyorBelt);
        newCard.GetComponent<HeroCardUI>().Setup(cardData, this);
        cards.Insert(index, newCard);
    }

    public void OnCardDraggedOut(GameObject card, HeroCardData cardData)
    {
        int index = cards.IndexOf(card);
        if (index != -1)
        {
            // Darken the card on the conveyor belt
            card.GetComponent<CanvasGroup>().alpha = 0.5f;
        }
    }

    public void OnCardDropped(GameObject card, HeroCardData cardData, bool spawnHero, SlotBehavior slot)
    {
        int index = cards.IndexOf(card);
        if (index != -1)
        {
            PlayerController.Instance.SpawnCharacter(slot, cardData.HeroType, cardData.RarityType, cardData);
            Destroy(card);
            cards.RemoveAt(index);
            UIManager.instance.CloseTutorial(TutorialStepID.summon_hero);
            UIManager.instance.SetTutorial(TutorialStepID.merge_card);
        }
    }

    private HeroCardData GenerateRandomHeroCardData2()
    {
        bool isHeroCard = Random.Range(0, 2) == 0;
        if (PlayerController.Instance.CountSpawnHero < GameManager.GameConfig.CountCheckSpawnHero)
            isHeroCard = true;
        TypeRarityHero highestRarity = TypeRarityHero.Common;
        if (isHeroCard)
        {
            // Generate Hero Card
            HeroeType heroeType = RandomHeroTypeExcluding();
            foreach (var card in cardDataList)
            {
                if (card.HeroType == heroeType)
                {
                    if ((int)card.RarityType > (int)highestRarity)
                    {
                        highestRarity = card.RarityType;
                    }
                }
            }
            TypeRarityHero selectedRarity = GetRandomRarity(highestRarity);

            string idHero = ((int)heroeType).ToString("D2");
            string id = idHero + ((int)selectedRarity).ToString("D2");

            HeroCardData hero = DataManager.Instance.CardsCollectionData.Find(card => card.Id == idHero);
            int level = 1;
            if (hero != null)
                level = hero.Level;
            SkillDatabase skillHeroDatabase = DataManager.Instance.SkillHeroDatabaseConfig;
            SkillData[] heroSkills = System.Array.FindAll(skillHeroDatabase.GetAllSkills(), s => s.Id == "10" + idHero);
            return new HeroCardData
            {
                Id = id,
                HeroType = heroeType,
                RarityType = selectedRarity,
                Level = level,
                CardType = TypeCard.Hero,
                SkillData = heroSkills[0]
            };
        }
        else
        {
            // Generate Skill Card
            SkillDatabase skillDatabase = DataManager.Instance.SkillPassiveDatabaseConfig; // Assume SkillDatabase is accessible
            SkillData[] passiveSkills = System.Array.FindAll(skillDatabase.GetAllSkills(), s => s.abilityType == AbilityType.Passive);

            if (passiveSkills.Length == 0)
            {
                Debug.LogError("No passive skills available in the database!");
                return null;
            }

            SkillData randomSkill = passiveSkills[Random.Range(0, passiveSkills.Length)];
            foreach (var card in cardDataList)
            {
                if (card.Id == randomSkill.Id)
                {
                    if ((int)card.RarityType > (int)highestRarity)
                    {
                        highestRarity = card.RarityType;
                    }
                }
            }
            TypeRarityHero selectedRarity = GetRandomRarity(highestRarity);

            return new HeroCardData
            {
                Id = randomSkill.Id,
                SkillData = randomSkill, // Assign SkillData
                RarityType = selectedRarity,
                CardType = TypeCard.Skill // Set as Skill card
            };
        }
    }

    private HeroCardData GenerateRandomHeroCardData()
    {
        HeroCardRarityConfig rarityConfig = DataManager.Instance.RarityHeroCardConfig;
        // Check the rarity of cards already in the cardDataList
        TypeRarityHero highestRarity = TypeRarityHero.Common;
        HeroeType heroeType = RandomHeroTypeExcluding();
        foreach (var card in cardDataList)
        {
            if (card.HeroType == heroeType)
            {
                if ((int)card.RarityType > (int)highestRarity)
                {
                    highestRarity = card.RarityType;
                }
            }
        }

        // Define probabilities based on the highest rarity found
        int commonChance = 0, rareChance = 0, epicChance = 0, legendaryChance = 0, godlikeChance = 0;
        switch (highestRarity)
        {
            case TypeRarityHero.Common:
                commonChance = rarityConfig.defaultCommonChance;
                rareChance = rarityConfig.defaultRareChance;
                epicChance = rarityConfig.defaultEpicChance;
                legendaryChance = rarityConfig.defaultLegendaryChance;
                godlikeChance = rarityConfig.defaultGodlikeChance;
                break;
            case TypeRarityHero.Rare: // Rare
                commonChance = rarityConfig.rareCommonChance;
                rareChance = rarityConfig.rareRareChance;
                epicChance = rarityConfig.rareEpicChance;
                legendaryChance = rarityConfig.rareLegendaryChance;
                godlikeChance = rarityConfig.rareGodlikeChance;
                break;
            case TypeRarityHero.Epic:
                commonChance = rarityConfig.epicCommonChance;
                rareChance = rarityConfig.epicRareChance;
                epicChance = rarityConfig.epicEpicChance;
                legendaryChance = rarityConfig.epicLegendaryChance;
                godlikeChance = rarityConfig.epicGodlikeChance;
                break;
            case TypeRarityHero.Legendary:
                commonChance = rarityConfig.legendaryCommonChance;
                rareChance = rarityConfig.legendaryRareChance;
                epicChance = rarityConfig.legendaryEpicChance;
                legendaryChance = rarityConfig.legendaryLegendaryChance;
                godlikeChance = rarityConfig.legendaryGodlikeChance;
                break;
            case TypeRarityHero.Godlike:
                commonChance = rarityConfig.godlikeCommonChance;
                rareChance = rarityConfig.godlikeRareChance;
                epicChance = rarityConfig.godlikeEpicChance;
                legendaryChance = rarityConfig.godlikeLegendaryChance;
                godlikeChance = rarityConfig.godlikeGodlikeChance;
                break;
        }

        // Generate a random number to determine the rarity
        int randomValue = Random.Range(1, 101); // Random number between 1 and 100
        TypeRarityHero selectedRarity;

        if (randomValue <= commonChance)
        {
            selectedRarity = TypeRarityHero.Common;
        }
        else if (randomValue <= commonChance + rareChance)
        {
            selectedRarity = TypeRarityHero.Rare;
        }
        else if (randomValue <= commonChance + rareChance + epicChance)
        {
            selectedRarity = TypeRarityHero.Epic;
        }
        else if (randomValue <= commonChance + rareChance + epicChance + legendaryChance)
        {
            selectedRarity = TypeRarityHero.Legendary;
        }
        else
        {
            selectedRarity = TypeRarityHero.Godlike;
        }
        string idHero = ((int)heroeType).ToString("D2");
        string id = idHero + ((int)selectedRarity).ToString("D2");
        HeroCardData hero = DataManager.Instance.CardsCollectionData.Find(card => card.Id == idHero);
        // Generate a random HeroCardData with the selected rarity
        return new HeroCardData
        {
            Id = id,
            HeroType = heroeType,
            RarityType = selectedRarity,
            Level = hero.Level
        };
    }
    private TypeRarityHero GetRandomRarity(TypeRarityHero highestRarity)
    {
        HeroCardRarityConfig rarityConfig = DataManager.Instance.RarityHeroCardConfig;
        int commonChance = 0, rareChance = 0, epicChance = 0, legendaryChance = 0, godlikeChance = 0;
        switch (highestRarity)
        {
            case TypeRarityHero.Common:
                commonChance = rarityConfig.defaultCommonChance;
                rareChance = rarityConfig.defaultRareChance;
                epicChance = rarityConfig.defaultEpicChance;
                legendaryChance = rarityConfig.defaultLegendaryChance;
                godlikeChance = rarityConfig.defaultGodlikeChance;
                break;
            case TypeRarityHero.Rare: // Rare
                commonChance = rarityConfig.rareCommonChance;
                rareChance = rarityConfig.rareRareChance;
                epicChance = rarityConfig.rareEpicChance;
                legendaryChance = rarityConfig.rareLegendaryChance;
                godlikeChance = rarityConfig.rareGodlikeChance;
                break;
            case TypeRarityHero.Epic:
                commonChance = rarityConfig.epicCommonChance;
                rareChance = rarityConfig.epicRareChance;
                epicChance = rarityConfig.epicEpicChance;
                legendaryChance = rarityConfig.epicLegendaryChance;
                godlikeChance = rarityConfig.epicGodlikeChance;
                break;
            case TypeRarityHero.Legendary:
                commonChance = rarityConfig.legendaryCommonChance;
                rareChance = rarityConfig.legendaryRareChance;
                epicChance = rarityConfig.legendaryEpicChance;
                legendaryChance = rarityConfig.legendaryLegendaryChance;
                godlikeChance = rarityConfig.legendaryGodlikeChance;
                break;
            case TypeRarityHero.Godlike:
                commonChance = rarityConfig.godlikeCommonChance;
                rareChance = rarityConfig.godlikeRareChance;
                epicChance = rarityConfig.godlikeEpicChance;
                legendaryChance = rarityConfig.godlikeLegendaryChance;
                godlikeChance = rarityConfig.godlikeGodlikeChance;
                break;
        }

        int randomValue = Random.Range(1, 101); // Random number between 1 and 100
        TypeRarityHero selectedRarity;

        if (randomValue <= commonChance)
        {
            selectedRarity = TypeRarityHero.Common;
        }
        else if (randomValue <= commonChance + rareChance)
        {
            selectedRarity = TypeRarityHero.Rare;
        }
        else if (randomValue <= commonChance + rareChance + epicChance)
        {
            selectedRarity = TypeRarityHero.Epic;
        }
        else if (randomValue <= commonChance + rareChance + epicChance + legendaryChance)
        {
            selectedRarity = TypeRarityHero.Legendary;
        }
        else
        {
            selectedRarity = TypeRarityHero.Godlike;
        }
        return selectedRarity;
    }
    private HeroeType RandomHeroTypeExcluding()
    {
        if (TutorialManager.GetTutorial(0) == 0 && cardCount < Define.heroesInTutorial.Count)
        {
            return Define.heroesInTutorial[cardCount];
        }
        List<HeroCardData> buildHeroes = DataManager.Instance.CardsCollectionData
            .FindAll(card => card.CardState == CardState.Build);

        if (buildHeroes.Count == 0)
        {
            return HeroeType.Ninja;
        }
        List<HeroeType> validHeroTypes = buildHeroes.ConvertAll(card => card.HeroType);
        int randomIndex = Random.Range(0, validHeroTypes.Count);
        return validHeroTypes[randomIndex];
    }

    public void RemoveCard(GameObject card, TypeRarityHero typeRarity,bool isAddExperience)
    {
        cards.Remove(card);
        if(isAddExperience)
          UIManager.instance.UIGameplayCtr.AddExperience(typeRarity);
    }
}