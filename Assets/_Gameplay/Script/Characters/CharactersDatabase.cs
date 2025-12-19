using UnityEngine;

namespace CLHoma
{
    [CreateAssetMenu(fileName = "Character Database", menuName = "Content/Characters/Character Database")]
    public class CharactersDatabase : ScriptableObject
    {
        [SerializeField] Character[] characters;
        public Character[] Characters => characters;


        public Character GetCharacter(HeroeType type)
        {
            for (int i = 0; i < characters.Length; i++)
            {
                if (characters[i].Type == type)
                    return characters[i];
            }

            return null;
        }
        public ElementType GetElementType(HeroeType type)
        {
            for (int i = 0; i < characters.Length; i++)
            {
                if (characters[i].Type == type)
                    return characters[i].ElementType;
            }
            return ElementType.None;
        }
    }
}