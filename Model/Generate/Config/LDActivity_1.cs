using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDActivity_1Category : ProtoObject, IMerge
    {
        public static LDActivity_1Category Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDActivity_1> dict = new Dictionary<int, LDActivity_1>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDActivity_1> list = new List<LDActivity_1>();
		
        public LDActivity_1Category()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDActivity_1Category s = o as LDActivity_1Category;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDActivity_1 config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDActivity_1 Get(int id)
        {
            this.dict.TryGetValue(id, out LDActivity_1 item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDActivity_1)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDActivity_1> GetAll()
        {
            return this.dict;
        }

        public LDActivity_1 GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDActivity_1: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>开启首购</summary>
		[ProtoMember(2)]
		public int Is_First { get; set; }
		/// <summary>首购额外奖励</summary>
		[ProtoMember(3)]
		public string First_Reward { get; set; }
		/// <summary>非首购额外奖励</summary>
		[ProtoMember(4)]
		public int Extra_Reward { get; set; }

	}
}
