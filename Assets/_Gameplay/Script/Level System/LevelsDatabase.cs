using UnityEngine;


namespace CLHoma.LevelSystem
{
    [CreateAssetMenu(fileName = "Levels Database", menuName = "Content/New Level/Levels Database")]
    public class LevelsDatabase : ScriptableObject
    {
        [SerializeField] LevelData[] levels;
        public LevelData[] LevelDatas => levels;

        public void Initialise()
        {

        }


        public LevelData GetRandomLevel()
        {
            return levels[Random.Range(0, levels.Length)];
        }

        public LevelData GetLevel(int levelIndex)
        {
            if (levels.IsInRange(levelIndex))
            {
                return levels[levelIndex];
            }
            return GetRandomLevel();
        }
    }
}
