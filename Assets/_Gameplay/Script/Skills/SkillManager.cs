using System.Collections.Generic;
using System.Linq;
using TemplateSystems;
using UnityEngine;
using static LevelConfig;
namespace CLHoma
{
    public class SkillManager : ManualSingletonMono<SkillManager>
    {
        [SerializeField] private SkillDatabase database;
        public SkillDatabase Database => database;
        private List<SkillSpec> _lstSkill;
        public List<SkillSpec> GetSkills => _lstSkill;

        public void Initialize()
        {
            _lstSkill = new List<SkillSpec>();
        }
        public void Reload()
        {
            _lstSkill.Clear();
        }
        public SkillSpec GetSkill(SkillData skill)
        {
            return _lstSkill.Find(s => s.skillData == skill);
        }
        public List<SkillSpec> GetListSkillWithType(AbilityType type)
        {
            return _lstSkill.FindAll(s => s.skillData.abilityType == type);
        }
        public void LearnSkill(SkillData skillData)
        {
            var skillSpec = GetSkill(skillData) ?? new SkillSpec(skillData);

            if (!_lstSkill.Contains(skillSpec))
            {
                _lstSkill.Add(skillSpec);
                Debug.LogError("_lstSkill ");
            }
            //else
            //{
            //    skillSpec.LevelUpSkill();
            //}
            ApplyPassiveEffects(skillSpec);
            //ApplyHeroesSkill(skillSpec);
        }

        public void LearnSkill(string id)
        {
            SkillData skillData = database.GetSkill(id);
            if (skillData != null)
            {
                LearnSkill(skillData);
            }
        }

        private void ApplyPassiveEffects(SkillSpec skillSpec)
        {
            //if (skillSpec.skillData.abilityType != AbilityType.Passive) return;
            foreach (PassiveSkillEffect effect in skillSpec.skillData.effects)
            {
                float valueChange = effect.GetEffectValue(skillSpec.level);
                PlayerController.StatsManager.UpdateStat(effect.statType, valueChange);
            }
        }
        public void ApplyPassiveEffects(HeroCardData cardData)
        {
            SkillSpec skillSpec = new SkillSpec(cardData.SkillData, (int)cardData.RarityType + 1);
            ApplyPassiveEffects(skillSpec);
        }
        private void ApplyHeroesSkill(SkillSpec skillSpec)
        {
            if (skillSpec.skillData.abilityType != AbilityType.Hero) return;
            foreach (HeroSummonSkillEffect effect in skillSpec.skillData.effects)
            {
                if (!PlayerController.Instance.HasCharacter(effect.HeroeType))
                {
                    //PlayerController.Instance.SpawnCharacter(effect.HeroeType);
                }
                else
                {
                    PlayerController.Instance.UpgradeCharacter(effect.HeroeType);
                }
            }
        }


