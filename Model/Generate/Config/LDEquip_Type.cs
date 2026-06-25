using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDEquip_TypeCategory : ProtoObject, IMerge
    {
        public static LDEquip_TypeCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDEquip_Type> dict = new Dictionary<int, LDEquip_Type>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDEquip_Type> list = new List<LDEquip_Type>();
		
        public LDEquip_TypeCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDEquip_TypeCategory s = o as LDEquip_TypeCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDEquip_Type config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDEquip_Type Get(int id)
        {
            this.dict.TryGetValue(id, out LDEquip_Type item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDEquip_Type)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDEquip_Type> GetAll()
        {
            return this.dict;
        }

        public LDEquip_Type GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDEquip_Type: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(2)]
		public int Name { get; set; }

	}
}
