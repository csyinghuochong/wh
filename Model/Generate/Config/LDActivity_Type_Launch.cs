using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDActivity_Type_LaunchCategory : ProtoObject, IMerge
    {
        public static LDActivity_Type_LaunchCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDActivity_Type_Launch> dict = new Dictionary<int, LDActivity_Type_Launch>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDActivity_Type_Launch> list = new List<LDActivity_Type_Launch>();
		
        public LDActivity_Type_LaunchCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDActivity_Type_LaunchCategory s = o as LDActivity_Type_LaunchCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDActivity_Type_Launch config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDActivity_Type_Launch)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDActivity_Type_Launch Get(int id)
        {
            this.dict.TryGetValue(id, out LDActivity_Type_Launch item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDActivity_Type_Launch)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDActivity_Type_Launch> GetAll()
        {
            return this.dict;
        }

        public LDActivity_Type_Launch GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDActivity_Type_Launch: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>≥天开启</summary>
		[ProtoMember(2)]
		public int Day_Open { get; set; }
		/// <summary>持续天数</summary>
		[ProtoMember(3)]
		public int During_Day { get; set; }
		/// <summary>持续秒数</summary>
		[ProtoMember(4)]
		public int During_Second { get; set; }

	}
}
