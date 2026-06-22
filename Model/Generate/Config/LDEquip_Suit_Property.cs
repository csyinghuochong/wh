using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDEquip_Suit_PropertyCategory : ProtoObject, IMerge
    {
        public static LDEquip_Suit_PropertyCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDEquip_Suit_Property> dict = new Dictionary<int, LDEquip_Suit_Property>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDEquip_Suit_Property> list = new List<LDEquip_Suit_Property>();
		
        public LDEquip_Suit_PropertyCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDEquip_Suit_PropertyCategory s = o as LDEquip_Suit_PropertyCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDEquip_Suit_Property config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDEquip_Suit_Property Get(int id)
        {
            this.dict.TryGetValue(id, out LDEquip_Suit_Property item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDEquip_Suit_Property)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDEquip_Suit_Property> GetAll()
        {
            return this.dict;
        }

        public LDEquip_Suit_Property GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDEquip_Suit_Property: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(2)]
		public int Name { get; set; }
		/// <summary>属性</summary>
		[ProtoMember(3)]
		public string Attribute { get; set; }

	}
}
