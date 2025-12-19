using System.Collections;
using System.Collections.Generic;
using TemplateSystems;
using UnityEngine;

[CreateAssetMenu(fileName = "HeroCardUpgradeCostConfig", menuName = "Configs/HeroCardUpgradeCostConfig", order = 9)]
public class HeroCardUpgradeCostConfig : ScriptableObject
{
    public List<HeroCardUpgradeCostData> Data = new List<HeroCardUpgradeCostData>();
}
