using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CLHoma
{
    public class UIFloatingTextController : ManualSingletonMono<UIFloatingTextController>
    {
        [SerializeField] private GameObject floatingTextPrefab;
        [SerializeField] private int poolSize = 10;
        private Queue<GameObject> textPool = new Queue<GameObject>();


        private void Awake()
        {
            base.Awake();
            InitializePool();
        }

        private void InitializePool()
        {
            for (int i = 0; i < poolSize; i++)
            {
                GameObject textObj = Instantiate(floatingTextPrefab, transform);
                textObj.SetActive(false);
                textPool.Enqueue(textObj);
            }
        }

        public void ShowText(string text, Vector3 target, Transform parent)
        {
            if (textPool.Count == 0) return;

            GameObject textObj = textPool.Dequeue();
            textObj.SetActive(true);
            textObj.transform.position = target + new Vector3(0, 50, 0);
            textObj.transform.SetParent(parent);
            TextMeshProUGUI textComponent = textObj.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = text;
                textComponent.alpha = 1;

                textObj.transform.DOMoveY(textObj.transform.position.y + 50, 1f)
                    .SetEase(Ease.OutQuad).SetUpdate(true)
                    .OnComplete(() =>
                    {
                        textObj.SetActive(false);
                        textPool.Enqueue(textObj);
                    });

                textComponent.DOFade(0, 1f).SetUpdate(true);
            }
        }
    }
}
