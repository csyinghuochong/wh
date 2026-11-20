using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDFundCategory : ProtoObject, IMerge
    {
        public static LDFundCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDFund> dict = new Dictionary<int, LDFund>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDFund> list = new List<LDFund>();
		
        public LDFundCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDFundCategory s = o as LDFundCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDFund config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDFund)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDFund Get(int id)
        {
            this.dict.TryGetValue(id, out LDFund item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDFund)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDFund> GetAll()
        {
            return this.dict;
        }

        public LDFund GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDFund: ProtoObject, IConfig
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
