using UnityEngine;

namespace UISystems
{
    public class UIToggleEffect : MonoBehaviour
    {

        public virtual void Visable()
        {
            gameObject.SetActive(true);
        }

        public virtual void Disable()
        {
            gameObject.SetActive(false);
        }
    }
}