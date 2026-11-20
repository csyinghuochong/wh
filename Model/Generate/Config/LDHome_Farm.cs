using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDHome_FarmCategory : ProtoObject, IMerge
    {
        public static LDHome_FarmCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDHome_Farm> dict = new Dictionary<int, LDHome_Farm>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDHome_Farm> list = new List<LDHome_Farm>();
		
        public LDHome_FarmCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDHome_FarmCategory s = o as LDHome_FarmCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDHome_Farm config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDHome_Farm)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDHome_Farm Get(int id)
        {
            this.dict.TryGetValue(id, out LDHome_Farm item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDHome_Farm)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDHome_Farm> GetAll()
        {
            return this.dict;
        }

        public LDHome_Farm GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDHome_Farm: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>初始模型</summary>
		[ProtoMember(2)]
		public string Model_Init { get; set; }
		/// <summary>时间</summary>
		[ProtoMember(3)]
		public int Time_1 { get; set; }
		/// <summary>模型</summary>
		[ProtoMember(4)]
		public string Model_1 { get; set; }
		/// <summary>时间</summary>
		[ProtoMember(5)]
		public int Time_2 { get; set; }
		/// <summary>模型</summary>
		[ProtoMember(6)]
		public string Model_2 { get; set; }
		/// <summary>时间</summary>
		[ProtoMember(7)]
		public int Time_3 { get; set; }
		/// <summary>模型</summary>
		[ProtoMember(8)]
		public string Model_3 { get; set; }
		/// <summary>奖励</summary>
		[ProtoMember(9)]
		public string Reward { get; set; }

	}
}
