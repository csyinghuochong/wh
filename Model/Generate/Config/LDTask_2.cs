using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDTask_2Category : ProtoObject, IMerge
    {
        public static LDTask_2Category Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDTask_2> dict = new Dictionary<int, LDTask_2>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDTask_2> list = new List<LDTask_2>();
		
        public LDTask_2Category()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDTask_2Category s = o as LDTask_2Category;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDTask_2 config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDTask_2)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDTask_2 Get(int id)
        {
            this.dict.TryGetValue(id, out LDTask_2 item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDTask_2)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDTask_2> GetAll()
        {
            return this.dict;
        }

        public LDTask_2 GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDTask_2: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>组 1-999</summary>
		[ProtoMember(2)]
		public int Group { get; set; }
		/// <summary>页内 排序</summary>
		[ProtoMember(3)]
		public int Order_SL { get; set; }
		/// <summary>子组 0-99</summary>
		[ProtoMember(4)]
		public int Sub_Group { get; set; }
		/// <summary>子组 编号 0-999</summary>
		[ProtoMember(5)]
		public int Sub_Group_Number { get; set; }
		/// <summary>前置任务 重新计数</summary>
		[ProtoMember(6)]
		public int Recount { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(7)]
		public int Name { get; set; }
		/// <summary>描述</summary>
		[ProtoMember(8)]
		public int Desc { get; set; }
		/// <summary>条件</summary>
		[ProtoMember(9)]
		public int Condition_Type { get; set; }
		/// <summary>主参数</summary>
		[ProtoMember(10)]
		public int Param1 { get; set; }
		/// <summary>副参数</summary>
		[ProtoMember(11)]
		public int Param2 { get; set; }
		/// <summary>副参数</summary>
		[ProtoMember(12)]
		public int Param3 { get; set; }
		/// <summary>完成NPC</summary>
		[ProtoMember(13)]
		public int NPC { get; set; }
		/// <summary>角色经验 类型 0-固定 1-公式</summary>
		[ProtoMember(14)]
		public int Exp_1_Type { get; set; }
		/// <summary>角色 经验</summary>
		[ProtoMember(15)]
		public int Exp_1 { get; set; }
		/// <summary>奖励选择 0-全拿 1-任选</summary>
		[ProtoMember(16)]
		public int Reward_Option { get; set; }
		/// <summary>共用 奖励</summary>
		[ProtoMember(17)]
		public string Reward { get; set; }
		/// <summary>战士 奖励</summary>
		[ProtoMember(18)]
		public string Reward_Occupation_10 { get; set; }
		/// <summary>猎人 奖励</summary>
		[ProtoMember(19)]
		public string Reward_Occupation_11 { get; set; }
		/// <summary>刺客 奖励</summary>
		[ProtoMember(20)]
		public string Reward_Occupation_12 { get; set; }
		/// <summary>法师 奖励</summary>
		[ProtoMember(21)]
		public string Reward_Occupation_15 { get; set; }
		/// <summary>侠士 奖励</summary>
		[ProtoMember(22)]
		public string Reward_Occupation_16 { get; set; }
		/// <summary>牧师 奖励</summary>
		[ProtoMember(23)]
		public string Reward_Occupation_17 { get; set; }

	}
}
