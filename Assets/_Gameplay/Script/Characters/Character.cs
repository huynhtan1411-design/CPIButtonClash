using TemplateSystems;
using UnityEngine;

namespace CLHoma
{
    [System.Serializable]
    public class Character
    {
        [SerializeField] string id;
        public string Id => id;

        [SerializeField] HeroeType type;
        TypeRarityHero rarityType;
        public HeroeType Type => type;

        [SerializeField] GameObject prefab;
        public GameObject Prefab => prefab;

        [SerializeField] WeaponType weaponType;
        public WeaponType WeaponType => weaponType;

        [SerializeField] ElementType elementType;
        public ElementType ElementType => elementType;

        public TypeRarityHero RarityType { get => rarityType; set => rarityType = value; }
        public int Level = 0;
        public float AttackPower = 0f;
        public float CooldownReduction = 0f;
        public float MoveSpeed = 0f;
        public Character() { }

        public Character(string id, HeroeType type, GameObject prefab, 
                         WeaponType weaponType, ElementType elementType, TypeRarityHero rarityType)
        {
            this.id = id;
            this.type = type;
            this.prefab = prefab;
            this.weaponType = weaponType;
            this.elementType = elementType;
            this.rarityType = rarityType;
        }
    }
}