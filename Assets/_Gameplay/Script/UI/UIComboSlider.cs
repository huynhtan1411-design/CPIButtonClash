using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CLHoma
{
    public class UIComboSlider : MonoBehaviour
    {
        [SerializeField] private Slider comboSlider;
        [SerializeField] private Slider comboSlider_2;
        [SerializeField] private TextMeshProUGUI txtCombo;

        [SerializeField] private Image[] images; 
       
        private ComboData[] comboDatas;
        private float currentStreak = 0f;
        private float currentMultiplier = 1f;
        private int currentComboLevel = 0;

        private void Awake()
        {
            if (comboSlider == null)
            {
                comboSlider = GetComponentInChildren<Slider>();
            }
            
            if (txtCombo == null)
            {
                txtCombo = GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        public float GetCurrentMultiplier()
        {
            return currentMultiplier;
        }

        public float GetCurrentStreak()
        {
            return currentStreak;
        }

        public void SetComboDatas(ComboData[] datas)
        {
            comboDatas = datas;
            UpdateComboColors(0);
        }

        public void UpdateCombo(float streak)
        {
            if (comboSlider == null || txtCombo == null || comboDatas == null || comboDatas.Length == 0) return;

            // Update currentStreak but don't let it increase beyond max combo
            currentStreak = Mathf.Min(streak, comboDatas[comboDatas.Length - 1].clicksRequired);
            
            int previousLevel = currentComboLevel;
            UpdateComboLevel();
            UpdateComboSlider();
            
            if (currentComboLevel != previousLevel)
            {
                UpdateComboColors(currentComboLevel);
            }
        }

        private void UpdateComboLevel()
        {
            currentComboLevel = 0;
            for (int i = comboDatas.Length - 1; i >= 0; i--)
            {
                if (currentStreak >= comboDatas[i].clicksRequired)
                {
                    currentMultiplier = comboDatas[i].multiplier;
                    currentComboLevel = i;
                    return;
                }
            }
            currentMultiplier = 1f;
        }

        private void UpdateComboSlider()
        {
            txtCombo.text = string.Format("x{0}", currentMultiplier);
            
            if (currentStreak >= comboDatas[comboDatas.Length - 1].clicksRequired)
            {
                comboSlider.value = 1f;
                comboSlider_2.value = 1f;
                return;
            }

            for (int i = 0; i < comboDatas.Length; i++)
            {
                if (currentStreak < comboDatas[i].clicksRequired)
                {
                    float progress;
                    if (i == 0)
                    {
                        progress = currentStreak / comboDatas[0].clicksRequired;
                    }
                    else
                    {
                        float previousRequired = comboDatas[i - 1].clicksRequired;
                        float currentRequired = comboDatas[i].clicksRequired;
                        progress = (currentStreak - previousRequired) / (currentRequired - previousRequired);
                    }
                    comboSlider.value = progress;
                    comboSlider_2.value = progress;
                    return;
                }
            }
        }
        
        private void UpdateComboColors(int comboLevel)
        {
            if (images == null || images.Length == 0 || comboDatas == null || comboDatas.Length == 0) return;
            
            Color targetColor = Color.white;
            if (comboLevel < comboDatas.Length)
            {
                targetColor = comboDatas[comboLevel].Color;
            }
            
            foreach (Image img in images)
            {
                if (img != null)
                {
                    img.color = targetColor;
                }
            }
            
            if (comboSlider != null)
            {
                Image fillImage = comboSlider.fillRect?.GetComponent<Image>();
                if (fillImage != null)
                {
                    fillImage.color = targetColor;
                }
            }
        }

        public void ResetCombo()
        {
            currentStreak = 0f;
            currentMultiplier = 1f;
            currentComboLevel = 0;
            
            if (comboSlider != null)
            {
                comboSlider.value = 0f;
                comboSlider_2.value = 0f;
            }
            
            if (txtCombo != null)
            {
                txtCombo.text = "x1";
            }
            
            UpdateComboColors(0);
        }
    }
} 