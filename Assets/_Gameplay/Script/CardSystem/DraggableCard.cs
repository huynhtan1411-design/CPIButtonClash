using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;
using TemplateSystems;
using CLHoma;
using CLHoma.Combat;
using Unity.VisualScripting;
using UISystems;

public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private GameObject overlayObject;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private HeroCardUI heroCardUI;
    private Transform parentTrans;
    private MergeCardHeros mergeCardHeros;
    private HeroCardData cardData;
    private RectTransform conveyorBeltRect;
    private bool isOutOfConveyorBelt = false;
    private bool isDrag = false;
    private CharacterBehaviour targetCharacter;

    public Transform ParentTrans { get => parentTrans;}
    public bool IsDrag => isDrag;

    public void Initialize(MergeCardHeros mergeCardHeros, HeroCardData cardData)
    {
        this.mergeCardHeros = mergeCardHeros;
        this.cardData = cardData;
        parentTrans = transform.parent;
        conveyorBeltRect = mergeCardHeros.conveyorBelt.GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        mergeCardHeros.PauseMoveCards();    
        transform.SetParent(mergeCardHeros.cardDrag); 
        transform.position = eventData.position;
        isOutOfConveyorBelt = false;
        isDrag = true;
        targetCharacter = null;
    }

    public void OnDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.6f;
        Vector3 posTarget = eventData.position;
        posTarget.z = 0;
        transform.position = posTarget;

        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        RaycastHit hit;
        LayerMask playerMask1 = LayerMask.GetMask("Slot");

        PlayerController.Instance.UnhighlightAllSlot();
        PlayerController.Instance.ToggleRecommendSlot(cardData, true);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, playerMask1))
        {
            SlotBehavior slot = hit.collider.GetComponent<SlotBehavior>();
            if (slot != null && cardData.CardType == TypeCard.Hero)
            {
                //if (slot.IsEmpty())
                //{
                //    slot.SetHighlight(SlotBehavior.SlotState.Green);
                //}
                //else
                //{
                //    if (slot.CharacterBehaviour.CanUpgradeWithCard(cardData))
                //    {
                //        slot.SetHighlight(SlotBehavior.SlotState.Green);
                //    }
                //    else
                //    {
                //        slot.SetHighlight(SlotBehavior.SlotState.Red);
                //    }
                //}
            }
        }
        
        if (!RectTransformUtility.RectangleContainsScreenPoint(conveyorBeltRect, eventData.position))
        {
            mergeCardHeros.ResumeMoveCards();
            mergeCardHeros.cardEraseArea.gameObject.SetActive(true);
        }
        else
        {
            mergeCardHeros.cardEraseArea.gameObject.SetActive(false);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDrag = false;
        PlayerController.Instance.UnhighlightAllSlot();
        PlayerController.Instance.ToggleRecommendSlot(cardData, false);

        canvasGroup.alpha = 1f;
        mergeCardHeros.ResumeMoveCards();
        mergeCardHeros.cardEraseArea.gameObject.SetActive(false);
        bool characterUpgraded = false;
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        RaycastHit hit;
        LayerMask playerMask = LayerMask.GetMask("Slot");
        SlotBehavior slot = null;
      

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, playerMask))
          {
            slot = hit.collider.GetComponent<SlotBehavior>();
            if (slot.IsEmpty())
            {
                //if (slot.IsTutorialSummonSlotActive() || slot.IsTutorialMergeSlotActive())
                //{
                //    transform.SetParent(parentTrans);
                //    transform.DOLocalMove(Vector3.zero, 0.2f);
                //    return;
                //}

                if (cardData.CardType == TypeCard.Hero)
                {
                    mergeCardHeros.OnCardDropped(parentTrans.gameObject, cardData, true, slot);
                    Destroy(gameObject);
                }
                else
                {
                    transform.SetParent(parentTrans);
                    transform.DOLocalMove(Vector3.zero, 0.2f);
                }
            }
            else
            {
                //if (slot.IsTutorialMergeSlotActive())
                //{
                //    transform.SetParent(parentTrans);
                //    transform.DOLocalMove(Vector3.zero, 0.2f);
                //    return;
                //}
                //if (cardData.CardType == TypeCard.Hero)
                //{
                //    slot.HandleCharacter(cardData, () =>
                //    {
                //        RemoveCard();
                //    }, () =>
                //    {
                //        transform.SetParent(parentTrans);
                //        transform.DOLocalMove(Vector3.zero, 0.2f);
                //    });
                //}
                //else if (cardData.CardType == TypeCard.Skill)
                //{
                //    CharacterBehaviour character = slot.CharacterBehaviour;
                //    character.ApplySkill(cardData); // Apply skill to the character
                //    RemoveCard();
                //    characterUpgraded = true;
                //}

            }
            return;
        }
        if (characterUpgraded)
            return;

        if (TutorialManager.GetTutorial(0) == 0)
        {
            transform.SetParent(parentTrans);
            transform.DOLocalMove(Vector3.zero, 0.2f);
            return;
        }

        if (TutorialManager.GetTutorial(1) == 0)
        {
            transform.SetParent(parentTrans);
            transform.DOLocalMove(Vector3.zero, 0.2f);
            return;
        }
        if (!RectTransformUtility.RectangleContainsScreenPoint(conveyorBeltRect, eventData.position))
        {
            isOutOfConveyorBelt = true;
            if (RectTransformUtility.RectangleContainsScreenPoint(mergeCardHeros.cardEraseArea, eventData.position))
            {
                RemoveCard(true);
                return;
            }
            // Process as hero generation if the card is out of the conveyor belt
            bool spawnHero = !PlayerController.Instance.IsFullHeroes && cardData.CardType == TypeCard.Hero;

            if (spawnHero)
            {
                mergeCardHeros.OnCardDropped(parentTrans.gameObject, cardData, spawnHero, slot);
                Destroy(gameObject);
            }
            else
            {
                if (cardData.CardType == TypeCard.Hero)
                {
                    bool canUpgrade = PlayerController.Instance.UpgradeCharacter(cardData.HeroType, cardData.RarityType);
                    if (canUpgrade)
                    {
                        mergeCardHeros.OnCardDropped(parentTrans.gameObject, cardData, spawnHero, slot);
                        Destroy(gameObject);
                        return;
                    }
                }
                else
                {
                  
                    SkillManager.Instance.ApplyPassiveEffects(cardData);
                }
                Debug.LogError("not");
                transform.SetParent(parentTrans);
                transform.DOLocalMove(Vector3.zero, 0.2f);
            }
        }
        else
        {
            isOutOfConveyorBelt = false;
            // Check if the card is between two other cards
            for (int i = 0; i < mergeCardHeros.Cards.Count - 1; i++)
            {
                GameObject cardA = mergeCardHeros.Cards[i];
                GameObject cardB = mergeCardHeros.Cards[i + 1];

                if (cardA == gameObject || cardB == gameObject) continue;
                Vector3 positionA = cardA.transform.position;
                Vector3 positionB = cardB.transform.position;
                if (transform.position.x > positionA.x && transform.position.x < positionB.x)
                {
                    int currentIndex = mergeCardHeros.Cards.IndexOf(parentTrans.gameObject);
                    int targetIndex = i + 1;
                    if (currentIndex != -1 && targetIndex != -1 && currentIndex != targetIndex)
                    {
                        float distance = positionB.x - positionA.x;
                        if (distance < 300)
                        {
                            InsertCardAtPosition(targetIndex);
                            return;
                        }
                    }
                }
            }
            transform.SetParent(parentTrans);
            transform.DOLocalMove(Vector3.zero, 0.1f);
        }
    }
    private void InsertCardAtPosition(int targetIndex)
    {
        int currentIndex = mergeCardHeros.Cards.IndexOf(parentTrans.gameObject);
        if (currentIndex != -1 && targetIndex != -1 && currentIndex != targetIndex)
        {
            mergeCardHeros.PauseMoveCards();

            mergeCardHeros.Cards.RemoveAt(currentIndex);
            mergeCardHeros.Cards.Insert(targetIndex, parentTrans.gameObject);
   
            ((RectTransform)parentTrans).anchoredPosition = ((RectTransform)transform).anchoredPosition;
            transform.SetParent(parentTrans);

            DG.Tweening.Sequence moveSequence = DOTween.Sequence();
            Vector2 startAnchoredPos = mergeCardHeros.pointEnd.anchoredPosition;

            for (int i = 0; i < mergeCardHeros.Cards.Count; i++)
            {
                GameObject card = mergeCardHeros.Cards[i];
                RectTransform cardRect = card.GetComponent<RectTransform>();

                Vector2 targetPos = startAnchoredPos + new Vector2(i * mergeCardHeros.cardSpacing, 0f);
                if (card == parentTrans.gameObject)
                {
                    moveSequence.Join(((RectTransform)parentTrans).DOAnchorPos(targetPos, 0.2f));
                }
                else
                {
                    moveSequence.Join(cardRect.DOAnchorPos(targetPos, 0.2f));
                }

            }

            moveSequence.OnComplete(() =>
            {
                mergeCardHeros.ResumeMoveCards();
            });
        }
    }

    private void RemoveCard(bool isAddExperience = false)
    {
        Destroy(gameObject);
        Destroy(parentTrans.gameObject);
        TypeRarityHero typeRarity = heroCardUI.CardData.RarityType;
        mergeCardHeros.RemoveCard(parentTrans.gameObject, typeRarity, isAddExperience);
    }
}