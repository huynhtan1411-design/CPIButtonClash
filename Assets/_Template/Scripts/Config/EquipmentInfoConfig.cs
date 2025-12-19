using UnityEngine;
using System.Collections.Generic;
using TemplateSystems;

[CreateAssetMenu(fileName = "EquipmentInfoConfig", menuName = "Configs/EquipmentInfoConfig", order = 2)]
public class EquipmentInfoConfig : ScriptableObject
{
    public List<EquipmentInfData> Data = new List<EquipmentInfData>();
}
