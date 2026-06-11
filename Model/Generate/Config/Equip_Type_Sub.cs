using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class Equip_Type_SubCategory : ProtoObject, IMerge
    {
        public static Equip_Type_SubCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, Equip_Type_Sub> dict = new Dictionary<int, Equip_Type_Sub>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<Equip_Type_Sub> list = new List<Equip_Type_Sub>();
		
        public Equip_Type_SubCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            Equip_Type_SubCategory s = o as Equip_Type_SubCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (Equip_Type_Sub config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public Equip_Type_Sub Get(int id)
        {
            this.dict.TryGetValue(id, out Equip_Type_Sub item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (Equip_Type_Sub)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, Equip_Type_Sub> GetAll()
        {
            return this.dict;
        }

        public Equip_Type_Sub GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class Equip_Type_Sub: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(2)]
		public int Name { get; set; }

	}
}
