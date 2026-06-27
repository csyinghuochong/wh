using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{
    public struct LDSkillAttributeLimit
    {
        public int CompareType;
        public int NumericType;
        public long Value;
    }


    public partial class LDSkill
    {
        public List<KeyValuePairInt> ReplaceSkillList = new List<KeyValuePairInt>();
        public List<LDSkillAttributeLimit> SelfAttributeLimits = new List<LDSkillAttributeLimit>();

        public void ParseRuntimeData()
        {
            this.ReplaceSkillList.Clear();
            this.SelfAttributeLimits.Clear();

            if (!string.IsNullOrEmpty(this.Replace_Skill) && this.Replace_Skill != "0")
            {
                string[] replaceItems = this.Replace_Skill.Split('|');
                for (int i = 0; i < replaceItems.Length; i++)
                {
                    string[] parts = replaceItems[i].Split('_');
                    if (parts.Length != 2)
                    {
                        continue;
                    }
                    if (!int.TryParse(parts[0], out int buffId) || !int.TryParse(parts[1], out int newSkillId))
                    {
                        continue;
                    }
                    KeyValuePairInt pair = new KeyValuePairInt();
                    pair.KeyId = buffId;
                    pair.Value = newSkillId;
                    this.ReplaceSkillList.Add(pair);
                }
            }

            if (!string.IsNullOrEmpty(this.Self_Attribute_Limit) && this.Self_Attribute_Limit != "0")
            {
                string[] limitItems = this.Self_Attribute_Limit.Split('|');
                for (int i = 0; i < limitItems.Length; i++)
                {
                    string[] parts = limitItems[i].Split('_');
                    if (parts.Length != 3)
                    {
                        continue;
                    }
                    if (!int.TryParse(parts[0], out int compareType)
                        || !int.TryParse(parts[1], out int numericType)
                        || !long.TryParse(parts[2], out long value))
                    {
                        continue;
                    }
                    this.SelfAttributeLimits.Add(new LDSkillAttributeLimit
                    {
                        CompareType = compareType,
                        NumericType = numericType,
                        Value = value,
                    });
                }
            }
        }
    }

    public partial class LDSkillCategory
    {
        
        /// <summary>
        /// 69060301 69060302 ..的基础技能都是69060300
        /// </summary>
        public Dictionary<int, int> InitWeaponSkillList = new Dictionary<int, int>();

        
        public Dictionary<int, List<KeyValuePairInt>> EquipSkillList = new Dictionary<int, List<KeyValuePairInt>>();

        /// <summary>
        /// 给该buff的玩家触发一个技能
        /// </summary>
        public Dictionary<int, KeyValuePairLong4> BuffTriggerSkill = new Dictionary<int, KeyValuePairLong4>();

        /// <summary>
        /// 给该buff的玩家触发额外伤害
        /// </summary>
        public Dictionary<int, KeyValuePairLong4> BuffAddHurt = new Dictionary<int, KeyValuePairLong4>();

        /// <summary>
        /// 给该buff的玩家触发二段技能
        /// </summary>
        public Dictionary<int, KeyValuePairLong4> BuffSecondSkill = new Dictionary<int, KeyValuePairLong4>();

        /// <summary>
        /// 获取技能链的一级/初始技能 Id
        /// </summary>
        /// <param name="skillid"></param>
        /// <returns></returns>
        public int GetInitWeaponSkill(int skillid)
        {
            if (this.InitWeaponSkillList.TryGetValue(skillid, out int baseskillid))
            {
                return baseskillid;
            }
            return skillid;
        }

        public override void AfterEndInit()
        {
            this.InitWeaponSkillList.Clear();
            foreach (LDSkill skillconfig in this.GetAll().Values)
            {
                skillconfig.ParseRuntimeData();
            }

            this.BuildInitWeaponSkillList();
        }

        /// <summary>
        /// 1003、1002、1001 的基础技能都是 1001。沿 NextId 链从链头向下填充。
        /// </summary>
        private void BuildInitWeaponSkillList()
        {
         
        }

        public int GetNewSkill(List<SkillPro> skillPros,  int oldskiull)
        {
            if (skillPros == null)
            {
                return oldskiull;
            }
            for (int i = 0; i < skillPros.Count; i++)
            {
                List<KeyValuePairInt> equipSkillds = null;
                this.EquipSkillList.TryGetValue(skillPros[i].SkillID, out equipSkillds);
                if (equipSkillds == null)
                {
                    continue;
                }

                for (int skillindex = 0; skillindex < equipSkillds.Count; skillindex++)
                {
                    if (equipSkillds[skillindex].KeyId == oldskiull)
                    {
                        return (int)equipSkillds[skillindex].Value;
                    }
                }
            }
            return oldskiull;
        }

        public int GetOldSkill(int baseskill, int newskiull)
        {
            List<KeyValuePairInt> equipSkillds = null;
            EquipSkillList.TryGetValue(baseskill, out equipSkillds);
            if (equipSkillds == null)
            {
                return 0;
            }

            for (int i = 0; i < equipSkillds.Count; i++)
            {
                if (equipSkillds[i].Value == newskiull)
                {
                    return equipSkillds[i].KeyId;
                }
            }
            return 0;
        }
    }
}
