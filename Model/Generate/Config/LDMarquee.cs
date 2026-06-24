using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDMarqueeCategory : ProtoObject, IMerge
    {
        public static LDMarqueeCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDMarquee> dict = new Dictionary<int, LDMarquee>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDMarquee> list = new List<LDMarquee>();
		
        public LDMarqueeCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDMarqueeCategory s = o as LDMarqueeCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDMarquee config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDMarquee Get(int id)
        {
            this.dict.TryGetValue(id, out LDMarquee item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDMarquee)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDMarquee> GetAll()
        {
            return this.dict;
        }

        public LDMarquee GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDMarquee: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }

	}
}
