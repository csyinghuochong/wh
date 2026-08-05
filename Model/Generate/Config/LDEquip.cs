using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDEquipCategory : ProtoObject, IMerge
    {
        public static LDEquipCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDEquip> dict = new Dictionary<int, LDEquip>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDEquip> list = new List<LDEquip>();
		
        public LDEquipCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDEquipCategory s = o as LDEquipCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDEquip config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDEquip)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDEquip Get(int id)
        {
            this.dict.TryGetValue(id, out LDEquip item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDEquip)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDEquip> GetAll()
        {
            return this.dict;
        }

        public LDEquip GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDEquip: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(2)]
		public int Name { get; set; }
		/// <summary>图标</summary>
		[ProtoMember(3)]
		public string Icon { get; set; }
		/// <summary>模型</summary>
		[ProtoMember(4)]
		public string Model { get; set; }
		/// <summary>类型</summary>
		[ProtoMember(5)]
		public int Sub_Type { get; set; }
		/// <summary>穿戴 等级</summary>
		[ProtoMember(6)]
		public int UseLv { get; set; }
		/// <summary>品质</summary>
		[ProtoMember(7)]
		public int Quality { get; set; }
		/// <summary>职业</summary>
		[ProtoMember(8)]
		public int[] Occupation { get; set; }
		/// <summary>套装ID</summary>
		[ProtoMember(9)]
		public int EquipSuitID { get; set; }
		/// <summary>套装 点数</summary>
		[ProtoMember(10)]
		public int EquipSuitParam { get; set; }
		/// <summary>必有属性</summary>
		[ProtoMember(11)]
		public string Attribute { get; set; }
		/// <summary>加入 数量</summary>
		[ProtoMember(12)]
		public int Att_Rand_Num { get; set; }
		/// <summary>组1 属性 (12)3 - 属性ID 456 - 值 / 最小值 (789) - 最大值</summary>
		[ProtoMember(13)]
		public int[] Att_Rand_Param11 { get; set; }
		/// <summary>组1 最大 个数</summary>
		[ProtoMember(14)]
		public int Att_Rand_Param12 { get; set; }
		/// <summary>组1 加入</summary>
		[ProtoMember(15)]
		public int Att_Rand_Param13 { get; set; }
		/// <summary>组2 属性 (12)3 - 属性ID 456 - 值 / 最小值 (789) - 最大值</summary>
		[ProtoMember(16)]
		public string Att_Rand_Param21 { get; set; }
		/// <summary>组2 最大 个数</summary>
		[ProtoMember(17)]
		public int Att_Rand_Param22 { get; set; }
		/// <summary>组2 加入</summary>
		[ProtoMember(18)]
		public int Att_Rand_Param23 { get; set; }
		/// <summary>组3 属性 (12)3 - 属性ID 456 - 值 / 最小值 (789) - 最大值</summary>
		[ProtoMember(19)]
		public string Att_Rand_Param31 { get; set; }
		/// <summary>组3 最大 个数</summary>
		[ProtoMember(20)]
		public int Att_Rand_Param32 { get; set; }
		/// <summary>组3 加入</summary>
		[ProtoMember(21)]
		public int Att_Rand_Param33 { get; set; }
		/// <summary>组4 属性 (12)3 - 属性ID 456 - 值 / 最小值 (789) - 最大值</summary>
		[ProtoMember(22)]
		public string Att_Rand_Param41 { get; set; }
		/// <summary>组4 最大个数</summary>
		[ProtoMember(23)]
		public int Att_Rand_Param42 { get; set; }
		/// <summary>组4 加入</summary>
		[ProtoMember(24)]
		public int Att_Rand_Param43 { get; set; }
		/// <summary>组5 属性 (12)3 - 属性ID 456 - 值 / 最小值 (789) - 最大值</summary>
		[ProtoMember(25)]
		public string Att_Rand_Param51 { get; set; }
		/// <summary>组5 最大个数</summary>
		[ProtoMember(26)]
		public int Att_Rand_Param52 { get; set; }
		/// <summary>组5 加入</summary>
		[ProtoMember(27)]
		public int Att_Rand_Param53 { get; set; }
		/// <summary>属性 必中1</summary>
		[ProtoMember(28)]
		public int[] Att_Rand_1 { get; set; }
		/// <summary>属性 必中2</summary>
		[ProtoMember(29)]
		public int[] Att_Rand_2 { get; set; }
		/// <summary>属性 必中3</summary>
		[ProtoMember(30)]
		public int[] Att_Rand_3 { get; set; }
		/// <summary>属性 必中4</summary>
		[ProtoMember(31)]
		public int[] Att_Rand_4 { get; set; }
		/// <summary>属性 必中5</summary>
		[ProtoMember(32)]
		public int[] Att_Rand_5 { get; set; }
		/// <summary>属性 必中6</summary>
		[ProtoMember(33)]
		public int[] Att_Rand_6 { get; set; }
		/// <summary>强化上限 0：无强化 -1：禁强化</summary>
		[ProtoMember(34)]
		public int Enhance { get; set; }
		/// <summary>强化属性</summary>
		[ProtoMember(35)]
		public string Enhance_Attribute { get; set; }
		/// <summary>鉴定等级 0：无鉴定 -1：禁鉴定</summary>
		[ProtoMember(36)]
		public int Appraise_Lv { get; set; }
		/// <summary>可鉴定 属性类型</summary>
		[ProtoMember(37)]
		public int[] Appraise_Attribute { get; set; }

	}
}
