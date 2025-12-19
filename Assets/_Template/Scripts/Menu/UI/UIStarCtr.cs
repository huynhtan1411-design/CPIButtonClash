using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEngine;
using UnityEngine.UI;

public class UIStarCtr : MonoBehaviour
{
    [SerializeField] private Image[] starImages;
    [SerializeField] private Image evoStarImage;
    [SerializeField] private GameObject groupStar;
    private Coroutine blinkCoroutine;
    private int blinkingIndex = -1;
    public void DisplayStars(int countStar, bool isShowUpgrade = false)
    {
        if (countStar <= 5)
        {
            groupStar.SetActive(true);
            for (int i = 0; i < starImages.Length; i++)
            {
                if (i < countStar)
                {
                    starImages[i].gameObject.SetActive(true);
                }
                else
                {
                    starImages[i].gameObject.SetActive(false);
                }

            }
            if (evoStarImage != null)
                evoStarImage.gameObject.SetActive(false);
            if(isShowUpgrade)
              StartBlinking(countStar + 1);
        }
        else
        {
            if (evoStarImage != null)
            {
                evoStarImage.gameObject.SetActive(true);
            }
            groupStar.SetActive(false);
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
            starImages[blinkingIndex].gameObject.SetActive(false);
            blinkingIndex = -1;
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
