using System.Collections.Generic;
using TemplateSystems;
using UnityEngine;
using ButtonClash.UI;
using System.Linq;
public enum ClearType
{
    None = -1,
    PartialClear = 0, // 50% Clear - slider 0
    FullClear = 1,    // Clear - slider 0.5
    PerfectClear = 2  // Perfect Clear - slider 1.0
}

[CreateAssetMenu(fileName = "ProgressRewardConfig", menuName = "Configs/ProgressRewardConfig", order = 3)]
public class ProgressRewardConfig : ScriptableObject
{
    [System.Serializable]
    public class ProgressMilestone
    {
        public ClearType clearType;
        public int coinReward;
        public TypeRarity[] equipments;
    }

    [System.Serializable]
    public class LevelRewards
    {
        public int levelId;
        public ProgressMilestone[] milestones = new ProgressMilestone[3];
    }
    
    public List<LevelRewards> levelRewards = new List<LevelRewards>();
    
    public List<ItemInfo> GetRewardsAtProgress(int levelId, ClearType clearType)
    {
        List<ItemInfo> rewards = new List<ItemInfo>();
        LevelRewards levelReward = levelRewards.Find(lr => lr.levelId == levelId);
        if (levelReward == null)
            return rewards;
            
        foreach (var milestone in levelReward.milestones)
        {
            if (clearType == milestone.clearType)
            {
                if (milestone.coinReward > 0)
                {
                    rewards.Add(new ItemInfo
                    {
                        Id = "2",
                        Quantity = milestone.coinReward,
                        ItemType = "Currency"
                    });
                }

                if (milestone.equipments != null && milestone.equipments.Length > 0)
                {
                    foreach (var rarityType in milestone.equipments)
                    {
                        string equipmentId = GetRandomEquipmentIdByRarity(rarityType);
                        if (!string.IsNullOrEmpty(equipmentId))
                        {
                            rewards.Add(new ItemInfo
                            {
                                Id = equipmentId,
                                Quantity = 1,
                                ItemType = "Equipment",
                                ColorRarity = DataManager.Instance.RarityTypeColorsData.GetColor(rarityType)
                            });
                        }
                    }
                }
            }
        }
        
        return rewards;
    }
    public List<ItemInfo> GetRewardInfo(int levelId, ClearType clearType)
    {
        List<ItemInfo> rewards = new List<ItemInfo>();
        LevelRewards levelReward = levelRewards.Find(lr => lr.levelId == levelId);
        if (levelReward == null)
            return rewards;

        foreach (var milestone in levelReward.milestones)
        {
            if (clearType == milestone.clearType)
            {
                if (milestone.coinReward > 0)
                {
                    rewards.Add(new ItemInfo
                    {
                        Id = "2",
                        Quantity = milestone.coinReward,
                        ItemType = "Currency"
                    });
                }

                if (milestone.equipments != null && milestone.equipments.Length > 0)
                {
                    foreach (var rarityType in milestone.equipments)
                    {
                        rewards.Add(new ItemInfo
                        {
                            Quantity = 1,
                            ItemType = "Equipment",
                            ColorRarity = DataManager.Instance.RarityTypeColorsData.GetColor(rarityType)
                        });
                    }
                }
            }
        }

        return rewards;
    }


    private string GetRandomEquipmentIdByRarity(TypeRarity rarityType)
    {
        var allEquipments = DataManager.Instance.EquipmentInfoData.Data;
        var equipmentsByRarity = allEquipments.Where(e => e.Rarity == rarityType).ToList();
        
        if (equipmentsByRarity != null && equipmentsByRarity.Count > 0)
        {
            int randomIndex = Random.Range(0, equipmentsByRarity.Count);
            return equipmentsByRarity[randomIndex].ID;
        }
        return null;
    }
    
    public bool HasRewardAtProgress(int levelId, ClearType progress)
    {
        LevelRewards levelReward = levelRewards.Find(lr => lr.levelId == levelId);
        if (levelReward == null)
            return false;
            
        foreach (var milestone in levelReward.milestones)
        {
            if (progress == milestone.clearType)
                return true;
        }
        
        return false;
    }
    
