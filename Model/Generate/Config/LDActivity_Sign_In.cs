using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDActivity_Sign_InCategory : ProtoObject, IMerge
    {
        public static LDActivity_Sign_InCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDActivity_Sign_In> dict = new Dictionary<int, LDActivity_Sign_In>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDActivity_Sign_In> list = new List<LDActivity_Sign_In>();
		
        public LDActivity_Sign_InCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDActivity_Sign_InCategory s = o as LDActivity_Sign_InCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDActivity_Sign_In config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDActivity_Sign_In Get(int id)
        {
            this.dict.TryGetValue(id, out LDActivity_Sign_In item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDActivity_Sign_In)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDActivity_Sign_In> GetAll()
        {
            return this.dict;
        }

        public LDActivity_Sign_In GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDActivity_Sign_In: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>活动Id</summary>
		[ProtoMember(2)]
		public int ActivityId { get; set; }
		/// <summary>天数</summary>
		[ProtoMember(3)]
		public int Sign_Day { get; set; }
		/// <summary>奖励</summary>
		[ProtoMember(4)]
		public string Reward { get; set; }
		/// <summary>周期组（1=第1个28天，2=第2个…；超出最大组后沿用最大组）</summary>
		[ProtoMember(5)]
		public int Group { get; set; }

	}
}
