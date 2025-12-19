using UnityEngine;


namespace UISystems
{
    public class UIToggleGroup : MonoBehaviour
    {

        UIToggle current;

        public UIToggle Current
        {
            get { return current; }
        }

        public void ChangeTab(UIToggle obj)
        {
            if (current != null)
                current.Visible = false;
            current = obj;
            current.Visible = true;
        }

        public void HideTab(UIToggle obj)
        {
            if (obj == current)
                current = null;
            obj.Visible = false;
        }
    }
}