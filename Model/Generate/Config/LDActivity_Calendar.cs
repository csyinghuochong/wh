using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDActivity_CalendarCategory : ProtoObject, IMerge
    {
        public static LDActivity_CalendarCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDActivity_Calendar> dict = new Dictionary<int, LDActivity_Calendar>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDActivity_Calendar> list = new List<LDActivity_Calendar>();
		
        public LDActivity_CalendarCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDActivity_CalendarCategory s = o as LDActivity_CalendarCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDActivity_Calendar config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDActivity_Calendar Get(int id)
        {
            this.dict.TryGetValue(id, out LDActivity_Calendar item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDActivity_Calendar)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDActivity_Calendar> GetAll()
        {
            return this.dict;
        }

        public LDActivity_Calendar GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDActivity_Calendar: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>描述</summary>
		[ProtoMember(2)]
		public int Desc { get; set; }
		/// <summary>资源</summary>
		[ProtoMember(3)]
		public string Resources { get; set; }
		/// <summary>奖励展示</summary>
		[ProtoMember(4)]
		public string Reward_Show { get; set; }

	}
}
