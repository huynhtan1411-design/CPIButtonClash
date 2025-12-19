using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WD;

namespace WD
{
    [CreateAssetMenu(fileName = "BuildingConfig", menuName = "BuildingConfig/BuildingConfig")]
    public class BuildingConfig : ScriptableObject
    {
        [Header("Building Data")]
        public List<BuildingData> Data = new List<BuildingData>();

        public List<BuildingData> InitializeData => Data.Where(x => x.LevelIndexUnlock == 0).ToList();
    }
}
