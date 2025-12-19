using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TemplateSystems;

public static class EquipmentRandomizer
{
    [System.Serializable]
    public class RarityRate
    {
        public TypeRarity Rarity { get; set; }
        public float Rate { get; set; } // Rate from 0 to 100

        public RarityRate(TypeRarity rarity, float rate)
        {
            Rarity = rarity;
            Rate = rate;
        }
    }

    public static List<EquipmentInfData> GetRandomEquipmentsByRarity(
        List<EquipmentInfData> allEquipments,
        List<RarityRate> rarityRates,
        int count = 1)
    {
        if (allEquipments == null || allEquipments.Count == 0)
        {
            Debug.LogError("No equipment data available");
            return new List<EquipmentInfData>();
        }

        var result = new List<EquipmentInfData>();
        var equipmentByRarity = allEquipments.GroupBy(e => e.Rarity)
                                           .ToDictionary(g => g.Key, g => g.ToList());

        for (int i = 0; i < count; i++)
        {
            // Random rarity based on rates
            float randomValue = Random.Range(0f, 100f);
            float currentSum = 0f;
            TypeRarity selectedRarity = TypeRarity.Common; // Default

            foreach (var rate in rarityRates)
            {
                currentSum += rate.Rate;
                if (randomValue <= currentSum)
                {
                    selectedRarity = rate.Rarity;
                    break;
                }
            }

            // Get equipment list for selected rarity
            if (equipmentByRarity.TryGetValue(selectedRarity, out var rarityEquipments))
            {
                if (rarityEquipments.Count > 0)
                {
                    int randomIndex = Random.Range(0, rarityEquipments.Count);
                    result.Add(rarityEquipments[randomIndex]);
                }
            }
        }

        return result;
    }

    public static List<RarityRate> CreateDefaultRates()
    {
        return new List<RarityRate>
        {
            new RarityRate(TypeRarity.Common, 80f),
            new RarityRate(TypeRarity.Rate, 20f),
        };
    }
} 