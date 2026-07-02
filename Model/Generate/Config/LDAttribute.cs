using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDAttributeCategory : ProtoObject, IMerge
    {
        public static LDAttributeCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDAttribute> dict = new Dictionary<int, LDAttribute>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDAttribute> list = new List<LDAttribute>();
		
        public LDAttributeCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDAttributeCategory s = o as LDAttributeCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDAttribute config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDAttribute Get(int id)
        {
            this.dict.TryGetValue(id, out LDAttribute item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDAttribute)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDAttribute> GetAll()
        {
            return this.dict;
        }

        public LDAttribute GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDAttribute: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(2)]
		public int Name { get; set; }
		/// <summary>面板 描述</summary>
		[ProtoMember(3)]
		public int Desc1 { get; set; }
		/// <summary>属性 描述</summary>
		[ProtoMember(4)]
		public int Desc2 { get; set; }
		/// <summary>类型 0-固定值 1-万分比</summary>
		[ProtoMember(5)]
		public int Type { get; set; }

	}
}
