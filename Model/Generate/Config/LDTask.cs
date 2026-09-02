using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDTaskCategory : ProtoObject, IMerge
    {
        public static LDTaskCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDTask> dict = new Dictionary<int, LDTask>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDTask> list = new List<LDTask>();
		
        public LDTaskCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDTaskCategory s = o as LDTaskCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDTask config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDTask)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDTask Get(int id)
        {
            this.dict.TryGetValue(id, out LDTask item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDTask)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDTask> GetAll()
        {
            return this.dict;
        }

        public LDTask GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDTask: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>下一 任务</summary>
		[ProtoMember(2)]
		public int[] Next_Id { get; set; }
		/// <summary>类型 0-全开 1-随机</summary>
		[ProtoMember(3)]
		public int Next_Id_Type { get; set; }
		/// <summary>任务结束 重新计数</summary>
		[ProtoMember(4)]
		public int Recount { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(5)]
		public int Name { get; set; }
		/// <summary>描述</summary>
		[ProtoMember(6)]
		public int Desc_Target { get; set; }
		/// <summary>任务 等级</summary>
		[ProtoMember(7)]
		public int Task_Lv { get; set; }
		/// <summary>难度</summary>
		[ProtoMember(8)]
		public int Difficult { get; set; }
		/// <summary>接取 等级</summary>
		[ProtoMember(9)]
		public int Accept_Lv_Min { get; set; }
		/// <summary>接取 等级</summary>
		[ProtoMember(10)]
		public int Accept_Lv_Max { get; set; }
		/// <summary>其他 接取 要求</summary>
		[ProtoMember(11)]
		public string Accept { get; set; }
		/// <summary>接取 NPC</summary>
		[ProtoMember(12)]
		public int NPC_Accept { get; set; }
		/// <summary>接取 对话</summary>
		[ProtoMember(13)]
		public int NPC_Accept_Dialogue_1 { get; set; }
		/// <summary>接取 对话</summary>
		[ProtoMember(14)]
		public int NPC_Accept_Dialogue_2 { get; set; }
		/// <summary>接取 对话</summary>
		[ProtoMember(15)]
		public int NPC_Accept_Dialogue_3 { get; set; }
		/// <summary>接取 对话</summary>
		[ProtoMember(16)]
		public int NPC_Accept_Dialogue_4 { get; set; }
		/// <summary>要求1</summary>
		[ProtoMember(17)]
		public int Requirement_1 { get; set; }
		/// <summary>要求2</summary>
		[ProtoMember(18)]
		public int Requirement_2 { get; set; }
		/// <summary>要求3</summary>
		[ProtoMember(19)]
		public int Requirement_3 { get; set; }
		/// <summary>要求4</summary>
		[ProtoMember(20)]
		public int Requirement_4 { get; set; }
		/// <summary>完成 NPC</summary>
		[ProtoMember(21)]
		public int NPC_Finish { get; set; }
		/// <summary>未完成 对话</summary>
		[ProtoMember(22)]
		public int NPC_Finish_Dialogue_1 { get; set; }
		/// <summary>完成 对话</summary>
		[ProtoMember(23)]
		public int NPC_Finish_Dialogue_2 { get; set; }
		/// <summary>角色 经验 参数</summary>
		[ProtoMember(24)]
		public int Exp_Role_Param1 { get; set; }
		/// <summary>角色 经验 参数</summary>
		[ProtoMember(25)]
		public int Exp_Role_Param2 { get; set; }
		/// <summary>角色 经验 参数</summary>
		[ProtoMember(26)]
		public int Exp_Role_Param3 { get; set; }
		/// <summary>绑定 钱币</summary>
		[ProtoMember(27)]
		public int Gold_1 { get; set; }
		/// <summary>非绑 钱币</summary>
		[ProtoMember(28)]
		public int Gold_2 { get; set; }
		/// <summary>奖励选择 0-全拿 1-任选</summary>
		[ProtoMember(29)]
		public int Reward_Option { get; set; }
		/// <summary>共用 奖励</summary>
		[ProtoMember(30)]
		public string Reward { get; set; }
		/// <summary>战士 奖励</summary>
		[ProtoMember(31)]
		public string Reward_Occupation_10 { get; set; }
		/// <summary>猎人 奖励</summary>
		[ProtoMember(32)]
		public string Reward_Occupation_11 { get; set; }
		/// <summary>刺客 奖励</summary>
		[ProtoMember(33)]
		public string Reward_Occupation_12 { get; set; }
		/// <summary>法师 奖励</summary>
		[ProtoMember(34)]
		public string Reward_Occupation_15 { get; set; }
		/// <summary>侠士 奖励</summary>
		[ProtoMember(35)]
		public string Reward_Occupation_16 { get; set; }
		/// <summary>牧师 奖励</summary>
		[ProtoMember(36)]
		public string Reward_Occupation_17 { get; set; }
		/// <summary>组 1-999</summary>
		[ProtoMember(37)]
		public int Group { get; set; }
		/// <summary>页内 排序</summary>
		[ProtoMember(38)]
		public int Order_SL { get; set; }
		/// <summary>子组 0-99</summary>
		[ProtoMember(39)]
		public int Sub_Group { get; set; }
		/// <summary>子组 编号 0-999</summary>
		[ProtoMember(40)]
		public int Sub_Group_Number { get; set; }
		/// <summary>描述</summary>
		[ProtoMember(41)]
		public int Desc { get; set; }
		/// <summary>条件</summary>
		[ProtoMember(42)]
		public int Condition_Type { get; set; }
		/// <summary>主参数</summary>
		[ProtoMember(43)]
		public int Param1 { get; set; }
		/// <summary>副参数</summary>
		[ProtoMember(44)]
		public int Param2 { get; set; }
		/// <summary>副参数</summary>
		[ProtoMember(45)]
		public int Param3 { get; set; }
		/// <summary>副参数</summary>
		[ProtoMember(46)]
		public int Param4 { get; set; }

	}
}
