using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDActivity_FundCategory : ProtoObject, IMerge
    {
        public static LDActivity_FundCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDActivity_Fund> dict = new Dictionary<int, LDActivity_Fund>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDActivity_Fund> list = new List<LDActivity_Fund>();
		
        public LDActivity_FundCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDActivity_FundCategory s = o as LDActivity_FundCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDActivity_Fund config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDActivity_Fund Get(int id)
        {
            this.dict.TryGetValue(id, out LDActivity_Fund item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDActivity_Fund)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDActivity_Fund> GetAll()
        {
            return this.dict;
        }

        public LDActivity_Fund GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDActivity_Fund: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>类型</summary>
		[ProtoMember(2)]
		public int Type { get; set; }
		/// <summary>支付1</summary>
		[ProtoMember(3)]
		public int Pay_1 { get; set; }
		/// <summary>价值1</summary>
		[ProtoMember(4)]
		public int Value_1 { get; set; }
		/// <summary>支付2</summary>
		[ProtoMember(5)]
		public int Pay_2 { get; set; }
		/// <summary>价值2</summary>
		[ProtoMember(6)]
		public int Value_2 { get; set; }
		/// <summary>支付3</summary>
		[ProtoMember(7)]
		public int Pay_3 { get; set; }
		/// <summary>价值3</summary>
		[ProtoMember(8)]
		public int Value_3 { get; set; }

	}
}
