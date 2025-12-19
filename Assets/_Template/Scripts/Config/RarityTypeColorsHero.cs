using System;
using UnityEngine;

namespace TemplateSystems
{
    [CreateAssetMenu(fileName = "RarityTypeColorsHero", menuName = "Configs/RarityTypeColorsHero", order = 1)]
    public class RarityTypeColorsHero : ScriptableObject
    {
        [System.Serializable]
        public struct RarityTypeColorPair
        {
            public TypeRarityHero type;
            public Color color;
        }

        public RarityTypeColorPair[] colorMapping;

        public Color GetColor(TypeRarityHero type)
        {
            var pair = Array.Find(colorMapping, p => p.type == type);
            return pair.color;
        }
    }
}