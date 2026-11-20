using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDUnion_FlagCategory : ProtoObject, IMerge
    {
        public static LDUnion_FlagCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDUnion_Flag> dict = new Dictionary<int, LDUnion_Flag>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDUnion_Flag> list = new List<LDUnion_Flag>();
		
        public LDUnion_FlagCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDUnion_FlagCategory s = o as LDUnion_FlagCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDUnion_Flag config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDUnion_Flag)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDUnion_Flag Get(int id)
        {
            this.dict.TryGetValue(id, out LDUnion_Flag item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDUnion_Flag)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDUnion_Flag> GetAll()
        {
            return this.dict;
        }

        public LDUnion_Flag GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDUnion_Flag: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>资源</summary>
		[ProtoMember(2)]
		public string Resources { get; set; }

	}
}
