using CLHoma.Combat;
using UnityEngine;

namespace CLHoma.LevelSystem
{
    [System.Serializable]
    public class LevelData
    {
        [SerializeField] RoundData[] roundDatas;
        public RoundData[] RoundDatas => roundDatas;

        [SerializeField] int totalLevelTime = 300000;
        public int TotalLevelTime => totalLevelTime;

        [SerializeField] int intervalEachEnemy = 500;
        public int IntervalEachEnemy => intervalEachEnemy;

        [SerializeField] int intervalEachRound = 2500;
        public int IntervalEachRound => intervalEachRound;
    }

    [System.Serializable]
    public class RoundData
    {
        [SerializeField] LevelType type;
        public LevelType Type => type;

        [SerializeField] EnemyData[] enemyEntities;
        public EnemyData[] EnemyEntities => enemyEntities;

        [SerializeField] int enemiesAmount;
        public int EnemiesAmount => enemiesAmount;
    }
}