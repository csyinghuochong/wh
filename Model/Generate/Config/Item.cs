using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class ItemCategory : ProtoObject, IMerge
    {
        public static ItemCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, Item> dict = new Dictionary<int, Item>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<Item> list = new List<Item>();
		
        public ItemCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            ItemCategory s = o as ItemCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (Item config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public Item Get(int id)
        {
            this.dict.TryGetValue(id, out Item item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (Item)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, Item> GetAll()
        {
            return this.dict;
        }

        public Item GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class Item: ProtoObject, IConfig
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
		/// <summary>Icon</summary>
		[ProtoMember(4)]
		public string Icon { get; set; }
		/// <summary>品质</summary>
		[ProtoMember(5)]
		public int Quality { get; set; }
		/// <summary>使用等级</summary>
		[ProtoMember(6)]
		public int UseLv { get; set; }
		/// <summary>使用职业</summary>
		[ProtoMember(7)]
		public int UseOcc { get; set; }
		/// <summary>类型</summary>
		[ProtoMember(8)]
		public int ItemType { get; set; }
		/// <summary>子类</summary>
		[ProtoMember(9)]
		public int ItemSubType { get; set; }
		/// <summary>使用参数</summary>
		[ProtoMember(10)]
		public string ItemUsePar { get; set; }
		/// <summary>最大堆叠</summary>
		[ProtoMember(11)]
		public int ItemPileSum { get; set; }
		/// <summary>出售类型</summary>
		[ProtoMember(12)]
		public int SellMoneyType { get; set; }
		/// <summary>出售值</summary>
		[ProtoMember(13)]
		public int SellMoneyValue { get; set; }
		/// <summary>洗练石 数量</summary>
		[ProtoMember(14)]
		public int[] XiLianStone { get; set; }
		/// <summary>回收 获取物品</summary>
		[ProtoMember(15)]
		public string HuiShouGetItem { get; set; }
		/// <summary>自动使用 0-否 1-是</summary>
		[ProtoMember(16)]
		public int IfAutoUse { get; set; }
		/// <summary>拍卖上架 0-禁止 1-允许</summary>
		[ProtoMember(17)]
		public int IfStopPaiMai { get; set; }
		/// <summary>获取绑定 0-否 1-是</summary>
		[ProtoMember(18)]
		public int IfLock { get; set; }
		/// <summary>每天 使用次数</summary>
		[ProtoMember(19)]
		public int DayUseNum { get; set; }
		/// <summary>总共 使用次数</summary>
		[ProtoMember(20)]
		public int SumUseNum { get; set; }
		/// <summary>显示特效</summary>
		[ProtoMember(21)]
		public string EquipEffect { get; set; }

	}
}
