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
		/// <summary>穿戴等级</summary>
		[ProtoMember(6)]
		public int UseLv { get; set; }
		/// <summary>品质</summary>
		[ProtoMember(7)]
		public int Quality { get; set; }
		/// <summary>鉴定道具</summary>
		[ProtoMember(8)]
		public int AppraisalItem { get; set; }
		/// <summary>套装ID</summary>
		[ProtoMember(9)]
		public int EquipSuitID { get; set; }
		/// <summary>血量</summary>
		[ProtoMember(10)]
		public int Equip_Hp { get; set; }
		/// <summary>物攻min</summary>
		[ProtoMember(11)]
		public int Equip_MinAct { get; set; }
		/// <summary>物攻max</summary>
		[ProtoMember(12)]
		public int Equip_MaxAct { get; set; }
		/// <summary>法攻min</summary>
		[ProtoMember(13)]
		public int Equip_MinMagAct { get; set; }
		/// <summary>法攻max</summary>
		[ProtoMember(14)]
		public int Equip_MaxMagAct { get; set; }
		/// <summary>最低防御</summary>
		[ProtoMember(15)]
		public int Equip_MinDef { get; set; }
		/// <summary>最高防御</summary>
		[ProtoMember(16)]
		public int Equip_MaxDef { get; set; }
		/// <summary>最低防御</summary>
		[ProtoMember(17)]
		public int Equip_MinAdf { get; set; }
		/// <summary>最高防御</summary>
		[ProtoMember(18)]
		public int Equip_MaxAdf { get; set; }
		/// <summary>暴击</summary>
		[ProtoMember(19)]
		public double Equip_Cri { get; set; }
		/// <summary>命中</summary>
		[ProtoMember(20)]
		public double Equip_Hit { get; set; }
		/// <summary>闪避</summary>
		[ProtoMember(21)]
		public double Equip_Dodge { get; set; }
		/// <summary>伤害加成</summary>
		[ProtoMember(22)]
		public double Equip_DamgeAdd { get; set; }
		/// <summary>伤害减免</summary>
		[ProtoMember(23)]
		public double Equip_DamgeSub { get; set; }
		/// <summary>速度</summary>
		[ProtoMember(24)]
		public double Equip_Speed { get; set; }
		/// <summary>幸运值</summary>
		[ProtoMember(25)]
		public int Equip_Lucky { get; set; }
		/// <summary>获取绑定 0-否 1-是</summary>
		[ProtoMember(26)]
		public int IfLock { get; set; }

	}
}
