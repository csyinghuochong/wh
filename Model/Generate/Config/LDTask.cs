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
		/// <summary>页码</summary>
		[ProtoMember(2)]
		public int Page { get; set; }
		/// <summary>排序</summary>
		[ProtoMember(3)]
		public int Order_SL { get; set; }
		/// <summary>类型</summary>
		[ProtoMember(4)]
		public int Type { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(5)]
		public int Name { get; set; }
		/// <summary>描述</summary>
		[ProtoMember(6)]
		public int Desc { get; set; }
		/// <summary>条件</summary>
		[ProtoMember(7)]
		public int Condition_Type { get; set; }
		/// <summary>主参数</summary>
		[ProtoMember(8)]
		public int Param1 { get; set; }
		/// <summary>副参数</summary>
		[ProtoMember(9)]
		public int Param2 { get; set; }
		/// <summary>完成NPC</summary>
		[ProtoMember(10)]
		public int NPC { get; set; }
		/// <summary>奖励选择 0-全拿 1-任选</summary>
		[ProtoMember(11)]
		public int Reward_Option { get; set; }
		/// <summary>共用 奖励</summary>
		[ProtoMember(12)]
		public string Reward { get; set; }
		/// <summary>战士 奖励</summary>
		[ProtoMember(13)]
		public string Reward_Occupation_1 { get; set; }
		/// <summary>猎人 奖励</summary>
		[ProtoMember(14)]
		public string Reward_Occupation_2 { get; set; }
		/// <summary>刺客 奖励</summary>
		[ProtoMember(15)]
		public string Reward_Occupation_3 { get; set; }
		/// <summary>法师 奖励</summary>
		[ProtoMember(16)]
		public string Reward_Occupation_4 { get; set; }
		/// <summary>侠士 奖励</summary>
		[ProtoMember(17)]
		public string Reward_Occupation_5 { get; set; }
		/// <summary>牧师 奖励</summary>
		[ProtoMember(18)]
		public string Reward_Occupation_6 { get; set; }

	}
}
