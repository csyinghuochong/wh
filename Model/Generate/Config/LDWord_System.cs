using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDWord_SystemCategory : ProtoObject, IMerge
    {
        public static LDWord_SystemCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDWord_System> dict = new Dictionary<int, LDWord_System>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDWord_System> list = new List<LDWord_System>();
		
        public LDWord_SystemCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDWord_SystemCategory s = o as LDWord_SystemCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDWord_System config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDWord_System Get(int id)
        {
            this.dict.TryGetValue(id, out LDWord_System item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDWord_System)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDWord_System> GetAll()
        {
            return this.dict;
        }

        public LDWord_System GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDWord_System: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>中文</summary>
		[ProtoMember(2)]
		public string CN { get; set; }

	}
}
