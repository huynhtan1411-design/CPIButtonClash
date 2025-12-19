using UnityEngine;

namespace CLHoma
{
    public enum AbilityType
    {
        Active,
        Passive,
        Hero,
        Special
    }

    [CreateAssetMenu(menuName = "Skills/New Skill")]
    public class SkillData : ScriptableObject
    {
        public string Id;
        public string IdIcon;
        public string skillName;
        public SkillEffect[] effects;
        public AbilityType abilityType;
        public SkillType skillType;
        public int chapterIndexRepair;
    }

    public abstract class SkillEffect : ScriptableObject
    {
        [SerializeField] protected AnimationCurve ScalingFunction;
        public abstract float GetEffectValue(int level);
        public abstract string GetEffectValueString(int level);
    }
}
