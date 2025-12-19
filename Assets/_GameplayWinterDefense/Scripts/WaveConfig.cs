using UnityEngine;
using System;
using System.Collections.Generic;
using CLHoma.Combat;
using WD;

namespace WD
{
    [Serializable]
    public class EnemyGroup
    {
        public EnemyData enemyData;     // The enemy prefab to spawn
        public int count;                  // Number of enemies to spawn
        public float spawnInterval;        // Time between each enemy spawn
        public float delayBeforeSpawn; 
        //public bool spawnWithNextGroup = false; 
        public SpawnDirection spawnDirection; // Direction to spawn from
    }

    [Serializable]
    public class WaveConfig
    {
        public string waveName;
        public List<EnemyGroup> enemyGroups;
        public float delayBeforeWave;     // Delay before wave starts
    }
}