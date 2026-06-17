using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDActivity_111Category : ProtoObject, IMerge
    {
        public static LDActivity_111Category Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDActivity_111> dict = new Dictionary<int, LDActivity_111>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDActivity_111> list = new List<LDActivity_111>();
		
        public LDActivity_111Category()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDActivity_111Category s = o as LDActivity_111Category;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDActivity_111 config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDActivity_111 Get(int id)
        {
            this.dict.TryGetValue(id, out LDActivity_111 item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDActivity_111)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDActivity_111> GetAll()
        {
            return this.dict;
        }

        public LDActivity_111 GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDActivity_111: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>Next_Id</summary>
		[ProtoMember(2)]
		public int Next_Id { get; set; }
		/// <summary>累计在线</summary>
		[ProtoMember(3)]
		public int Online_Time { get; set; }
		/// <summary>奖励</summary>
		[ProtoMember(4)]
		public string Reward { get; set; }

	}
}
