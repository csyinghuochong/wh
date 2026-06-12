using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDRechargeCategory : ProtoObject, IMerge
    {
        public static LDRechargeCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDRecharge> dict = new Dictionary<int, LDRecharge>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDRecharge> list = new List<LDRecharge>();
		
        public LDRechargeCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDRechargeCategory s = o as LDRechargeCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDRecharge config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDRecharge Get(int id)
        {
            this.dict.TryGetValue(id, out LDRecharge item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDRecharge)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDRecharge> GetAll()
        {
            return this.dict;
        }

        public LDRecharge GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDRecharge: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>SKU</summary>
		[ProtoMember(2)]
		public string Recharge_Sku { get; set; }
		/// <summary>人民币</summary>
		[ProtoMember(3)]
		public int Recharge_CNY { get; set; }
		/// <summary>美元</summary>
		[ProtoMember(4)]
		public int Recharge_USD { get; set; }

	}
}
