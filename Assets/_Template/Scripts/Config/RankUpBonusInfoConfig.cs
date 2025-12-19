using System.Collections.Generic;
using TemplateSystems;
using UnityEngine;

[CreateAssetMenu(fileName = "RankUpBonusInfoData", menuName = "Configs/RankUpBonusInfoData", order = 8)]
public class RankUpBonusInfoConfig : ScriptableObject
{
    public List<RankUpBonusInfoData> Data = new List<RankUpBonusInfoData>();
}
