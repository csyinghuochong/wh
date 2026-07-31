using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDFund_RewardCategory : ProtoObject, IMerge
    {
        public static LDFund_RewardCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDFund_Reward> dict = new Dictionary<int, LDFund_Reward>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDFund_Reward> list = new List<LDFund_Reward>();
		
        public LDFund_RewardCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDFund_RewardCategory s = o as LDFund_RewardCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDFund_Reward config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDFund_Reward)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDFund_Reward Get(int id)
        {
            this.dict.TryGetValue(id, out LDFund_Reward item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDFund_Reward)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDFund_Reward> GetAll()
        {
            return this.dict;
        }

        public LDFund_Reward GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDFund_Reward: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>值</summary>
		[ProtoMember(2)]
		public int value { get; set; }
		/// <summary>免费奖励</summary>
		[ProtoMember(3)]
		public string Reward_Free { get; set; }
		/// <summary>付费奖励1</summary>
		[ProtoMember(4)]
		public string Reward_Pay_1 { get; set; }
		/// <summary>付费奖励2</summary>
		[ProtoMember(5)]
		public string Reward_Pay_2 { get; set; }
		/// <summary>付费奖励3</summary>
		[ProtoMember(6)]
		public string Reward_Pay_3 { get; set; }
		/// <summary>特殊节点</summary>
		[ProtoMember(7)]
		public int Is_Special { get; set; }

	}
}
