using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDTextCategory : ProtoObject, IMerge
    {
        public static LDTextCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDText> dict = new Dictionary<int, LDText>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDText> list = new List<LDText>();
		
        public LDTextCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDTextCategory s = o as LDTextCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDText config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDText Get(int id)
        {
            this.dict.TryGetValue(id, out LDText item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDText)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDText> GetAll()
        {
            return this.dict;
        }

        public LDText GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDText: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>中文</summary>
		[ProtoMember(2)]
		public string CN { get; set; }
		/// <summary>英文</summary>
		[ProtoMember(3)]
		public string EN { get; set; }

	}
}
