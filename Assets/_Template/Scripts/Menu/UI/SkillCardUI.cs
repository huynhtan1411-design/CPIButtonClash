using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using DG.Tweening;
using UISystems;
public class SkillCardUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject notiImage;
    [SerializeField] private TextMeshProUGUI nameText; 
    [SerializeField] private TextMeshProUGUI descriptionText; 
    [SerializeField] private Button cardButton;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image[] starImages; // 5 images for normal stars
    [SerializeField] private Image evoStarImage; // Image for EVO star
    [SerializeField] private UIElemental uIElemental;

    private LevelConfig.SkillCardData cardData;
    private MenuGame menuGame;
    private Coroutine blinkCoroutine;
    private int blinkingIndex = -1;
    private int starCount;

    private bool hadClick = false;
    public void Setup(LevelConfig.SkillCardData data, int starCount, MenuGame mg)
    {
        cardData = data;
        menuGame = mg;

        nameText.text = data.cardName;
        descriptionText.text = data.description;
        uIElemental.Setup(data.elementType);
        if (!string.IsNullOrEmpty(cardData.iconId))
        {
            menuGame.LoadIcon(cardData.iconId, iconImage);
        }
        else
        {
            menuGame.LoadIcon(cardData.cardId, iconImage);
        }

        //SetBackgroundcolor(data.skillType);
        if(data.skillType == SkillType.SpawnHero)
            notiImage.SetActive(true);
        else
            notiImage.SetActive(false);
        this.starCount = starCount;

        if (starCount <= 5)
        {
            // Display normal stars
            for (int i = 0; i < starImages.Length; i++)
            {
                starImages[i].gameObject.SetActive(true);
                if (i < starCount)
                {
                    starImages[i].gameObject.SetActive(true);
                }
                else
                {
                    starImages[i].gameObject.SetActive(false);
                }
            }

            // If starCount < 5, blink the next star
            if (starCount < 5)
            {
                StartBlinking(starCount);
            }
            else
            {
                StopBlinking();
            }
            evoStarImage.gameObject.SetActive(false);
        }
        else
        {
            // Display EVO star
            for (int i = 0; i < starImages.Length; i++)
            {
                starImages[i].gameObject.SetActive(false);
            }
            evoStarImage.gameObject.SetActive(true);
            StopBlinking();
        }

        cardButton.onClick.RemoveAllListeners();
        cardButton.onClick.AddListener(OnCardClicked);

        gameObject.SetActive(true);
        hadClick = false;
    }

    private void OnCardClicked()
    {
        if (hadClick) return;
        hadClick = true;
        Audio_Manager.instance.play("Click");
        DOTween.Kill(transform);
        Sequence tweenSequence = DOTween.Sequence();
        tweenSequence.Append(transform.DOScale(new Vector3(1.1f, 1.1f, 1.1f), 0.1f).SetEase(Ease.OutSine));
        tweenSequence.Append(transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.InSine));
        tweenSequence.OnComplete(() => {
            tweenSequence.Kill();
            menuGame.ExecuteSkillAction(cardData);
        });

    }

    private void SetBackgroundcolor(SkillType type)
    {
        if (menuGame.SkillTypeColors != null)
        {
            try
            {
                Color color = menuGame.SkillTypeColors.GetColor(type);
                backgroundImage.color = color;
            }
            catch (InvalidOperationException)
            {
                backgroundImage.color = new Color(1f, 1f, 1f);
            }
        }
        else
        {
            backgroundImage.color = new Color(1f, 1f, 1f);
        }
    }

    private void StartBlinking(int index)
    {
        if (blinkCoroutine != null)
        {
            StopBlinking();
        }
        blinkingIndex = index;
        blinkCoroutine = StartCoroutine(BlinkStar(index));
    }

    private void StopBlinking()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        if (blinkingIndex != -1)
        {
            //starImages[blinkingIndex].gameObject.SetActive(false);
            //blinkingIndex = -1;
        }
    }

    private IEnumerator BlinkStar(int index)
    {
        if (index < starImages.Length)
        {
            while (true)
            {
                starImages[index].gameObject.SetActive(true);
                yield return new WaitForSeconds(0.5f);
                starImages[index].gameObject.SetActive(false);
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}