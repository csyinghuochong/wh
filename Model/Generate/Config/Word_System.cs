using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class Word_SystemCategory : ProtoObject, IMerge
    {
        public static Word_SystemCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, Word_System> dict = new Dictionary<int, Word_System>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<Word_System> list = new List<Word_System>();
		
        public Word_SystemCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            Word_SystemCategory s = o as Word_SystemCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (Word_System config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public Word_System Get(int id)
        {
            this.dict.TryGetValue(id, out Word_System item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (Word_System)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, Word_System> GetAll()
        {
            return this.dict;
        }

        public Word_System GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class Word_System: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }

	}
}
