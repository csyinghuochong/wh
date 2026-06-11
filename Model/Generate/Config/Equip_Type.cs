using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class Equip_TypeCategory : ProtoObject, IMerge
    {
        public static Equip_TypeCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, Equip_Type> dict = new Dictionary<int, Equip_Type>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<Equip_Type> list = new List<Equip_Type>();
		
        public Equip_TypeCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            Equip_TypeCategory s = o as Equip_TypeCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (Equip_Type config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public Equip_Type Get(int id)
        {
            this.dict.TryGetValue(id, out Equip_Type item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (Equip_Type)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, Equip_Type> GetAll()
        {
            return this.dict;
        }

        public Equip_Type GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class Equip_Type: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(2)]
		public int Name { get; set; }
		/// <summary>包含子类</summary>
		[ProtoMember(3)]
		public int[] Type_Sub { get; set; }

	}
}
