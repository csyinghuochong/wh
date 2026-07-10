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
		/// <summary>属性</summary>
		[ProtoMember(11)]
		public string Attribute { get; set; }
		/// <summary>可否 鉴定</summary>
		[ProtoMember(12)]
		public int Appraise { get; set; }
		/// <summary>强化 上限</summary>
		[ProtoMember(13)]
		public int Enhance { get; set; }
		/// <summary>强化属性</summary>
		[ProtoMember(14)]
		public string Enhance_Attribute { get; set; }

	}
}
