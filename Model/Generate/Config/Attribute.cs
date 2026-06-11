using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class AttributeCategory : ProtoObject, IMerge
    {
        public static AttributeCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, Attribute> dict = new Dictionary<int, Attribute>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<Attribute> list = new List<Attribute>();
		
        public AttributeCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            AttributeCategory s = o as AttributeCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (Attribute config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public Attribute Get(int id)
        {
            this.dict.TryGetValue(id, out Attribute item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (Attribute)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, Attribute> GetAll()
        {
            return this.dict;
        }

        public Attribute GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class Attribute: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(2)]
		public int Name { get; set; }
		/// <summary>描述</summary>
		[ProtoMember(3)]
		public int Desc { get; set; }
		/// <summary>排序</summary>
		[ProtoMember(4)]
		public int Order_SL { get; set; }
		/// <summary>显示</summary>
		[ProtoMember(5)]
		public int IsShow { get; set; }

	}
}
