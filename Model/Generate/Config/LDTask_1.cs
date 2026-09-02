using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDTask_1Category : ProtoObject, IMerge
    {
        public static LDTask_1Category Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDTask_1> dict = new Dictionary<int, LDTask_1>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDTask_1> list = new List<LDTask_1>();
		
        public LDTask_1Category()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDTask_1Category s = o as LDTask_1Category;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDTask_1 config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDTask_1)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDTask_1 Get(int id)
        {
            this.dict.TryGetValue(id, out LDTask_1 item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDTask_1)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDTask_1> GetAll()
        {
            return this.dict;
        }

        public LDTask_1 GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDTask_1: ProtoObject, IConfig
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

	}
}
