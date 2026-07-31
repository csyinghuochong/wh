using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDEquip_AppraiseCategory : ProtoObject, IMerge
    {
        public static LDEquip_AppraiseCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDEquip_Appraise> dict = new Dictionary<int, LDEquip_Appraise>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDEquip_Appraise> list = new List<LDEquip_Appraise>();
		
        public LDEquip_AppraiseCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDEquip_AppraiseCategory s = o as LDEquip_AppraiseCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDEquip_Appraise config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDEquip_Appraise)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDEquip_Appraise Get(int id)
        {
            this.dict.TryGetValue(id, out LDEquip_Appraise item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDEquip_Appraise)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDEquip_Appraise> GetAll()
        {
            return this.dict;
        }

        public LDEquip_Appraise GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDEquip_Appraise: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>属性 类型</summary>
		[ProtoMember(2)]
		public int Attribute_Type { get; set; }
		/// <summary>鉴定 等级</summary>
		[ProtoMember(3)]
		public int Appraise_Lv { get; set; }
		/// <summary>属性 最小值</summary>
		[ProtoMember(4)]
		public int Attribute_Min { get; set; }
		/// <summary>属性 最大值</summary>
		[ProtoMember(5)]
		public int Attribute_Max { get; set; }

	}
}
