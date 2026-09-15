using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDPrivilege_CardCategory : ProtoObject, IMerge
    {
        public static LDPrivilege_CardCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDPrivilege_Card> dict = new Dictionary<int, LDPrivilege_Card>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDPrivilege_Card> list = new List<LDPrivilege_Card>();
		
        public LDPrivilege_CardCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDPrivilege_CardCategory s = o as LDPrivilege_CardCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDPrivilege_Card config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDPrivilege_Card)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDPrivilege_Card Get(int id)
        {
            this.dict.TryGetValue(id, out LDPrivilege_Card item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDPrivilege_Card)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDPrivilege_Card> GetAll()
        {
            return this.dict;
        }

        public LDPrivilege_Card GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDPrivilege_Card: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }

	}
}
