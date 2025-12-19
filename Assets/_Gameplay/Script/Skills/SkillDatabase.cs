using UnityEngine;
using System.Collections.Generic;
using static LTGUI;

namespace CLHoma
{
    [CreateAssetMenu(menuName = "Skills/Database")]
    public class SkillDatabase : ScriptableObject
    {
        [SerializeField] private SkillData[] skilldatabase;

        public SkillData GetSkill(int index)
        {
            if (index < 0 || index >= skilldatabase.Length)
            {
                Debug.LogError($"Skill index {index} is out of range!");
                return null;
            }
            return skilldatabase[index];
        }
        public SkillData GetSkill(string id)
        {
            return System.Array.Find(skilldatabase, s => s.Id == id);
        }
        public int GetSkillCount()
        {
            return skilldatabase.Length;
        }

        public string GetHeroesUnlockedAtChapter(int chapterIndex)
        {
            //foreach (SkillData skill in skilldatabase)
            //{
            //    if (skill.abilityType == AbilityType.Hero && skill.chapterIndexRepair == chapterIndex)
            //    {
            //        if (skill.effects[0] is HeroSummonSkillEffect heroEffect)
            //        {
            //            return PlayerController.Instance.GetCharacter(heroEffect.HeroeType).Id;
            //        }
            //    }
            //}
            return string.Empty;
        }

        public List<string> GetUnlockedHeroIds(int chapterIndex)
        {
            List<string> unlockedHeroIds = new List<string>();
            foreach (SkillData skill in skilldatabase)
            {
                if (skill.abilityType == AbilityType.Hero && skill.chapterIndexRepair < chapterIndex)
                {
                    if (skill.effects[0] is HeroSummonSkillEffect heroEffect)
                    {
                        unlockedHeroIds.Add(PlayerController.Instance.GetCharacter(heroEffect.HeroeType).Id);
                    }
                }
            }
            return unlockedHeroIds;
        }

        internal SkillData[] GetAllSkills()
        {
            return skilldatabase;
        }

#if UNITY_EDITOR
        private void Reset()
        {
        }
#endif
    }
}
