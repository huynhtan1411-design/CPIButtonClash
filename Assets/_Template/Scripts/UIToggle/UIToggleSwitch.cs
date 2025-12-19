using UnityEngine;
using UnityEngine.Events;


namespace UISystems
{
    public class UIToggleSwitch : MonoBehaviour
    {
        [SerializeField] UIToggle ToggleOn;
        [SerializeField] UIToggle ToggleOff;
        public UnityEvent<bool> OnChangeValue;
        private bool value = true;

        public void SetValue(bool value)
        {
            this.value = value;
            UpdateToggle();
        }

        private void UpdateToggle()
        {
            if (value)
                ToggleOn.Show();
            else ToggleOff.Show();
            OnChangeValue?.Invoke(value);
        }
    }
}