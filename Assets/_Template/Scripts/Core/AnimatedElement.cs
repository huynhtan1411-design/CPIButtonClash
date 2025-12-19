using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
namespace UISystems
{

    public class AnimatedElement : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public Ease tweenType;
        public float duration;

        public void OnPointerDown(PointerEventData eventData)
        {
            transform.DOScale(Vector3.one * 0.9f, duration).SetEase(tweenType);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            transform.DOScale(Vector3.one, duration).SetEase(tweenType);
        }
    }
}