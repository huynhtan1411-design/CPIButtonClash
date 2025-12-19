using DG.Tweening;
using UnityEngine;

public class TutorialHand : MonoBehaviour
{
    [SerializeField] private RectTransform hand;

    void OnEnable()
    {
        if (hand != null)
        {
            hand.DOScale(Vector3.one * 1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        }
    }

    void OnDisable()
    {
        if (hand != null)
            DOTween.Kill(hand);
    }
}
