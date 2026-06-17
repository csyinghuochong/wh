using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDActivity_Fund_RewardCategory : ProtoObject, IMerge
    {
        public static LDActivity_Fund_RewardCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDActivity_Fund_Reward> dict = new Dictionary<int, LDActivity_Fund_Reward>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDActivity_Fund_Reward> list = new List<LDActivity_Fund_Reward>();
		
        public LDActivity_Fund_RewardCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDActivity_Fund_RewardCategory s = o as LDActivity_Fund_RewardCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDActivity_Fund_Reward config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDActivity_Fund_Reward Get(int id)
        {
            this.dict.TryGetValue(id, out LDActivity_Fund_Reward item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDActivity_Fund_Reward)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDActivity_Fund_Reward> GetAll()
        {
            return this.dict;
        }

        public LDActivity_Fund_Reward GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDActivity_Fund_Reward: ProtoObject, IConfig
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
