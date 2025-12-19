using UnityEngine;
using System.Collections.Generic;

namespace WD
{
    [CreateAssetMenu(fileName = "LevelConfigSO", menuName = "WinterDefense/Level Config")]
    public class LevelConfigSO : ScriptableObject
    {
        public List<LevelConfig> levels = new List<LevelConfig>();
    }
} 