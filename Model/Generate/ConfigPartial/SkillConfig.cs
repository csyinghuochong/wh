using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    public struct LDSkillAttributeLimit
    {
        public int CompareType;
        public int NumericType;
        public long Value;
    }

    public struct LDSkillConsumeItem
    {
        public int NumericType;
        public long Value;
    }

    public partial class LDSkill
    {
        [ProtoIgnore]
        [BsonIgnore]
        public List<KeyValuePairInt> ReplaceSkillList = new List<KeyValuePairInt>();

        [ProtoIgnore]
        [BsonIgnore]
        public List<LDSkillAttributeLimit> SelfAttributeLimits = new List<LDSkillAttributeLimit>();

        [ProtoIgnore]
        [BsonIgnore]
        public List<LDSkillConsumeItem> ConsumeList = new List<LDSkillConsumeItem>();

        public void ParseRuntimeData()
        {
            this.ReplaceSkillList.Clear();
            this.SelfAttributeLimits.Clear();
            this.ConsumeList.Clear();

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

            if (!string.IsNullOrEmpty(this.Consume) && this.Consume != "0")
            {
                string[] consumeItems = this.Consume.Split('|');
                for (int i = 0; i < consumeItems.Length; i++)
                {
                    string[] parts = consumeItems[i].Split('_');
                    if (parts.Length != 2)
                    {
                        continue;
                    }
                    if (!int.TryParse(parts[0], out int numericType) || !long.TryParse(parts[1], out long value))
                    {
                        continue;
                    }
                    this.ConsumeList.Add(new LDSkillConsumeItem
                    {
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
        /// 1003 1002 1001..的基础技能都是1001   
        /// </summary>
        public Dictionary<int, int> InitWeaponSkillList = new Dictionary<int, int>();

        public Dictionary<int, List<KeyValuePairInt>> EquipSkillList = new Dictionary<int, List<KeyValuePairInt>>();

        /// <summary>
        /// 给该buff的玩家触发一个技能
        /// </summary>
        public Dictionary<int , KeyValuePairLong4> BuffTriggerSkill = new Dictionary<int , KeyValuePairLong4>();

        /// <summary>
        /// 给该buff的玩家触发额外伤害
        /// </summary>
        public Dictionary<int, KeyValuePairLong4> BuffAddHurt = new Dictionary<int, KeyValuePairLong4>();

        /// <summary>
        /// 给该buff的玩家触发二段技能
        /// </summary>
        public Dictionary<int, KeyValuePairLong4> BuffSecondSkill = new Dictionary<int, KeyValuePairLong4>();


        //技能额外属性来自自身
        public Dictionary<int, List<PropertyValue>> ExtraPropertyFromSelf = new Dictionary<int, List<PropertyValue>>();


        public Dictionary<int,List<int>> SkillSpecifiedMonster = new Dictionary<int, List<int>>();  

        /// <summary>
        /// 获取是技能的一级基础技能
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
           
            foreach (LDSkill skillconfig in this.GetAll().Values)
            {
                skillconfig.ParseRuntimeData();
            }

            this.InitWeaponSkillList.Clear();
            this.BuildInitWeaponSkillList();
        }

        /// <summary>
        /// 1003、1002、1001 的基础技能都是 1001。沿 NextId 链从链头向下填充。
        /// </summary>
        private void BuildInitWeaponSkillList()
        {
            Dictionary<int, LDSkill> allSkills = this.GetAll();
            HashSet<int> hasPrevious = new HashSet<int>();
            foreach (LDSkill skill in allSkills.Values)
            {
                if (skill.NextId != 0)
                {
                    hasPrevious.Add(skill.NextId);
                }
            }

            foreach (LDSkill skill in allSkills.Values)
            {
                if (hasPrevious.Contains(skill.Id))
                {
                    continue;
                }

                int baseSkillId = skill.Id;
                int currentId = skill.Id;
                HashSet<int> visited = new HashSet<int>();
                while (currentId != 0 && allSkills.ContainsKey(currentId) && visited.Add(currentId))
                {
                    this.InitWeaponSkillList[currentId] = baseSkillId;
                    currentId = allSkills[currentId].NextId;
                }
            }

            foreach (LDSkill skill in allSkills.Values)
            {
                if (!this.InitWeaponSkillList.ContainsKey(skill.Id))
                {
                    this.InitWeaponSkillList[skill.Id] = skill.Id;
                }
            }
        }


        public int GetNewSkill(List<SkillPro> skillPros, int oldskiull)
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
