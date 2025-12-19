using System;
using System.Collections;
using UnityEngine;
namespace CLHoma
{
    public enum StatType
    {
        ProjectileSpeed,
        CooldownReduction,
        HealthRegeneration,
        SkillDuration,
        MaxHealth,
        SkillRange,
        AttackPower,
        GoldGain,
        DamageReduction,
        MoveSpeed,

        RestoreBaseHP,
        GoldPickupIncrease,
        IncreaseFireHeroATK,
        IncreaseIceHeroATK,
        IncreaseElectricHeroATK,
        IncreaseWindHeroATK,
        // Resource Skills
        ResourcePerTap,
        AutoClick,
        ResourceOnKill,
        ResourceMultiplier,
        ResourcePerSecond,

        // GET MORE
        Heal,
        GetGold

    }

    public class PlayerStatsManager
    {
        public Action OnChangeExperience;
        public Action OnChangeStats;
        public int Level { get; private set; }
        public double CurrentExperience { get; private set; }
        private float currentHealth;
        public float CurrentHealth
        {
            get => currentHealth;
            set
            {
                currentHealth = Mathf.Clamp(value, 0, maxHealth);
                OnChangeStats?.Invoke();
            }
        }

        public bool IsFullHealth => CurrentHealth >= maxHealth;

        // Additional stats
        public float projectileSpeed = 1f;
        public float cooldownReduction = 1f;
        public float healthRegeneration = 0f;
        public float skillDuration = 1f;
        public float maxHealth = 100f;
        public float skillRange = 1f;
        public float attackPower = 1f;
        public float goldGain = 0f;
        public float damageReduction = 0f;
        public float moveSpeed = 1f;

        // Resource stats
        public float increaseResourcePerTap = 1f;
        public int autoClick = 0;
        public float resourceOnKill = 1f;
        public float resourceMultiplier = 1f;

        public float resourcePerSecond = 0f;
        // Skills

        private PlayerStatsData statsData;

        public PlayerStatsManager(PlayerStatsData data)
        {
            statsData = data;
            InitializeStats();
        }

        public void InitializeStats()
        {
            Debug.LogError("InitializeStats");
            Level = statsData.InitialLevel;
            CurrentExperience = statsData.InitialExperience;

            maxHealth = statsData.InitialMaxHealth;
            currentHealth = maxHealth;

            projectileSpeed = statsData.InitialProjectileSpeed;
            cooldownReduction = statsData.InitialCooldownReduction;
            healthRegeneration = statsData.InitialHealthRegeneration;
            skillDuration = statsData.InitialSkillDuration;
            skillRange = statsData.InitialSkillRange;
            attackPower = statsData.InitialAttackPower;
            goldGain = statsData.InitialGoldGain;
            damageReduction = statsData.InitialDamageReduction;
            moveSpeed = statsData.InitialMoveSpeed;

            increaseResourcePerTap = statsData.InitialResourcePerTap;
            autoClick = statsData.InitialAutoClick;
            resourceOnKill = statsData.InitialResourceOnKill;
            resourceMultiplier = statsData.InitialResourceMultiplier;
            resourcePerSecond = statsData.InitialResourcePerSecond;

            OnChangeStats?.Invoke();
            OnChangeExperience?.Invoke();
        }

        public void AddExperience(double amount)
        {
            CurrentExperience += amount;
            CheckLevelUp();
        }
        public void AddExperienceOnKillEnemy(EnemyTier tier)
        {
            float resourceOnKillAmount = resourceOnKill * resourceMultiplier;
            CurrentExperience += resourceOnKillAmount;
            CheckLevelUp();
        }
        private void CheckLevelUp()
        {
            double experienceRequired = GetExperienceRequiredForNextLevel();
            if (CurrentExperience >= experienceRequired)
            {
                CurrentExperience -= experienceRequired;
                Level++;
                HandleLevelUp();
            }
            OnChangeExperience?.Invoke();
        }
        public double GetExperienceRequiredForNextLevel()
        {
            int baseExp = GameManager.GameConfig.baseExperience;
            //float facter = GameManager.GameConfig.factorGrowthExperience;
            return baseExp;// Utils.CalculateExpForNextLevel(Level + 1, baseExp, facter);
        }
        private void HandleLevelUp()
        {
            UISystems.UIManager.instance.SetMenuGame();
        }

        public bool SpendExperience(double amount)
        {
            if (CurrentExperience >= amount)
            {
                CurrentExperience -= amount;
                OnChangeExperience?.Invoke();
                return true;
            }
            return false;
        }

