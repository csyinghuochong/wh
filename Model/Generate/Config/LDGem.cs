using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDGemCategory : ProtoObject, IMerge
    {
        public static LDGemCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDGem> dict = new Dictionary<int, LDGem>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDGem> list = new List<LDGem>();
		
        public LDGemCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDGemCategory s = o as LDGemCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDGem config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDGem Get(int id)
        {
            this.dict.TryGetValue(id, out LDGem item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDGem)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDGem> GetAll()
        {
            return this.dict;
        }

        public LDGem GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDGem: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(2)]
		public int Name { get; set; }
		/// <summary>Icon</summary>
		[ProtoMember(3)]
		public string Icon { get; set; }
		/// <summary>品质</summary>
		[ProtoMember(4)]
		public int Quality { get; set; }

	}
}
