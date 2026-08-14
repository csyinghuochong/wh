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
        
        public LDSkill_Battle_Lv GetLDSkill_Lv(int skill, int lv)
        {
            return ConsumeList[skill][lv - 1];
        }
    }
}