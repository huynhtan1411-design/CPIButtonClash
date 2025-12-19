using UnityEngine;
using DG.Tweening;
using System.Collections;
using TMPro;
namespace WD
{
    public class TreeInteractiveBehaviour : MonoBehaviour
    {
        [Header("Tree Settings")]
        [SerializeField] private bool isDeadTree = false;

        [SerializeField] private int hitsToChop = 3;
        [SerializeField] private int amount = 1;
        [SerializeField] private Renderer renderer;
        [SerializeField] private Transform info;
        [SerializeField] private TextMeshProUGUI txtAmount;

        private int currentHits = 0;
        private bool isDestroyed = false;

        public void Init()
        {
            currentHits = 0;
            isDestroyed = false;
            if (renderer != null)
            {
                renderer.enabled = true;
            }
            if (info != null)
            {
                info.gameObject.SetActive(false);
            }
            gameObject.SetActive(true);
            StopAllCoroutines();
        }

        public void ChopTree(System.Action callback)
        {
            StartCoroutine(ChopTreeCoroutine(0.5f, callback));
        }
        public void CancelChop()
        {
            StopAllCoroutines();
            isDestroyed = false;
            transform.DOKill();
        }

        private IEnumerator ChopTreeCoroutine(float duration, System.Action callback)
        {
            while (currentHits < hitsToChop)
            {
                yield return new WaitForSeconds(duration);
                callback?.Invoke();
                currentHits++;
                PlayChopEffect();
                Audio_Manager.instance.play("sfx_chop_tree");
                if (currentHits >= hitsToChop)
                {
                    DestroyTree();
                    yield break;
                }
            }
        }

        private void PlayChopEffect()
        {
            Vector3 originalScale = transform.localScale;
            transform.DOScaleY(originalScale.y * 0.75f, 0.1f).SetEase(Ease.InOutSine)
                .OnComplete(() =>
                {
                    transform.DOScaleY(originalScale.y, 0.1f).SetEase(Ease.InOutSine);
                });
        }

        private void ShowInfoEffect()
        {
            if (info != null && txtAmount != null)
            {
                info.gameObject.SetActive(true);
                txtAmount.text = "+" + amount.ToString();
                info.rotation = Quaternion.Euler(-45f, 180f, 0f);
                info.DOLocalMoveY(info.localPosition.y + 1f, 0.75f).SetEase(Ease.OutQuad).OnComplete(() => {
                           info.gameObject.SetActive(false);
                       });
            }
        }

        private void DestroyTree()
        {
            if (isDestroyed) return;
            isDestroyed = true;

            if (BuildingManager.Instance != null && !isDeadTree)
            {
                BuildingManager.Instance.AddResources(amount);
                Audio_Manager.instance.play("Collect");
            }

            ShowInfoEffect();
            renderer.enabled = false;

            DOVirtual.DelayedCall(1f, () =>
            {
                gameObject.SetActive(false);
            });
        }

        public bool IsDestroyed => isDestroyed;
    }
}