using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDSkill_LogicCategory : ProtoObject, IMerge
    {
        public static LDSkill_LogicCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDSkill_Logic> dict = new Dictionary<int, LDSkill_Logic>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDSkill_Logic> list = new List<LDSkill_Logic>();
		
        public LDSkill_LogicCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDSkill_LogicCategory s = o as LDSkill_LogicCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDSkill_Logic config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDSkill_Logic Get(int id)
        {
            this.dict.TryGetValue(id, out LDSkill_Logic item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDSkill_Logic)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDSkill_Logic> GetAll()
        {
            return this.dict;
        }

        public LDSkill_Logic GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDSkill_Logic: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>技能</summary>
		[ProtoMember(2)]
		public int Skill_Id { get; set; }
		/// <summary>序列</summary>
		[ProtoMember(3)]
		public int Sequence { get; set; }
		/// <summary>几率</summary>
		[ProtoMember(4)]
		public int Probability { get; set; }
		/// <summary>效果</summary>
		[ProtoMember(5)]
		public int Logic { get; set; }
		/// <summary>参数1</summary>
		[ProtoMember(6)]
		public int Logic_Param1 { get; set; }
		/// <summary>参数2</summary>
		[ProtoMember(7)]
		public int Logic_Param2 { get; set; }
		/// <summary>参数3</summary>
		[ProtoMember(8)]
		public int Logic_Param3 { get; set; }

	}
}
