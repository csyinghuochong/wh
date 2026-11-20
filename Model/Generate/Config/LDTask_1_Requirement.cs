using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDTask_1_RequirementCategory : ProtoObject, IMerge
    {
        public static LDTask_1_RequirementCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDTask_1_Requirement> dict = new Dictionary<int, LDTask_1_Requirement>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDTask_1_Requirement> list = new List<LDTask_1_Requirement>();
		
        public LDTask_1_RequirementCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDTask_1_RequirementCategory s = o as LDTask_1_RequirementCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDTask_1_Requirement config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDTask_1_Requirement)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDTask_1_Requirement Get(int id)
        {
            this.dict.TryGetValue(id, out LDTask_1_Requirement item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDTask_1_Requirement)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDTask_1_Requirement> GetAll()
        {
            return this.dict;
        }

        public LDTask_1_Requirement GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDTask_1_Requirement: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>条件</summary>
		[ProtoMember(2)]
		public int Condition_Type { get; set; }
		/// <summary>主参数</summary>
		[ProtoMember(3)]
		public int Param1 { get; set; }
		/// <summary>副参数</summary>
		[ProtoMember(4)]
		public int Param2 { get; set; }
		/// <summary>副参数</summary>
		[ProtoMember(5)]
		public int Param3 { get; set; }
		/// <summary>副参数</summary>
		[ProtoMember(6)]
		public int Param4 { get; set; }

	}
}
