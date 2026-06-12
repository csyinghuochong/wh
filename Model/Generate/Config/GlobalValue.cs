using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class GlobalValueCategory : ProtoObject, IMerge
    {
        public static GlobalValueCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, GlobalValue> dict = new Dictionary<int, GlobalValue>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<GlobalValue> list = new List<GlobalValue>();
		
        public GlobalValueCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            GlobalValueCategory s = o as GlobalValueCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (GlobalValue config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public GlobalValue Get(int id)
        {
            this.dict.TryGetValue(id, out GlobalValue item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (GlobalValue)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, GlobalValue> GetAll()
        {
            return this.dict;
        }

        public GlobalValue GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class GlobalValue: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>唯一索引</summary>
		[ProtoMember(2)]
		public string Key { get; set; }
		/// <summary>值</summary>
		[ProtoMember(3)]
		public string Value { get; set; }

	}
}
