using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDEquip_SuitCategory : ProtoObject, IMerge
    {
        public static LDEquip_SuitCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDEquip_Suit> dict = new Dictionary<int, LDEquip_Suit>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDEquip_Suit> list = new List<LDEquip_Suit>();
		
        public LDEquip_SuitCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDEquip_SuitCategory s = o as LDEquip_SuitCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDEquip_Suit config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDEquip_Suit)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDEquip_Suit Get(int id)
        {
            this.dict.TryGetValue(id, out LDEquip_Suit item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDEquip_Suit)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDEquip_Suit> GetAll()
        {
            return this.dict;
        }

        public LDEquip_Suit GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDEquip_Suit: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(2)]
		public int Name { get; set; }
		/// <summary>部位</summary>
		[ProtoMember(3)]
		public int[] Type_Id { get; set; }
		/// <summary>装备ID</summary>
		[ProtoMember(4)]
		public int[] Equip_Id { get; set; }
		/// <summary>效果组</summary>
		[ProtoMember(5)]
		public string Effect_Id { get; set; }

	}
}
