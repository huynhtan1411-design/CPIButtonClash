using DG.Tweening;
using System.Collections;
using UnityEngine;

public class PopOut : MonoBehaviour
{
    [SerializeField] private bool playEnable = false;
    private RectTransform rect;
    public float delay;
    public bool inplace;
    public float threshold = 1.2f;
    public float height = 9f;
    Vector2 oldPos;
    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        oldPos = Vector2.zero;
        if (inplace) transform.localScale = Vector3.zero;
        else
        {
            oldPos = rect.anchoredPosition;

        }
    }

    void OnEnable()
    {
        if(playEnable)
          StartCoroutine(DelayPopOut());
    }

    public void PlayShowAnim()
    {
        StartCoroutine(DelayPopOut());
    }
    IEnumerator DelayPopOut()
    {
        if (inplace) transform.localScale = Vector3.zero;
        else
        {
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -Screen.height * 1.5f);
        }
        yield return new WaitForSeconds(delay);
        if (!inplace)
        {

            rect.DOAnchorPosY(oldPos.y + Screen.height / height, 0.4f).SetEase(Ease.OutQuad).SetUpdate(true).OnComplete(() =>
            {
                rect.DOAnchorPosY(oldPos.y, 0.3f).SetEase(Ease.OutQuad).SetUpdate(true);
            });
        }
        else
        {
            transform.localScale = Vector3.one * 1 / threshold;
            transform.DOScale(Vector3.one * threshold, 0.5f).SetEase(Ease.OutQuad).SetUpdate(true).OnComplete(() =>
            {
                transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutQuad);
            });
        }


    }

}
