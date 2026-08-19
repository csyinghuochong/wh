using System.Collections.Generic;

namespace ET
{
    public partial class LDSkill_Battle_LvCategory
    {
        public Dictionary<int, List<LDSkill_Battle_Lv>> ConsumeList = new Dictionary<int, List<LDSkill_Battle_Lv>>();

        public override void AfterEndInit()
        {
            this.ConsumeList.Clear();
            foreach (LDSkill_Battle_Lv skillconfig in this.GetAll().Values)
            {
                if (!this.ConsumeList.TryGetValue(skillconfig.Skill_Battle_Id, out List<LDSkill_Battle_Lv> skillLvList))
                {
                    skillLvList = new List<LDSkill_Battle_Lv>();
                    this.ConsumeList.Add(skillconfig.Skill_Battle_Id, skillLvList);
                }

                skillLvList.Add(skillconfig);
            }

            foreach (List<LDSkill_Battle_Lv> skillLvList in this.ConsumeList.Values)
            {
                skillLvList.Sort((a, b) => a.Skill_Battle_Lv.CompareTo(b.Skill_Battle_Lv));
            }
        }

        public int GetSkillMaxLv(int skill)
        {
            if (skill <= 0)
            {
                return 0;
            }
            if (!ConsumeList.TryGetValue(skill, out List<LDSkill_Battle_Lv> skillLvList))
            {
                return 1;
            }
            return skillLvList.Count;
        }

        public LDSkill_Battle_Lv GetLDSkillLv(int skill, int lv)
        {
            if (lv <= 0)
            {
                return null;
            }
            if (!ConsumeList.TryGetValue(skill, out List<LDSkill_Battle_Lv> skillLvList))
            {
                return null;
            }
            if (skillLvList.Count < lv)
            {
                return null;
            }
            return skillLvList[lv - 1];
        }
    }
}