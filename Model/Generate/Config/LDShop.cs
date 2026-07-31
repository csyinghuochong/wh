using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDShopCategory : ProtoObject, IMerge
    {
        public static LDShopCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDShop> dict = new Dictionary<int, LDShop>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDShop> list = new List<LDShop>();
		
        public LDShopCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDShopCategory s = o as LDShopCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDShop config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDShop)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDShop Get(int id)
        {
            this.dict.TryGetValue(id, out LDShop item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDShop)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDShop> GetAll()
        {
            return this.dict;
        }

        public LDShop GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDShop: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>类型 1-固定 2-随机可重复 3-随机不重复 9-全服</summary>
		[ProtoMember(2)]
		public int Type { get; set; }
		/// <summary>商品数量 -1为全部</summary>
		[ProtoMember(3)]
		public int Goods_Num { get; set; }
		/// <summary>手动刷新 0-否 1-是</summary>
		[ProtoMember(4)]
		public int Is_Refresh { get; set; }
		/// <summary>手动刷新 次数限制</summary>
		[ProtoMember(5)]
		public int Refresh_Times { get; set; }
		/// <summary>刷新道具</summary>
		[ProtoMember(6)]
		public int Refresh_Item { get; set; }
		/// <summary>钻石刷新</summary>
		[ProtoMember(7)]
		public int[] Refresh_Diamond { get; set; }
		/// <summary>自动刷新 1-日 2-周 3-月 9-特殊</summary>
		[ProtoMember(8)]
		public int Auto_Refresh { get; set; }
		/// <summary>资源条 其他</summary>
		[ProtoMember(9)]
		public int[] Resource_Bar { get; set; }
		/// <summary>资源条 金币 0-无 1-所有 2-非绑</summary>
		[ProtoMember(10)]
		public int Resource_Bar_2 { get; set; }
		/// <summary>资源条 钻石 0-无 1-所有 2-非绑</summary>
		[ProtoMember(11)]
		public int Resource_Bar_1 { get; set; }
		/// <summary>对应 隶属</summary>
		[ProtoMember(12)]
		public int Belong_Id { get; set; }

	}
}
