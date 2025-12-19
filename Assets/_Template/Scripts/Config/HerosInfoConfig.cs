using System.Collections.Generic;
using TemplateSystems;
using UnityEngine;

[CreateAssetMenu(fileName = "HerosInfoConfig", menuName = "Configs/HerosInfoConfig", order = 3)]
public class HerosInfoConfig : ScriptableObject
{
    public List<HerosInfoData> Data = new List<HerosInfoData>();
}

