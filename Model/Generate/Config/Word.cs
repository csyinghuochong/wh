using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class WordCategory : ProtoObject, IMerge
    {
        public static WordCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, Word> dict = new Dictionary<int, Word>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<Word> list = new List<Word>();
		
        public WordCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            WordCategory s = o as WordCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (Word config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public Word Get(int id)
        {
            this.dict.TryGetValue(id, out Word item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (Word)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, Word> GetAll()
        {
            return this.dict;
        }

        public Word GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class Word: ProtoObject, IConfig
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
