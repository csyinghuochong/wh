using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDMakeCategory : ProtoObject, IMerge
    {
        public static LDMakeCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDMake> dict = new Dictionary<int, LDMake>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDMake> list = new List<LDMake>();
		
        public LDMakeCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDMakeCategory s = o as LDMakeCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDMake config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDMake)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDMake Get(int id)
        {
            this.dict.TryGetValue(id, out LDMake item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDMake)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDMake> GetAll()
        {
            return this.dict;
        }

        public LDMake GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDMake: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>条件1</summary>
		[ProtoMember(2)]
		public int Condition_Type1 { get; set; }
		/// <summary>值1</summary>
		[ProtoMember(3)]
		public int Condition_Value1 { get; set; }
		/// <summary>条件2</summary>
		[ProtoMember(4)]
		public int Condition_Type2 { get; set; }
		/// <summary>值2</summary>
		[ProtoMember(5)]
		public int Condition_Value2 { get; set; }
		/// <summary>时间</summary>
		[ProtoMember(6)]
		public int Time { get; set; }
		/// <summary>特殊消耗</summary>
		[ProtoMember(7)]
		public int Special_Consume { get; set; }
		/// <summary>消耗</summary>
		[ProtoMember(8)]
		public string Consume { get; set; }
		/// <summary>奖励</summary>
		[ProtoMember(9)]
		public string Reward { get; set; }

	}
}
