using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDAICategory : ProtoObject, IMerge
    {
        public static LDAICategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDAI> dict = new Dictionary<int, LDAI>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDAI> list = new List<LDAI>();
		
        public LDAICategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDAICategory s = o as LDAICategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDAI config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDAI)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDAI Get(int id)
        {
            this.dict.TryGetValue(id, out LDAI item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDAI)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDAI> GetAll()
        {
            return this.dict;
        }

        public LDAI GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDAI: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }

	}
}
