using UnityEngine;

namespace CLHoma
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "CLHoma/Game Config", order = 1)]
    public class GameConfig : ScriptableObject
    {
        [Header("Experience Settings")]
        public int baseExperience = 600;
        public int CountCheckSpawnHero = 5;
        public float factorGrowthExperience = 1.2f;
        public int baseExperiencePerClick = 60;
        public int silverStart = 20;

        [Header("Element Interaction")]
        public float weaknessFactor = 0.5f; // gain damage
        public float resistanceFactor = 0.5f; // lose damage

        public string[] guide =
        {
            "Zombies take <color=#FF2424>50% less damage</color> from the element which they can <color=#FFA500>resist</color>",
            "And receive <color=#33FF00>50% more damage</color> from the counter element"
        };

        [Header("Combo System")]
        public float comboDecayTime = 1f;
        public ComboData[] comboTiers = new ComboData[5]
        {
            new ComboData { clicksRequired = 5, multiplier = 2 },
            new ComboData { clicksRequired = 50, multiplier = 3 },
            new ComboData { clicksRequired = 100, multiplier = 4 },
            new ComboData { clicksRequired = 200, multiplier = 5 },
            new ComboData { clicksRequired = 500, multiplier = 6 }
        };

        [Header("EnemySystem")]
        public Vector3 SpawnPointEnemy = new Vector3(0, 0, 0);
        public float FactorPos = 1.5f;
    }
}