using System;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillTypeColors", menuName = "Configs/SkillTypeColors", order = 1)]
public class SkillTypeColors : ScriptableObject
{
    [System.Serializable]
    public struct SkillTypeColorPair
    {
        public SkillType type;
        public Color color;
    }

    public SkillTypeColorPair[] colorMapping;

    public Color GetColor(SkillType type)
    {
        var pair = Array.Find(colorMapping, p => p.type == type);
        return pair.color;
    }
}