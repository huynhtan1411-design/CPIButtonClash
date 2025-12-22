using UnityEngine;
using System.Collections.Generic;
using CLHoma.Combat;
using TemplateSystems;
using Unity.VisualScripting.FullSerializer;
using System.Linq;
namespace WD
{
    [System.Serializable]
    public class BuildingLevelData
    {
        [Header("General")]
        [TextArea(1, 1)]
        public string name;
        public Sprite icon;
        public GameObject towerModel;
        [TextArea(3, 10)]
        public string description;

        [Header("Stats")]
        public float percentAddHealth;
        public float percentAddDamage;
        public float attackRange;
        public float attackSpeed;

        [Header("Special Effects")]
        public float radiusExplosion; 
        public float damageExplosion;

        [Range(1, 5)]
        public int maxTargets = 1; 

        [Header("Upgrade & unlock")]
        public int upgradeCost;
        public int levelUnlockCondition;
        public Vector3 modelOffset = Vector3.zero;

        private float health;
        private float attackDamage;

        public float Health { get => health; set => health = value; }
        public float AttackDamage { get => attackDamage; set => attackDamage = value; }
    }

    [CreateAssetMenu(fileName = "BuildingData", menuName = "BuildingConfig/BuildingData")]
    public class BuildingData : ScriptableObject
    {
        [Header("Identification")]
        public string ID;
        public string nameBase;
        public float HealthBase;
        public float AttackDamageBase;
        private int levelCard;
        public int LevelIndexUnlock;
        public BuildingType Type;
        public BuildingLevelData[] levelConfigs;
        public GameObject Prefab;

        public int LevelCard { get => levelCard; set => levelCard = value; }

        private float GetTotalPercentAddDamage(int level)
        {
            float totalPercent = 0f;
            // Sum up percentAddDamage from level 0 to current level
            for (int i = 0; i <= level - 1; i++)
            {
                if (i < levelConfigs.Length)
                {
                    totalPercent += levelConfigs[i].percentAddDamage;
                }
            }
            return totalPercent;
        }

        private float GetTotalPercentAddHealth(int level)
        {
            float totalPercent = 0f;
            // Sum up percentAddHealth from level 0 to current level
            for (int i = 0; i <= level - 1; i++)
            {
                if (i < levelConfigs.Length)
                {
                    totalPercent += levelConfigs[i].percentAddHealth;
                }
            }
            return totalPercent;
        }

        public float GetDamageUpgrade(int level)
        {
            var dataLevel = GetLevelData(level);
            float ugradeDamage = AttackDamageBase + (AttackDamageBase * (LevelCard - 1) * 10f / 100f);
            float totalPercentAdd = GetTotalPercentAddDamage(level);
            float currentDamage = ugradeDamage + (ugradeDamage * totalPercentAdd);
            
            int roundedUpgradedDamage = Mathf.RoundToInt(currentDamage);
            return roundedUpgradedDamage;
        }

        public float GetHealthUpgrade(int level)
        {
            var dataLevel = GetLevelData(level);
            float ugradeHealth = HealthBase + (HealthBase * (LevelCard - 1) * 10f / 100f);
            float totalPercentAdd = GetTotalPercentAddHealth(level);
            float currentHealth = ugradeHealth + (ugradeHealth * totalPercentAdd);

            int roundedUpgradedHealth = Mathf.RoundToInt(currentHealth);
            Debug.LogError($"Level {level} health: {roundedUpgradedHealth} with total percentAdd: {totalPercentAdd}");
            return roundedUpgradedHealth;
        }
        public int GetCostUpgrade(int level)
        {
            if (level <= 0 || level > levelConfigs.Length)
            {
                Debug.LogError($"Invalid tower level: {level}. Valid range is 1 to {levelConfigs.Length}");
                return 0;
            }
            return levelConfigs[level - 1].upgradeCost;
        }
        public BuildingLevelData GetLevelData(int level)
        {
            if (level <= 0 || level > levelConfigs.Length)
            {
                Debug.LogError($"Invalid tower level: {level}. Valid range is 1 to {levelConfigs.Length}");
                return null;
            }
            return levelConfigs[level - 1];
        }

        public BuildingLevelData GetLevelDataUpgrade(int level)
        {
            if (level <= 0 || level > levelConfigs.Length)
            {
                Debug.LogError($"Invalid tower level: {level}. Valid range is 1 to {levelConfigs.Length}");
                return null;
            }
            var buildingCard = DataManager.Instance.GetBuildingCollectionItem(ID);
            if (buildingCard != null)
                levelCard = buildingCard.Level;
            else
                levelCard = 1;
            BuildingLevelData levelData = levelConfigs[level - 1];
            float healthUpgrade = GetHealthUpgrade(level);
            float damageUpgrade = GetDamageUpgrade(level);
            BuildingLevelData levelDataUpgrade = levelData;
            levelDataUpgrade.Health = healthUpgrade;
            levelDataUpgrade.AttackDamage = damageUpgrade;
            return levelDataUpgrade;
        }
    }
//    [CreateAssetMenu(fileName = "BuildingConfig", menuName = "BuildingConfig/BuildingConfig")]
//    public class BuildingConfig : ScriptableObject
//    {
//        [Header("Building Data")]
//        public List<BuildingData> Data = new List<BuildingData>();

//        public List<BuildingData> InitializeData => Data.Where(x => x.LevelIndexUnlock == 0).ToList();
//    }
}
