using System.Collections.Generic;

namespace ET
{
    public partial class LDSkill_LvCategory
    {
        public Dictionary<int, List<LDSkill_Lv>> ConsumeList = new Dictionary<int, List<LDSkill_Lv>>();

        public override void AfterEndInit()
        {
            this.ConsumeList.Clear();
            foreach (LDSkill_Lv skillconfig in this.GetAll().Values)
            {
                if (!this.ConsumeList.TryGetValue(skillconfig.Skill_Id, out List<LDSkill_Lv> skillLvList))
                {
                    skillLvList = new List<LDSkill_Lv>();
                    this.ConsumeList.Add(skillconfig.Skill_Id, skillLvList);
                }

                skillLvList.Add(skillconfig);
            }

            foreach (List<LDSkill_Lv> skillLvList in this.ConsumeList.Values)
            {
                skillLvList.Sort((a, b) => a.Skill_Lv.CompareTo(b.Skill_Lv));
            }
        }
        
        public LDSkill_Lv GetLDSkill_Lv(int skill, int lv)
        {
            return ConsumeList[skill][lv - 1];
        }
    }
}