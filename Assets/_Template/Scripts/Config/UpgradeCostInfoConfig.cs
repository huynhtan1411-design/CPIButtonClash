using System.Collections.Generic;
using TemplateSystems;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeCostInfoConfig", menuName = "Configs/UpgradeCostInfoConfig", order = 7)]
public class UpgradeCostInfoConfig : ScriptableObject
{
    public List<UpgradeCostData> Data = new List<UpgradeCostData>();
}

