using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDActivity_Type_LimitCategory : ProtoObject, IMerge
    {
        public static LDActivity_Type_LimitCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDActivity_Type_Limit> dict = new Dictionary<int, LDActivity_Type_Limit>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDActivity_Type_Limit> list = new List<LDActivity_Type_Limit>();
		
        public LDActivity_Type_LimitCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDActivity_Type_LimitCategory s = o as LDActivity_Type_LimitCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDActivity_Type_Limit config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDActivity_Type_Limit)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDActivity_Type_Limit Get(int id)
        {
            this.dict.TryGetValue(id, out LDActivity_Type_Limit item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDActivity_Type_Limit)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDActivity_Type_Limit> GetAll()
        {
            return this.dict;
        }

        public LDActivity_Type_Limit GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDActivity_Type_Limit: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>预告时间</summary>
		[ProtoMember(2)]
		public string Time_Preview { get; set; }
		/// <summary>开始时间</summary>
		[ProtoMember(3)]
		public string Time_Start { get; set; }
		/// <summary>结束时间</summary>
		[ProtoMember(4)]
		public string Time_End { get; set; }
		/// <summary>结算时间</summary>
		[ProtoMember(5)]
		public string Time_Settlement { get; set; }
		/// <summary>最终时间</summary>
		[ProtoMember(6)]
		public string Time_Finally { get; set; }

	}
}
