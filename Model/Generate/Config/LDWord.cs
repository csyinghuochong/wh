using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDWordCategory : ProtoObject, IMerge
    {
        public static LDWordCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDWord> dict = new Dictionary<int, LDWord>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDWord> list = new List<LDWord>();
		
        public LDWordCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDWordCategory s = o as LDWordCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDWord config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDWord)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDWord Get(int id)
        {
            this.dict.TryGetValue(id, out LDWord item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDWord)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDWord> GetAll()
        {
            return this.dict;
        }

        public LDWord GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDWord: ProtoObject, IConfig
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
