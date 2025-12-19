#if UNITY_EDITOR
#endif
using UnityEngine;

namespace CLHoma
{
    [CreateAssetMenu(menuName = "Skills/Effect/Passive")]
    public class PassiveSkillEffect : SkillEffect
    {
        [System.Serializable]
        class EffectDescription
        {
            public int level;
            [TextArea] public string description;
        }

        public StatType statType;
        public float value;
        [TextArea] public string description;
        public bool isPercentageFormat;

        [SerializeField] private EffectDescription[] specialDescriptions;

        public override float GetEffectValue(int level)
        {
            float result = ScalingFunction.Evaluate(level) * value;
            return Mathf.Round(result * 100f) / 100f;
        }

        public float GetDeltaEffectValue(int levelSkill)
        {
            float currentValue = GetEffectValue(levelSkill);
            float previousValue = (levelSkill > 0) ? GetEffectValue(levelSkill - 1) : 0f;
            return currentValue - previousValue;
        }

        public override string GetEffectValueString(int level)
        {
            string result = string.Empty;
            result = GetOnDescription(level);
            if (result != string.Empty) return result;

            string value = isPercentageFormat ? $"{GetEffectValue(level) * 100}%" : GetEffectValue(level).ToString();
            result = string.Format(description, value);
            return result;
        }

        private string GetOnDescription(int level)
        {
            if (specialDescriptions == null || specialDescriptions.Length == 0) return string.Empty;

            var specialDesc = System.Array.Find(specialDescriptions, x => x.level == level);
            if (specialDesc != null)
            {
                return specialDesc.description;
            }

            return string.Empty;
        }
    }
}

