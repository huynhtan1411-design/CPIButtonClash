using UnityEngine;

namespace CLHoma.Combat
{
    [System.Serializable]
    public class RarityData
    {
        [SerializeField] Rarity rarity;
        public Rarity Rarity => rarity;

        [SerializeField] string name;
        public string Name => name;

        [SerializeField] Color mainColor;
        public Color MainColor => mainColor;

        [SerializeField] Color textColor;
        public Color TextColor => textColor;
    }
}