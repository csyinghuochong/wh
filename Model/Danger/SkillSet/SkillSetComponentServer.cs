using System.Collections.Generic;
#if SERVER
using MongoDB.Bson.Serialization.Attributes;
#endif


namespace ET
{
    public class KeyValuePairLong4
    {

        public long KeyId;

        public long Value;

        public long Value2;
        public long Value3;
    }



    public class SkillSetComponentServer : Entity, IAwake, ITransfer, IUnitCache
    {
        public int TianFuPlan = 0;

        public List<int> TianFuList = new List<int>();          //第一套天賦方案

        public List<int> TianFuList1 = new List<int>();         //第二套天賦方案

        public List<int> TianFuAddition = new List<int>();         //附加天赋

        public List<SkillPro> SkillList = new List<SkillPro>();

        public List<SkillPro> SkillListRemove = new List<SkillPro>();   //备份移除的技能

        /// <summary>当前技能方案 0/1</summary>
        public int SkillBarPlan = 0;

        /// <summary>技能方案0</summary>
        public List<SkillBarSlot> SkillBarList = new List<SkillBarSlot>();

        /// <summary>技能方案1</summary>
        public List<SkillBarSlot> SkillBarList1 = new List<SkillBarSlot>();

        //生命之盾
        public List<LifeShieldInfo> LifeShieldList = new List<LifeShieldInfo>();

#if SERVER
        [BsonIgnore]
        public M2C_SkillSetMessage M2C_SkillSetMessage = new M2C_SkillSetMessage() { SkillSetInfo = new SkillSetInfo() };
#endif
    }
}
