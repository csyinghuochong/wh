using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDItemCategory : ProtoObject, IMerge
    {
        public static LDItemCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDItem> dict = new Dictionary<int, LDItem>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDItem> list = new List<LDItem>();
		
        public LDItemCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDItemCategory s = o as LDItemCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDItem config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDItem)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDItem Get(int id)
        {
            this.dict.TryGetValue(id, out LDItem item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDItem)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDItem> GetAll()
        {
            return this.dict;
        }

        public LDItem GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDItem: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>相同 Id</summary>
		[ProtoMember(2)]
		public int Same_Id { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(3)]
		public int Name { get; set; }
		/// <summary>描述</summary>
		[ProtoMember(4)]
		public int Desc { get; set; }
		/// <summary>Icon</summary>
		[ProtoMember(5)]
		public string Icon { get; set; }
		/// <summary>品质</summary>
		[ProtoMember(6)]
		public int Quality { get; set; }
		/// <summary>最小 使用 等级</summary>
		[ProtoMember(7)]
		public int UseLv_Min { get; set; }
		/// <summary>最大 使用 等级</summary>
		[ProtoMember(8)]
		public int UseLv_Max { get; set; }
		/// <summary>子 类</summary>
		[ProtoMember(9)]
		public int ItemType { get; set; }
		/// <summary>参数1</summary>
		[ProtoMember(10)]
		public string ItemTypeParam1 { get; set; }
		/// <summary>参数2</summary>
		[ProtoMember(11)]
		public string ItemTypeParam2 { get; set; }
		/// <summary>参数3</summary>
		[ProtoMember(12)]
		public string ItemTypeParam3 { get; set; }
		/// <summary>参数4</summary>
		[ProtoMember(13)]
		public string ItemTypeParam4 { get; set; }
		/// <summary>奖励</summary>
		[ProtoMember(14)]
		public int Drop_Id { get; set; }
		/// <summary>堆叠</summary>
		[ProtoMember(15)]
		public int ItemPileSum { get; set; }
		/// <summary>出售ID</summary>
		[ProtoMember(16)]
		public int Sell_ID { get; set; }
		/// <summary>出售值</summary>
		[ProtoMember(17)]
		public int Sell_Num { get; set; }
		/// <summary>进背包 0-否 1-是</summary>
		[ProtoMember(18)]
		public int IfBag { get; set; }
		/// <summary>背包类型 1-装备 2-奇珍 3-材料 4-消耗</summary>
		[ProtoMember(19)]
		public int BagType { get; set; }
		/// <summary>自动 使用 0-否 1-是</summary>
		[ProtoMember(20)]
		public int IfAutoUse { get; set; }
		/// <summary>获取 绑定 0-否 1-是</summary>
		[ProtoMember(21)]
		public int IfLock { get; set; }
		/// <summary>交易 所属</summary>
		[ProtoMember(22)]
		public int Exchange_Belong { get; set; }
		/// <summary>每天 使用 次数</summary>
		[ProtoMember(23)]
		public int DayUseNum { get; set; }
		/// <summary>总共 使用 次数</summary>
		[ProtoMember(24)]
		public int SumUseNum { get; set; }
		/// <summary>显示特效</summary>
		[ProtoMember(25)]
		public string EquipEffect { get; set; }
		/// <summary>排序</summary>
		[ProtoMember(26)]
		public int Order_LS { get; set; }

	}
}
