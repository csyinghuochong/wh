using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDHomeCategory : ProtoObject, IMerge
    {
        public static LDHomeCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDHome> dict = new Dictionary<int, LDHome>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDHome> list = new List<LDHome>();
		
        public LDHomeCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDHomeCategory s = o as LDHomeCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDHome config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDHome Get(int id)
        {
            this.dict.TryGetValue(id, out LDHome item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDHome)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDHome> GetAll()
        {
            return this.dict;
        }

        public LDHome GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDHome: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>经验</summary>
		[ProtoMember(2)]
		public int Exp { get; set; }
		/// <summary>农场上限</summary>
		[ProtoMember(3)]
		public int Limit_Farm { get; set; }
		/// <summary>牧场上限</summary>
		[ProtoMember(4)]
		public int Limit_Ranch { get; set; }

	}
}
