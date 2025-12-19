using UnityEngine;
using TMPro;
using DG.Tweening;

public class TextNotiEffect : SingletonMono<TextNotiEffect>
{
    public TextMeshProUGUI textNoti;

    private Tween currentTween;
    private Sequence currentSequence;

    private void Start()
    {
        textNoti.gameObject.SetActive(false);
    }

    public void ShowNoti(string message)
    {
        currentTween?.Kill();
        currentSequence?.Kill();

        textNoti.text = message;
        textNoti.color = new Color(textNoti.color.r, textNoti.color.g, textNoti.color.b, 1f); 
        textNoti.rectTransform.localScale = Vector3.zero;
        textNoti.rectTransform.anchoredPosition = Vector2.zero;

        textNoti.gameObject.SetActive(true);

        currentSequence = DOTween.Sequence();

        currentSequence.Append(textNoti.rectTransform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack))
                       .Append(textNoti.rectTransform.DOAnchorPosY(250f, 2f).SetEase(Ease.OutQuad))
                       .Join(textNoti.DOFade(0f, 2f))
                       .OnComplete(() => textNoti.gameObject.SetActive(false));
    }
}