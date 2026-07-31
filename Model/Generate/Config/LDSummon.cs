using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDSummonCategory : ProtoObject, IMerge
    {
        public static LDSummonCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDSummon> dict = new Dictionary<int, LDSummon>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDSummon> list = new List<LDSummon>();
		
        public LDSummonCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDSummonCategory s = o as LDSummonCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDSummon config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDSummon)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDSummon Get(int id)
        {
            this.dict.TryGetValue(id, out LDSummon item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDSummon)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDSummon> GetAll()
        {
            return this.dict;
        }

        public LDSummon GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDSummon: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>类型</summary>
		[ProtoMember(2)]
		public int Type { get; set; }
		/// <summary>特效ID</summary>
		[ProtoMember(3)]
		public int Effect_ID { get; set; }
		/// <summary>技能1</summary>
		[ProtoMember(4)]
		public int Skill_1 { get; set; }
		/// <summary>技能2</summary>
		[ProtoMember(5)]
		public int Skill_2 { get; set; }
		/// <summary>速度</summary>
		[ProtoMember(6)]
		public int Speed { get; set; }

	}
}
