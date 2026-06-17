using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDPay_TierCategory : ProtoObject, IMerge
    {
        public static LDPay_TierCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDPay_Tier> dict = new Dictionary<int, LDPay_Tier>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDPay_Tier> list = new List<LDPay_Tier>();
		
        public LDPay_TierCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDPay_TierCategory s = o as LDPay_TierCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDPay_Tier config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDPay_Tier Get(int id)
        {
            this.dict.TryGetValue(id, out LDPay_Tier item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDPay_Tier)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDPay_Tier> GetAll()
        {
            return this.dict;
        }

        public LDPay_Tier GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDPay_Tier: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>SKU</summary>
		[ProtoMember(2)]
		public string Tier_Sku { get; set; }
		/// <summary>人民币</summary>
		[ProtoMember(3)]
		public int Tier_CNY { get; set; }
		/// <summary>美元</summary>
		[ProtoMember(4)]
		public int Tier_USD { get; set; }

	}
}
