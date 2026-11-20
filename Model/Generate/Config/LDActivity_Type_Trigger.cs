using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDActivity_Type_TriggerCategory : ProtoObject, IMerge
    {
        public static LDActivity_Type_TriggerCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDActivity_Type_Trigger> dict = new Dictionary<int, LDActivity_Type_Trigger>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDActivity_Type_Trigger> list = new List<LDActivity_Type_Trigger>();
		
        public LDActivity_Type_TriggerCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDActivity_Type_TriggerCategory s = o as LDActivity_Type_TriggerCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDActivity_Type_Trigger config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDActivity_Type_Trigger)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDActivity_Type_Trigger Get(int id)
        {
            this.dict.TryGetValue(id, out LDActivity_Type_Trigger item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDActivity_Type_Trigger)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDActivity_Type_Trigger> GetAll()
        {
            return this.dict;
        }

        public LDActivity_Type_Trigger GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDActivity_Type_Trigger: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>触发类型</summary>
		[ProtoMember(2)]
		public int Trigger_Type { get; set; }
		/// <summary>参数1</summary>
		[ProtoMember(3)]
		public int Param1 { get; set; }
		/// <summary>参数2</summary>
		[ProtoMember(4)]
		public int Param2 { get; set; }
		/// <summary>持续天数</summary>
		[ProtoMember(5)]
		public int During_Day { get; set; }
		/// <summary>持续秒数</summary>
		[ProtoMember(6)]
		public int During_Second { get; set; }

	}
}
