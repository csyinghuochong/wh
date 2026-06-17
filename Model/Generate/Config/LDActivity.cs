using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDActivityCategory : ProtoObject, IMerge
    {
        public static LDActivityCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDActivity> dict = new Dictionary<int, LDActivity>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDActivity> list = new List<LDActivity>();
		
        public LDActivityCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDActivityCategory s = o as LDActivityCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDActivity config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDActivity Get(int id)
        {
            this.dict.TryGetValue(id, out LDActivity item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDActivity)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDActivity> GetAll()
        {
            return this.dict;
        }

        public LDActivity GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDActivity: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>类型 1-开服 2-限时 9-触发</summary>
		[ProtoMember(2)]
		public int Type { get; set; }
		/// <summary>排序</summary>
		[ProtoMember(3)]
		public int Order_SL { get; set; }
		/// <summary>关闭 0-否 1-是</summary>
		[ProtoMember(4)]
		public int Is_Close { get; set; }

	}
}
