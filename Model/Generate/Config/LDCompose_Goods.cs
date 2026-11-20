using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDCompose_GoodsCategory : ProtoObject, IMerge
    {
        public static LDCompose_GoodsCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDCompose_Goods> dict = new Dictionary<int, LDCompose_Goods>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDCompose_Goods> list = new List<LDCompose_Goods>();
		
        public LDCompose_GoodsCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDCompose_GoodsCategory s = o as LDCompose_GoodsCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDCompose_Goods config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDCompose_Goods)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDCompose_Goods Get(int id)
        {
            this.dict.TryGetValue(id, out LDCompose_Goods item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDCompose_Goods)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDCompose_Goods> GetAll()
        {
            return this.dict;
        }

        public LDCompose_Goods GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDCompose_Goods: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>Compose Id</summary>
		[ProtoMember(2)]
		public int Compose_Id { get; set; }
		/// <summary>排序</summary>
		[ProtoMember(3)]
		public int Order_LS { get; set; }
		/// <summary>商品</summary>
		[ProtoMember(4)]
		public string Goods { get; set; }
		/// <summary>消耗 灵玉</summary>
		[ProtoMember(5)]
		public int Consume1 { get; set; }
		/// <summary>消耗 绑玉</summary>
		[ProtoMember(6)]
		public int Consume2 { get; set; }
		/// <summary>消耗 金币</summary>
		[ProtoMember(7)]
		public int Consume4 { get; set; }
		/// <summary>消耗 绑金</summary>
		[ProtoMember(8)]
		public int Consume5 { get; set; }
		/// <summary>特殊 消耗</summary>
		[ProtoMember(9)]
		public string Consume_Special { get; set; }
		/// <summary>主 消耗 类型</summary>
		[ProtoMember(10)]
		public int Consume_Type_1 { get; set; }
		/// <summary>主 消耗 ID</summary>
		[ProtoMember(11)]
		public int Consume_Id_1 { get; set; }
		/// <summary>主 消耗 数量</summary>
		[ProtoMember(12)]
		public int Consume_Num_1 { get; set; }
		/// <summary>副 消耗 类型</summary>
		[ProtoMember(13)]
		public int Consume_Type_2 { get; set; }
		/// <summary>副 消耗 ID</summary>
		[ProtoMember(14)]
		public int Consume_Id_2 { get; set; }
		/// <summary>副 消耗 数量</summary>
		[ProtoMember(15)]
		public int Consume_Num_2 { get; set; }
		/// <summary>副 消耗 类型</summary>
		[ProtoMember(16)]
		public int Consume_Type_3 { get; set; }
		/// <summary>副 消耗 ID</summary>
		[ProtoMember(17)]
		public int Consume_Id_3 { get; set; }
		/// <summary>副 消耗 数量</summary>
		[ProtoMember(18)]
		public int Consume_Num_3 { get; set; }
		/// <summary>副 消耗 类型</summary>
		[ProtoMember(19)]
		public int Consume_Type_4 { get; set; }
		/// <summary>副 消耗 ID</summary>
		[ProtoMember(20)]
		public int Consume_Id_4 { get; set; }
		/// <summary>副 消耗 数量</summary>
		[ProtoMember(21)]
		public int Consume_Num_4 { get; set; }
		/// <summary>可选关系 0-可不选 1-必选其一</summary>
		[ProtoMember(22)]
		public int Consume_Choose { get; set; }
		/// <summary>可选 消耗 类型</summary>
		[ProtoMember(23)]
		public int Consume_Choose_Type_1 { get; set; }
		/// <summary>可选 消耗 ID</summary>
		[ProtoMember(24)]
		public int Consume_Choose_Id_1 { get; set; }
		/// <summary>可选 消耗 数量</summary>
		[ProtoMember(25)]
		public int Consume_Choose_Num_1 { get; set; }
		/// <summary>可选 消耗 作用</summary>
		[ProtoMember(26)]
		public int Consume_Choose_Effect_1 { get; set; }
		/// <summary>可选 消耗 类型</summary>
		[ProtoMember(27)]
		public int Consume_Choose_Type_2 { get; set; }
		/// <summary>可选 消耗 ID</summary>
		[ProtoMember(28)]
		public int Consume_Choose_Id_2 { get; set; }
		/// <summary>可选 消耗 数量</summary>
		[ProtoMember(29)]
		public int Consume_Choose_Num_2 { get; set; }
		/// <summary>可选 消耗 作用</summary>
		[ProtoMember(30)]
		public int Consume_Choose_Effect_2 { get; set; }

	}
}
