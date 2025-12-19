using System;
using System.Collections.Generic;
using UnityEngine;

namespace CLHoma
{
    [CreateAssetMenu(fileName = "SafeZoneConfig", menuName = "CLHoma/SafeZone Config")]
    public class SafeZoneConfig : ScriptableObject
    {
        [Serializable]
        public class SafeZoneLevelData
        {
            public int zoneLevel;
            public float safeZoneRadius;
            [TextArea(2, 4)]
            public string description;
            public int upgradeCost;
        }

        [Header("SafeZone Level Configuration")]
        [SerializeField] private List<SafeZoneLevelData> zoneData = new List<SafeZoneLevelData>();
        
        private Dictionary<int, SafeZoneLevelData> zoneDataMap;

        private void InitializeDictionary()
        {
            zoneDataMap = new Dictionary<int, SafeZoneLevelData>();
            
            foreach (var data in zoneData)
            {
                if (!zoneDataMap.ContainsKey(data.zoneLevel))
                {
                    zoneDataMap.Add(data.zoneLevel, data);
                }
                else
                {
                    Debug.LogWarning($"Duplicate zone level found: {data.zoneLevel}");
                }
            }
        }

        public float GetSafeZoneRadius(int zoneLevel)
        {
            if (zoneDataMap == null)
            {
                InitializeDictionary();
            }

            if (zoneDataMap.TryGetValue(zoneLevel, out SafeZoneLevelData data))
            {
                return data.safeZoneRadius;
            }

            // Return the highest available level data if current level not found
            float maxRadius = 0f;
            int highestLevel = 0;
            
            foreach (var entry in zoneDataMap)
            {
                if (entry.Key > highestLevel)
                {
                    highestLevel = entry.Key;
                    maxRadius = entry.Value.safeZoneRadius;
                }
            }
            
            Debug.LogWarning($"No specific data for zone level: {zoneLevel}, using highest available level: {highestLevel}");
            return maxRadius;
        }

        public List<SafeZoneLevelData> GetAllZoneData()
        {
            return new List<SafeZoneLevelData>(zoneData);
        }
    }
} 