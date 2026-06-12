using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDTask_ConditionCategory : ProtoObject, IMerge
    {
        public static LDTask_ConditionCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDTask_Condition> dict = new Dictionary<int, LDTask_Condition>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDTask_Condition> list = new List<LDTask_Condition>();
		
        public LDTask_ConditionCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDTask_ConditionCategory s = o as LDTask_ConditionCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDTask_Condition config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDTask_Condition Get(int id)
        {
            this.dict.TryGetValue(id, out LDTask_Condition item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDTask_Condition)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDTask_Condition> GetAll()
        {
            return this.dict;
        }

        public LDTask_Condition GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDTask_Condition: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>描述</summary>
		[ProtoMember(2)]
		public int Desc { get; set; }
		/// <summary>模式 0-累计 1-覆盖</summary>
		[ProtoMember(3)]
		public int Type { get; set; }
		/// <summary>持续计数 0-否 1-是</summary>
		[ProtoMember(4)]
		public int Count_Sustain { get; set; }
		/// <summary>消耗 待扩充</summary>
		[ProtoMember(5)]
		public string Submit { get; set; }
		/// <summary>数字处理 0-否 1-是</summary>
		[ProtoMember(6)]
		public int Digit_Deal { get; set; }
		/// <summary>备注</summary>
		[ProtoMember(7)]
		public string Note1 { get; set; }
		/// <summary>备注</summary>
		[ProtoMember(8)]
		public string Note2 { get; set; }

	}
}
