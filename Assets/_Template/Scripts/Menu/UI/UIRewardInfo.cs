using System.Collections;
using System.Collections.Generic;
using UISystems;
using UnityEngine;
using TMPro;
using ButtonClash.UI;

public class UIRewardInfo : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private UIItemList uIItemList;

    public void Setup(RectTransform parent, List<ItemInfo> lstItems)
    {
        StopAllCoroutines();
        canvasGroup.alpha = 1.0f;
        canvasGroup.blocksRaycasts = true;
        rectTransform.SetParent(parent, false);
        uIItemList.Setup(lstItems);

        StartCoroutine(AutoHideAfterDelay(3f));
    }

    private IEnumerator AutoHideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Hide();
    }

    public void Hide()
    {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0f;
    }
}
