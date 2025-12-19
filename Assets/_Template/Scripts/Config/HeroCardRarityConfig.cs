using UnityEngine;

[CreateAssetMenu(fileName = "HeroCardRarityConfig", menuName = "Configs/HeroCardRarityConfig", order = 1)]
public class HeroCardRarityConfig : ScriptableObject
{
    [Header("Default Rarity Probabilities")]
    [Range(0, 100)] public int defaultCommonChance = 100;
    [Range(0, 100)] public int defaultRareChance = 0;
    [Range(0, 100)] public int defaultEpicChance = 0;
    [Range(0, 100)] public int defaultLegendaryChance = 0;
    [Range(0, 100)] public int defaultGodlikeChance = 0;

    [Header("Rarity Probabilities for Rare Cards")]
    [Range(0, 100)] public int rareCommonChance = 80;
    [Range(0, 100)] public int rareRareChance = 20;
    [Range(0, 100)] public int rareEpicChance = 0;
    [Range(0, 100)] public int rareLegendaryChance = 0;
    [Range(0, 100)] public int rareGodlikeChance = 0;

    [Header("Rarity Probabilities for Epic Cards")]
    [Range(0, 100)] public int epicCommonChance = 70;
    [Range(0, 100)] public int epicRareChance = 25;
    [Range(0, 100)] public int epicEpicChance = 5;
    [Range(0, 100)] public int epicLegendaryChance = 0;
    [Range(0, 100)] public int epicGodlikeChance = 0;

    [Header("Rarity Probabilities for Legendary Cards")]
    [Range(0, 100)] public int legendaryCommonChance = 60;
    [Range(0, 100)] public int legendaryRareChance = 25;
    [Range(0, 100)] public int legendaryEpicChance = 10;
    [Range(0, 100)] public int legendaryLegendaryChance = 5;
    [Range(0, 100)] public int legendaryGodlikeChance = 0;

    [Header("Rarity Probabilities for Godlike Cards")]
    [Range(0, 100)] public int godlikeCommonChance = 60;
    [Range(0, 100)] public int godlikeRareChance = 25;
    [Range(0, 100)] public int godlikeEpicChance = 10;
    [Range(0, 100)] public int godlikeLegendaryChance = 5;
    [Range(0, 100)] public int godlikeGodlikeChance = 0;
}