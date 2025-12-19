using CLHoma.DGTween;
using TMPro;
using UnityEngine;

namespace CLHoma.Combat
{
    public class FloatingTextHitBehaviour : FloatingTextBaseBehaviour
    {
        [SerializeField] TextMeshProUGUI floatingText;

        [Space]
        [SerializeField] float delay;
        [SerializeField] float disableDelay;
        [SerializeField] float scale;
        [SerializeField] float time;
        [SerializeField] Ease.Type easing;

        [Space]
        [SerializeField] float scaleTime;
        [SerializeField] Ease.Type scaleEasing;

        private Vector3 defaultScale;

        private void Awake()
        {
            defaultScale = transform.localScale;
        }

        public override void Activate(string text, float scale = 1.0f)
        {
            floatingText.text = text;

            int sign = Random.value >= 0.5f ? 1 : -1;

            transform.localScale = defaultScale * scale * this.scale;
            transform.localRotation = Quaternion.Euler(45, 0, 18 * sign);

            Tween.DelayedCall(delay, delegate
            {
                transform.DOLocalRotate(Quaternion.Euler(45, 0, 0), time).SetEasing(easing).OnComplete(delegate
                {
                    Tween.DelayedCall(disableDelay, delegate
                    {
                        gameObject.SetActive(false);
                    });
                });
                transform.DOScale(defaultScale, scaleTime).SetEasing(scaleEasing);
                transform.DOMove(transform.position + new Vector3(0, 0.15f, 0), time * 1.5f).SetEasing(easing);
            });

            transform.localPosition = Utils.GetRandomPositionAround(transform.position, 0.5f);
        }

        
    }
}