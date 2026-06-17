using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDShop_GoodsCategory : ProtoObject, IMerge
    {
        public static LDShop_GoodsCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDShop_Goods> dict = new Dictionary<int, LDShop_Goods>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDShop_Goods> list = new List<LDShop_Goods>();
		
        public LDShop_GoodsCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDShop_GoodsCategory s = o as LDShop_GoodsCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDShop_Goods config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDShop_Goods Get(int id)
        {
            this.dict.TryGetValue(id, out LDShop_Goods item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDShop_Goods)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDShop_Goods> GetAll()
        {
            return this.dict;
        }

        public LDShop_Goods GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDShop_Goods: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>ShopId</summary>
		[ProtoMember(2)]
		public int ShopId { get; set; }
		/// <summary>组</summary>
		[ProtoMember(3)]
		public int Group { get; set; }
		/// <summary>编号</summary>
		[ProtoMember(4)]
		public int Number { get; set; }
		/// <summary>角色等级</summary>
		[ProtoMember(5)]
		public int Lv_Min { get; set; }
		/// <summary>角色等级</summary>
		[ProtoMember(6)]
		public int Lv_Max { get; set; }
		/// <summary>权重</summary>
		[ProtoMember(7)]
		public int weight { get; set; }
		/// <summary>限购条件</summary>
		[ProtoMember(8)]
		public string Buy_Limit_Condition { get; set; }
		/// <summary>限购</summary>
		[ProtoMember(9)]
		public int Buy_Limit_Num { get; set; }
		/// <summary>消耗 类型</summary>
		[ProtoMember(10)]
		public int Consume_Type { get; set; }
		/// <summary>消耗 ID</summary>
		[ProtoMember(11)]
		public int Consume_Id { get; set; }
		/// <summary>现价</summary>
		[ProtoMember(12)]
		public int Consume_Value { get; set; }
		/// <summary>原价</summary>
		[ProtoMember(13)]
		public int Consume_Original { get; set; }
		/// <summary>折扣</summary>
		[ProtoMember(14)]
		public double discount { get; set; }
		/// <summary>商品</summary>
		[ProtoMember(15)]
		public string Goods { get; set; }
		/// <summary>排序</summary>
		[ProtoMember(16)]
		public int Order_LS { get; set; }
		/// <summary>禁用</summary>
		[ProtoMember(17)]
		public int Is_Close { get; set; }

	}
}
