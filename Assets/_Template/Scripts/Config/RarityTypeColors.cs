using System;
using System.Linq;
using UnityEngine;

namespace TemplateSystems
{
    [CreateAssetMenu(fileName = "RarityTypeColors", menuName = "Configs/RarityTypeColors", order = 1)]
    public class RarityTypeColors : ScriptableObject
    {
        [System.Serializable]
        public struct RarityTypeColorPair
        {
            public TypeRarity type;
            public Color color;
        }

        public RarityTypeColorPair[] colorMapping;

        public Color GetColor(TypeRarity type)
        {
            var pair = Array.Find(colorMapping, p => p.type == type);
            return pair.color;
        }
    }
}