    [ContextMenu("Setup Default Levels")]
    public void SetupDefaultLevels()
    {
        levelRewards.Clear();
        
        // Level 1
        var level1 = new LevelRewards { levelId = 0 };
        level1.milestones = new ProgressMilestone[3];
        level1.milestones[0] = new ProgressMilestone {
            clearType = ClearType.PartialClear,
            coinReward = 1000,
            equipments = new TypeRarity[0]
        };
        level1.milestones[1] = new ProgressMilestone {
            clearType = ClearType.FullClear,
            coinReward = 0,
            equipments = new TypeRarity[] { TypeRarity.Common }
        };
        level1.milestones[2] = new ProgressMilestone {
            clearType = ClearType.PerfectClear,
            coinReward = 0,
            equipments = new TypeRarity[] { TypeRarity.Epic }
        };
        levelRewards.Add(level1);
        
        // Level 2
        var level2 = new LevelRewards { levelId = 1 };
        level2.milestones = new ProgressMilestone[3];
        level2.milestones[0] = new ProgressMilestone {
            clearType = ClearType.PartialClear,
            coinReward = 2000,
            equipments = new TypeRarity[0]
        };
        level2.milestones[1] = new ProgressMilestone {
            clearType = ClearType.FullClear,
            coinReward = 0,
            equipments = new TypeRarity[] { TypeRarity.Common }
        };
        level2.milestones[2] = new ProgressMilestone {
            clearType = ClearType.PerfectClear,
            coinReward = 0,
            equipments = new TypeRarity[] { TypeRarity.Epic }
        };
        levelRewards.Add(level2);
        
        // Level 3
        var level3 = new LevelRewards { levelId = 2 };
        level3.milestones = new ProgressMilestone[3];
        level3.milestones[0] = new ProgressMilestone {
            clearType = ClearType.PartialClear,
            coinReward = 3000,
            equipments = new TypeRarity[0]
        };
        level3.milestones[1] = new ProgressMilestone {
            clearType = ClearType.FullClear,
            coinReward = 0,
            equipments = new TypeRarity[] { TypeRarity.Common }
        };
        level3.milestones[2] = new ProgressMilestone {
            clearType = ClearType.PerfectClear,
            coinReward = 0,
            equipments = new TypeRarity[] { TypeRarity.Epic }
        };
        levelRewards.Add(level3);
        
        // Level 4
        var level4 = new LevelRewards { levelId = 3 };
        level4.milestones = new ProgressMilestone[3];
        level4.milestones[0] = new ProgressMilestone {
            clearType = ClearType.PartialClear,
            coinReward = 4000,
            equipments = new TypeRarity[0]
        };
        level4.milestones[1] = new ProgressMilestone {
            clearType = ClearType.FullClear,
            coinReward = 0,
            equipments = new TypeRarity[] { TypeRarity.Common }
        };
        level4.milestones[2] = new ProgressMilestone {
            clearType = ClearType.PerfectClear,
            coinReward = 0,
            equipments = new TypeRarity[] { TypeRarity.Epic }
        };
        levelRewards.Add(level4);
        
        // Level 5
        var level5 = new LevelRewards { levelId = 4 };
        level5.milestones = new ProgressMilestone[3];
        level5.milestones[0] = new ProgressMilestone {
            clearType = ClearType.PartialClear,
            coinReward = 5000,
            equipments = new TypeRarity[0]
        };
        level5.milestones[1] = new ProgressMilestone {
            clearType = ClearType.FullClear,
            coinReward = 0,
            equipments = new TypeRarity[] { TypeRarity.Common }
        };
        level5.milestones[2] = new ProgressMilestone {
            clearType = ClearType.PerfectClear,
            coinReward = 0,
            equipments = new TypeRarity[] { TypeRarity.Epic }
        };
        levelRewards.Add(level5);
        
        // Level 6
        var level6 = new LevelRewards { levelId = 5 };
        level6.milestones = new ProgressMilestone[3];
        level6.milestones[0] = new ProgressMilestone {
            clearType = ClearType.PartialClear,
            coinReward = 6000,
            equipments = new TypeRarity[0]
        };
        level6.milestones[1] = new ProgressMilestone {
            clearType = ClearType.FullClear,
            coinReward = 0,
            equipments = new TypeRarity[0]
        };
        level6.milestones[2] = new ProgressMilestone {
            clearType = ClearType.PerfectClear,
            coinReward = 0,
            equipments = new TypeRarity[0]
        };
        levelRewards.Add(level6);
    }
}

