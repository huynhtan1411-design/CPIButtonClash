using System.Collections;
using System.Collections.Generic;
using TemplateSystems;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveDataConfig", menuName = "Configs/WaveDataConfig", order = 10)]
public class WaveDataConfig : ScriptableObject
{
    public List<WaveData> Data = new List<WaveData>();
}
