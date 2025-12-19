using UnityEngine;

namespace CLHoma
{
    [CreateAssetMenu(fileName = "PlayerStatsData", menuName = "CLHoma/Player Stats Data", order = 1)]
    public class PlayerStatsData : ScriptableObject
    {
        [Header("Basic Stats")]
        [SerializeField] private float initialMaxHealth = 100f;
        [SerializeField] private float initialProjectileSpeed = 1f;
        [SerializeField] private float initialCooldownReduction = 1f;
        [SerializeField] private float initialHealthRegeneration = 0f;
        [SerializeField] private float initialSkillDuration = 1f;
        [SerializeField] private float initialSkillRange = 1f;
        [SerializeField] private float initialAttackPower = 1f;
        [SerializeField] private float initialGoldGain = 0f;
        [SerializeField] private float initialDamageReduction = 0f;
        [SerializeField] private float initialMoveSpeed = 1f;

        [Header("Resource Stats")]
        [SerializeField] private float initialResourcePerTap = 1f;
        [SerializeField] private int initialAutoClick = 0;
        [SerializeField] private float initialResourceOnKill = 1f;
        [SerializeField] private float initialResourceMultiplier = 1f;
        [SerializeField] private float initialResourcePerSecond = 0f;

        [Header("Level Settings")]
        [SerializeField] private int initialLevel = 1;
        [SerializeField] private double initialExperience = 0;

        public float InitialMaxHealth => initialMaxHealth;
        public float InitialProjectileSpeed => initialProjectileSpeed;
        public float InitialCooldownReduction => initialCooldownReduction;
        public float InitialHealthRegeneration => initialHealthRegeneration;
        public float InitialSkillDuration => initialSkillDuration;
        public float InitialSkillRange => initialSkillRange;
        public float InitialAttackPower => initialAttackPower;
        public float InitialGoldGain => initialGoldGain;
        public float InitialDamageReduction => initialDamageReduction;
        public float InitialMoveSpeed => initialMoveSpeed;
        public float InitialResourcePerTap => initialResourcePerTap;
        public int InitialAutoClick => initialAutoClick;
        public float InitialResourceOnKill => initialResourceOnKill;
        public float InitialResourceMultiplier => initialResourceMultiplier;
        public int InitialLevel => initialLevel;
        public double InitialExperience => initialExperience;
        public float InitialResourcePerSecond => initialResourcePerSecond;
    }
}