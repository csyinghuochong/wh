using System;
using System.Collections.Generic;

namespace ET
{
    public partial class TalentConfigCategory
    {

        /// <summary>
        /// 技能转天赋
        /// </summary>
        public Dictionary<int, List<int>> SkillToTalentList = new Dictionary<int, List<int>>();


        /// <summary>
        /// 天赋转技能
        /// </summary>
        public Dictionary<int, List<int>> TalentToSkillList = new Dictionary<int, List<int>>();     
        

        public List<int> GetSkillToTalentId(int skillid)
        {
            if (SkillToTalentList.ContainsKey(skillid))
            {
                return SkillToTalentList[skillid];
            }
            return null;
        }

        public bool HaveTalentSkillId(int tianfuid, int skillid)
        {
            if (TalentToSkillList.ContainsKey(tianfuid))
            {
                return TalentToSkillList[tianfuid].Contains(skillid);   
            }

            return false;
        }
    }
}
