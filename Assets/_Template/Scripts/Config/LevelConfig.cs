using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Configs/LevelConfig", order = 1)]
public class LevelConfig : ScriptableObject
{
    [System.Serializable]
    public class SkillCardData
    {
        public string cardId; 
        public string iconId; 
        public string cardName; 
        public string description; 
        public SkillType skillType; 
        public int requiredLevel;
        public float skillValue;
        public int starCount;
        public ElementType elementType;
    }

    public List<SkillCardData> skillCards = new List<SkillCardData>();
}