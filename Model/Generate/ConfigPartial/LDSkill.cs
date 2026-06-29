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


        public override void AfterEndInit()
        {
 
            foreach (LDSkill skillconfig in this.GetAll().Values)
            {
                skillconfig.ParseRuntimeData();
            }

        }


    }
}
