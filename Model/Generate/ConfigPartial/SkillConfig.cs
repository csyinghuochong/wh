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
        /// 69060301 69060302 ..的基础技能都是69060300
        /// </summary>
        public Dictionary<int, int> BaseSkillList = new Dictionary<int, int>();

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
        public int GetInitSkill(int skillid)
        {
            int baseskillid = 0;
            BaseSkillList.TryGetValue( skillid, out baseskillid);
            return baseskillid;
        }

        public override void AfterEndInit()
        {
            BaseSkillList.Clear();
            foreach (LDSkill skillconfig in this.GetAll().Values)
            {
                skillconfig.ParseRuntimeData();
            }

            foreach (LDSkill skillconfig in this.GetAll().Values)
            {
                /*string equipskill = skillconfig.EquipSkill;
                if (string.IsNullOrEmpty(equipskill) || equipskill.Equals("0"))
                {
                    continue;
                }

                List<KeyValuePairInt> equipSkillds = null;
                EquipSkillList.TryGetValue(skillconfig.Id, out equipSkillds);
                if (equipSkillds == null)
                {
                    equipSkillds = new List<KeyValuePairInt>();
                    EquipSkillList.Add(skillconfig.Id, equipSkillds);
                }

                //61023101,61023102;61023102,61023103;61023103,61023104;61023104,61023105;61023105,61023106
                string[] skillkeys = equipskill.Split(';');
                if (skillkeys == null)
                {
                    Log.Error($"skillconfig.EquipSkill.error1: equipskillid: {skillconfig.Id}  :{equipskill}");
                    continue;
                }

                foreach (string key in skillkeys)
                {
                    string[] skillitem = key.Split(',');
                    if (skillitem.Length != 2)
                    {
                        Log.Error($"skillconfig.EquipSkill.error2: equipskillid: {skillconfig.Id} {equipskill}");
                        continue;
                    }

                    if (!int.TryParse(skillitem[0], out int oldSkillId))
                    {
                        Log.Error($"int.TryParse error: {skillitem[0]} skillId:{skillconfig.Id} equipskill:{equipskill}");
                        continue;
                    }

                    if (!int.TryParse(skillitem[1], out int newSkillId))
                    {
                        Log.Error($"int.TryParse error: {skillitem[1]} skillId:{skillconfig.Id} equipskill:{equipskill}");
                        continue;
                    }

                    KeyValuePairInt keyValuePairInt = new KeyValuePairInt();
                    keyValuePairInt.KeyId = oldSkillId;
                    keyValuePairInt.Value = newSkillId;
                    equipSkillds.Add(keyValuePairInt);
                }*/
            }

            foreach (LDSkill skillconfig in this.GetAll().Values)
            {
                
            }

            // 得到所有技能的基础技能
            foreach (LDSkill skillConfig in this.GetAll().Values)
            {
                SetBaseSkill(skillConfig, 0);
            }

            void SetBaseSkill(LDSkill skillConfig, int baseId)
            {
                if (!this.BaseSkillList.ContainsKey(skillConfig.Id))
                {
                    if (baseId != 0)
                    {
                        this.BaseSkillList.Add(skillConfig.Id, baseId);
                        int nextId = skillConfig.NextId;
                        if (nextId != 0)
                        {
                            SetBaseSkill(this.GetAll()[nextId], baseId);
                        }
                    }
                    else
                    {
                        this.BaseSkillList.Add(skillConfig.Id, skillConfig.Id);
                        int nextId = skillConfig.NextId;
                        if (nextId != 0)
                        {
                            SetBaseSkill(this.GetAll()[nextId], skillConfig.Id);
                        }
                    }
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
