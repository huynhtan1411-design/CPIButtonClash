using UnityEngine;

namespace CLHoma
{
    [CreateAssetMenu(menuName = "Skills/Effect/Hero summon")]
    public class HeroSummonSkillEffect : SkillEffect
    {
        [System.Serializable]
        class HeroDescription
        {
            [SerializeField] int level;
            [TextArea] public string description;
        }
        [SerializeField] private HeroDescription[] heroDescriptions;
        [SerializeField] private HeroeType heroeType;
        public HeroeType HeroeType => heroeType;
        public override float GetEffectValue(int level)
        {
            return ScalingFunction.Evaluate(level);
        }

        public override string GetEffectValueString(int level)
        {
            if (level - 1 >= heroDescriptions.Length)
                return string.Empty;
            return heroDescriptions[level - 1].description.ToString();
        }
    }
}
