using UnityEngine;

namespace CLHoma
{
    public class UIAnimationAutoClicker : MonoBehaviour
    {
        [SerializeField] CanvasGroup _canvasGroup;
        [SerializeField] Animator _animatorHamer;
        [SerializeField] Animator _animatorClicker;
        [SerializeField] float _fadeDuration = 0.3f;

        private void Start()
        {
            _canvasGroup.alpha = 0f;
            _animatorClicker.enabled = false;
            _animatorHamer.enabled = false;
        }

        public void Setup(float speed)
        {
            _animatorClicker.enabled = true;
            _animatorHamer.enabled = true;
            _animatorHamer.speed = speed;
            _animatorClicker.speed = speed;
            _canvasGroup.alpha = 1;
        }

        public void Disable()
        {
            _canvasGroup.alpha = 0f;
            _animatorClicker.enabled = false;
            _animatorHamer.enabled = false;
        }

        public void ResetAnimation()
        {
            if (_animatorClicker != null)
            {
                _animatorClicker.Rebind();
                _animatorClicker.Update(0f);
            }
            
            if (_animatorHamer != null)
            {
                _animatorHamer.Rebind();
                _animatorHamer.Update(0f);
            }
        }
    }
}
