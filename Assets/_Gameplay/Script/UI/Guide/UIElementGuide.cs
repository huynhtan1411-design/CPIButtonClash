using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UISystems;
using DG.Tweening;

namespace CLHoma
{
    public class UIElementGuide : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private RectTransform buttonNext;
        [SerializeField] private RectTransform buttonClose;
        [SerializeField] private CanvasGroup canvasGroup;
        private int indexStep = 0;

        private void OnEnable()
        {
            // Reset transform and alpha
            transform.localScale = Vector3.one * 0.8f;
            canvasGroup.alpha = 0f;

            // Animate
            transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
            canvasGroup.DOFade(1f, 0.3f);

            var guildConfig = GameManager.GameConfig.guide;
            indexStep = 0;
            if (guildConfig.Length > 0)
            {
                description.text = guildConfig[indexStep];
            }

            buttonClose.gameObject.SetActive(false);
            buttonNext.gameObject.SetActive(true);
        }

        public void OnNext()
        {
            var guildConfig = GameManager.GameConfig.guide;
            indexStep++;
            description.text = guildConfig[indexStep];

            buttonClose.gameObject.SetActive(true);
            buttonNext.gameObject.SetActive(false);
        }

        public void OnClose()
        {
            // Animate out
            transform.DOScale(0.8f, 0.2f).SetEase(Ease.InBack);
            canvasGroup.DOFade(0f, 0.2f).OnComplete(() => {
                UIManager.instance.CloseTutorialElement();
                gameObject.SetActive(false);
            });
        }
    }
}