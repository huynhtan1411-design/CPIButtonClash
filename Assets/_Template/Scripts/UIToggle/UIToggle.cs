using UnityEngine;


namespace UISystems
{
    public class UIToggle : MonoBehaviour
    {

        public UIToggleGroup group;
        public UIToggleEffect effect;

        void Awake()
        {
            if (group == null)
                Debug.LogError(string.Format("sg toggle group null {0}", name));
            if (gameObject.activeSelf)
            {
                if (group.Current == null)
                    Show();
                else if (group.Current != this)
                    gameObject.SetActive(false);
            }
        }

        public bool Visible
        {
            set
            {

                if (effect != null)
                {
                    if (value)
                        effect.Visable();
                    else
                        effect.Disable();
                }
                else
                    gameObject.SetActive(value);
            }
            get { return gameObject.activeSelf; }
        }

        public void Show()
        {
#if UNITY_EDITOR
            if (group == null)
                Debug.LogError(gameObject.name);
#endif
            group.ChangeTab(this);
        }

        public void Hide()
        {
            group.HideTab(this);
        }
    }
}