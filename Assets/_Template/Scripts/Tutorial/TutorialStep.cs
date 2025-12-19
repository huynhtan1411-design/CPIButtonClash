using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;
[System.Serializable]
public class TutorialStep : MonoBehaviour
{
    [SerializeField] private Image overlay; 
    [SerializeField] private RectTransform clickableArea;
    [SerializeField] private RectTransform unmask;
    [SerializeField] private int tutorialId; 
    private Vector3 initialUnmaskScale;

    public UnityEvent onUnmaskSequenceComplete;
    void Awake()
    {
        if (unmask != null)
        {
            initialUnmaskScale = unmask.localScale;
        }
    }
    void OnEnable()
    {
        if (overlay != null)
            overlay.gameObject.SetActive(true);

        if (unmask != null)
        {
            unmask.localScale = initialUnmaskScale;

            Sequence unmaskSequence = DOTween.Sequence();
            unmaskSequence.Append(unmask.DOScale(initialUnmaskScale * 50f, 0.1f))
                          .Append(unmask.DOScale(initialUnmaskScale, 1.3f))
                          .SetEase(Ease.OutQuad).OnComplete(() =>
                          {
                              onUnmaskSequenceComplete?.Invoke();
                          });
        }

        //if (clickableArea.GetComponent<Button>() != null)
        //{
        //    clickableArea.GetComponent<Button>().onClick.AddListener(OnClickableAreaClicked);
        //}
    }

    void OnDisable()
    {
        if (overlay != null)
            overlay.gameObject.SetActive(false);

        if (unmask != null)
        {
            DOTween.Kill(unmask);
            unmask.localScale = initialUnmaskScale;
        }

        //if (clickableArea.GetComponent<Button>() != null)
        //{
        //    clickableArea.GetComponent<Button>().onClick.RemoveListener(OnClickableAreaClicked);
        //}
    }

    private void OnClickableAreaClicked()
    {
        gameObject.SetActive(false);
    }
}
