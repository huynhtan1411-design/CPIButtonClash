using System.Collections.Generic;
using TemplateSystems;
using UnityEngine;

[CreateAssetMenu(fileName = "TalentsInfoConfig", menuName = "Configs/TalentsInfoConfig", order = 9)]
public class TalentsInfoConfig : ScriptableObject
{
    public List<TalentsInfoData> Data = new List<TalentsInfoData>();
}
