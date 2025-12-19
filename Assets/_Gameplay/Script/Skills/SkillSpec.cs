using System;

namespace CLHoma
{
    [Serializable]
    public class SkillSpec
    {
        public int level;
        public SkillData skillData;

        public bool IsMaxLevel => level == 5;
        public SkillSpec(SkillData skillData, int lv = 1)
        {
            level = lv;
            this.skillData = skillData;
        }
        public void LevelUpSkill()
        {
            level++;
        }
    }
}
