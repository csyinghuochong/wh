using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDPayCategory : ProtoObject, IMerge
    {
        public static LDPayCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDPay> dict = new Dictionary<int, LDPay>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDPay> list = new List<LDPay>();
		
        public LDPayCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDPayCategory s = o as LDPayCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDPay config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDPay)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDPay Get(int id)
        {
            this.dict.TryGetValue(id, out LDPay item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDPay)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDPay> GetAll()
        {
            return this.dict;
        }

        public LDPay GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDPay: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(2)]
		public int Name { get; set; }
		/// <summary>描述</summary>
		[ProtoMember(3)]
		public int Desc { get; set; }
		/// <summary>限购</summary>
		[ProtoMember(4)]
		public int Buy_Limit { get; set; }
		/// <summary>价格</summary>
		[ProtoMember(5)]
		public int Tier_Id { get; set; }
		/// <summary>原价</summary>
		[ProtoMember(6)]
		public int Tier_Original_Id { get; set; }
		/// <summary>奖励</summary>
		[ProtoMember(7)]
		public string Reward { get; set; }

	}
}