        public void UpdateStat(StatType type, float value)
        {
            Debug.LogWarning("ApplySkill Special " + type + " = " + value);
            switch (type)
            {
                case StatType.ProjectileSpeed:
                    projectileSpeed += value;
                    break;
                case StatType.CooldownReduction:
                    cooldownReduction += value;
                    break;
                case StatType.SkillDuration:
                    skillDuration += value;
                    break;
                case StatType.MaxHealth:
                    maxHealth *= (1f + value);
                    currentHealth *= (1f + value);
                    break;
                case StatType.SkillRange:
                    skillRange += value;
                    break;
                case StatType.AttackPower:
                    attackPower += value;
                    break;
                case StatType.GoldGain:
                    //goldGain += value;
                    break;
                case StatType.DamageReduction:
                    damageReduction += value;
                    break;
                case StatType.MoveSpeed:
                    moveSpeed += value;
                    break;
                case StatType.ResourcePerTap:
                    increaseResourcePerTap += value;
                    break;
                case StatType.AutoClick:
                    autoClick += (int)value;
                    break;
                case StatType.ResourceOnKill:
                    resourceOnKill += value;
                    break;
                case StatType.ResourceMultiplier:
                    resourceMultiplier += value;
                    break;
                case StatType.ResourcePerSecond:
                    resourcePerSecond += value;
                    break;
                case StatType.HealthRegeneration:
                    healthRegeneration += value;
                    Debug.LogWarning("HealthRegeneration value" + value);
                    break;
                case StatType.RestoreBaseHP:
                    float restoreAmount = maxHealth * value;
                    Debug.LogWarning(CurrentHealth + " RestoreBaseHP " + restoreAmount);
                    Heal(restoreAmount);
                    break;
                case StatType.GoldPickupIncrease: 
                    goldGain += value;
                    Debug.LogWarning("GoldPickupIncrease " + goldGain);
                    break;
                case StatType.IncreaseFireHeroATK:
                    IncreaseHeroAttackByElement(ElementType.Fire, value);
                    break;
                case StatType.IncreaseIceHeroATK:
                    IncreaseHeroAttackByElement(ElementType.Ice, value);
                    break;
                case StatType.IncreaseElectricHeroATK:
                    IncreaseHeroAttackByElement(ElementType.Electric, value);
                    break;
                case StatType.IncreaseWindHeroATK:
                    IncreaseHeroAttackByElement(ElementType.Wind, value);
                    break;
                default:
                    Debug.LogWarning("Stat type not found: " + type.ToString());
                    return;
            }
            OnChangeStats?.Invoke();
        }
        private void IncreaseHeroAttackByElement(ElementType elementType, float percentage)
        {
            if (PlayerController.Instance == null || PlayerController.Instance.Characters == null)
            {
                Debug.LogError("PlayerController or Characters list is null");
                return;
            }

            foreach (var character in PlayerController.Instance.Characters)
            {
                if (character.Data.ElementType == elementType)
                {
                    character.Data.AttackPower +=  percentage;
                    Debug.LogError($" {character.Data.ElementType} Increased ATK of {character.name} by {percentage * 100}%");
                }
            }
        }
        public void AddMaxHealth(float value)
        {
            maxHealth += value;
            currentHealth += value;
            OnChangeStats?.Invoke();
        }

        public void Heal(float amount)
        {
            CurrentHealth += amount;
            OnChangeStats?.Invoke();
        }
        public void TakeDamage(float amount)
        {
            float reducedAmount = amount * (1 - damageReduction);

            CurrentHealth -= reducedAmount;
            if (CurrentHealth <= 0)
            {
                GameManager.Instance.FailLevel();
                return;
            }
            Vibration_Manager.instance.Vibrate(MoreMountains.NiceVibrations.HapticTypes.LightImpact);
            OnChangeStats?.Invoke();
        }

        public IEnumerator RegenerateHealthOverTime()
        {
            while (true)
            {
                if (healthRegeneration > 0 && CurrentHealth < maxHealth)
                {
                    float regenAmount = maxHealth * healthRegeneration;
                    Debug.LogWarning("RegenerateHealthOverTime 5s HP+" + regenAmount);
                    Heal(regenAmount);
                }
                yield return new WaitForSeconds(5f);
            }
        }

        public IEnumerator RegenerateResourcePerSecond()
        {
            while (true)
            {
                if (resourcePerSecond > 0)
                {
                    float resourcePerSecondAmount = resourcePerSecond * resourceMultiplier;
                    AddExperience(resourcePerSecondAmount);
                }
                yield return new WaitForSeconds(1f);
            }
        }
    }
}
