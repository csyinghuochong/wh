using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDGlobalValueCategory : ProtoObject, IMerge
    {
        public static LDGlobalValueCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDGlobalValue> dict = new Dictionary<int, LDGlobalValue>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDGlobalValue> list = new List<LDGlobalValue>();
		
        public LDGlobalValueCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDGlobalValueCategory s = o as LDGlobalValueCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDGlobalValue config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDGlobalValue)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDGlobalValue Get(int id)
        {
            this.dict.TryGetValue(id, out LDGlobalValue item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDGlobalValue)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDGlobalValue> GetAll()
        {
            return this.dict;
        }

        public LDGlobalValue GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDGlobalValue: ProtoObject, IConfig
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