        public List<SkillData> GetRandomSkills(int num)
        {
            //List<SkillData> randomSkills = new List<SkillData>();

            //if (database.GetSkillCount() == 0 || num <= 0)
            //    return randomSkills;

            //List<int> heroOnlyLevels = Define.heroOnlyLevels;

            //List<SkillData> heroSkills = new List<SkillData>();
            //List<SkillData> regularSkills = new List<SkillData>();

            //int chapterIndex = TemplateSystems.DataManager.Instance.GetLevel();
            //for (int i = 0; i < database.GetSkillCount(); i++)
            //{
            //    SkillData skill = database.GetSkill(i);

            //    if (GetSkill(skill) != null)
            //    {
            //        if (GetSkill(skill).IsMaxLevel) continue;
            //    }

            //    if (chapterIndex <= skill.chapterIndexRepair) continue;

            //    if (skill.abilityType == AbilityType.Hero)
            //        heroSkills.Add(skill);
            //    else if (skill.abilityType == AbilityType.Passive)
            //        regularSkills.Add(skill);
            //}

            //int prevLevel = PlayerController.StatsManager.Level - 1;

            //if (PlayerController.Instance.IsFullHeroes)
            //{
            //    heroSkills = GetSkillHeroDataLearned();
            //}
            //if (IsFullSkill(AbilityType.Passive))
            //{
            //    regularSkills = GetSkillPassiveDataLearned();
            //}
            //bool isHeroOnlyLevel = heroOnlyLevels.Contains(prevLevel);
            //if (heroSkills.Count < 3) // NOT ENOUGH HERO UNLOCKED
            //    isHeroOnlyLevel = false;

            //if (isHeroOnlyLevel)
            //{
            //    HashSet<int> selectedIndices = new HashSet<int>();
            //    while (randomSkills.Count < num && selectedIndices.Count < heroSkills.Count)
            //    {
            //        int randomIndex = Random.Range(0, heroSkills.Count);
            //        if (!selectedIndices.Contains(randomIndex))
            //        {
            //            selectedIndices.Add(randomIndex);
            //            randomSkills.Add(heroSkills[randomIndex]);
            //        }
            //    }
            //}
            //else
            //{
            //    if (heroSkills.Count > 0)
            //    {
            //        SkillData heroSkill = heroSkills[Random.Range(0, heroSkills.Count)];
            //        randomSkills.Add(heroSkill);
            //        regularSkills.Remove(heroSkill);
            //    }

            //    HashSet<int> selectedIndices = new HashSet<int>();
            //    while (randomSkills.Count < num && selectedIndices.Count < regularSkills.Count)
            //    {
            //        int randomIndex = Random.Range(0, regularSkills.Count);
            //        if (!selectedIndices.Contains(randomIndex))
            //        {
            //            selectedIndices.Add(randomIndex);
            //            randomSkills.Add(regularSkills[randomIndex]);
            //        }
            //    }
            //}
            List<SkillData> randomSkills = new List<SkillData>();
            SkillData[] regularSkills = DataManager.Instance.SkillSpecialDatabaseConfig.GetAllSkills();
            HashSet<int> selectedIndices = new HashSet<int>();
            while (randomSkills.Count < 3)
            {
                int randomIndex = Random.Range(0, regularSkills.Length);
                if (!selectedIndices.Contains(randomIndex))
                {
                    selectedIndices.Add(randomIndex);
                    randomSkills.Add(regularSkills[randomIndex]);
                }
            }
            return randomSkills;
        }
        private bool IsFullSkill(AbilityType ability)
        {
            return GetListSkillWithType(ability).Count >= Define.LIMIT_NUMBER_SKILL;
        }
        private List<SkillData> GetSkillHeroDataLearned()
        {
            return GetListSkillWithType(AbilityType.Hero).
                Where(skillSpec => !skillSpec.IsMaxLevel)
                .Select(skillSpec => skillSpec.skillData)
                .ToList();
        }
        private List<SkillData> GetSkillPassiveDataLearned()
        {
            return GetListSkillWithType(AbilityType.Passive).
                Where(skillSpec => !skillSpec.IsMaxLevel)
                .Select(skillSpec => skillSpec.skillData)
                .ToList();
        }

        public List<SkillCardData> GetAvailableSkillCards()
        {
            var availableSkills = GetRandomSkills(3);
            List<SkillCardData> availableCards = new List<SkillCardData>();
            foreach (var skill in availableSkills)
            {
                availableCards.Add(ConvertToSkillCardData(skill));
            }
            return availableCards;
        }

        private SkillCardData ConvertToSkillCardData(SkillData skillData)
        {
            SkillSpec skillSpec = GetSkill(skillData);
            int level = skillSpec == null ? 1 : skillSpec.level + 1;
            
            ElementType elementType = ElementType.None;
            if (skillData.skillType == SkillType.SpawnHero && skillData.effects.Length > 0)
            {
                if (skillData.effects[0] is HeroSummonSkillEffect heroEffect)
                {
                    elementType = PlayerController.Instance.GetCharacter(heroEffect.HeroeType).ElementType;
                }
            }

            SkillCardData cardData = new SkillCardData
            {
                cardId = skillData.Id,
                iconId = skillData.IdIcon,
                cardName = skillData.skillName,
                description = skillData.effects[0].GetEffectValueString(level),
                skillType = skillData.skillType,
                starCount = level - 1,
                elementType = elementType
            };
            return cardData;
        }
    }
}
