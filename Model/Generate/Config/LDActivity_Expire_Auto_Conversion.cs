using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDActivity_Expire_Auto_ConversionCategory : ProtoObject, IMerge
    {
        public static LDActivity_Expire_Auto_ConversionCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDActivity_Expire_Auto_Conversion> dict = new Dictionary<int, LDActivity_Expire_Auto_Conversion>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDActivity_Expire_Auto_Conversion> list = new List<LDActivity_Expire_Auto_Conversion>();
		
        public LDActivity_Expire_Auto_ConversionCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDActivity_Expire_Auto_ConversionCategory s = o as LDActivity_Expire_Auto_ConversionCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDActivity_Expire_Auto_Conversion config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDActivity_Expire_Auto_Conversion)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDActivity_Expire_Auto_Conversion Get(int id)
        {
            this.dict.TryGetValue(id, out LDActivity_Expire_Auto_Conversion item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDActivity_Expire_Auto_Conversion)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDActivity_Expire_Auto_Conversion> GetAll()
        {
            return this.dict;
        }

        public LDActivity_Expire_Auto_Conversion GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDActivity_Expire_Auto_Conversion: ProtoObject, IConfig
	{
		/// <summary>活动ID</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>消耗类型</summary>
		[ProtoMember(2)]
		public int Consume_Type { get; set; }
		/// <summary>消耗ID</summary>
		[ProtoMember(3)]
		public int Consume_Id { get; set; }
		/// <summary>消耗值</summary>
		[ProtoMember(4)]
		public int Consume_Num { get; set; }
		/// <summary>奖励类型</summary>
		[ProtoMember(5)]
		public int Reward_Type { get; set; }
		/// <summary>奖励ID</summary>
		[ProtoMember(6)]
		public int Reward_Id { get; set; }
		/// <summary>奖励数量</summary>
		[ProtoMember(7)]
		public int Reward_Num { get; set; }
		/// <summary>邮件标题</summary>
		[ProtoMember(8)]
		public int Mail_Title { get; set; }
		/// <summary>邮件内容</summary>
		[ProtoMember(9)]
		public int Mail_Content { get; set; }

	}
}
