using UnityEngine;

namespace CLHoma.Combat
{
    [CreateAssetMenu(fileName = "Enemies Database", menuName = "Content/Enemies Database")]
    public class EnemiesDatabase : ScriptableObject
    {
        [SerializeField] private EnemyData[] enemies;
        public EnemyData[] Enemies => enemies;

        public EnemyData GetEnemyData(EnemyType type)
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i].EnemyType == type)
                    return enemies[i];
            }
            Debug.LogError($"[Enemies Database] Enemy of type {type} is not found!");
            return enemies[0];
        }

        public EnemyData GetEnemyData(EnemyType type, ElementType elementType)
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i].EnemyType == type && enemies[i].ElementType == elementType)
                    return enemies[i];
            }
            Debug.LogError($"[Enemies Database] Enemy of type {type} with element {elementType} is not found!");
            return enemies[0];
        }
    }
}