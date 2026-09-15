using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDVIPCategory : ProtoObject, IMerge
    {
        public static LDVIPCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDVIP> dict = new Dictionary<int, LDVIP>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDVIP> list = new List<LDVIP>();
		
        public LDVIPCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDVIPCategory s = o as LDVIPCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDVIP config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDVIP)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDVIP Get(int id)
        {
            this.dict.TryGetValue(id, out LDVIP item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDVIP)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDVIP> GetAll()
        {
            return this.dict;
        }

        public LDVIP GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDVIP: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>经验</summary>
		[ProtoMember(2)]
		public int Exp { get; set; }
		/// <summary>特权ID</summary>
		[ProtoMember(3)]
		public int Privilege_Group { get; set; }
		/// <summary>显示条件</summary>
		[ProtoMember(4)]
		public int Show_Level { get; set; }
		/// <summary>奖励</summary>
		[ProtoMember(5)]
		public string Reward { get; set; }
		/// <summary>每日奖励</summary>
		[ProtoMember(6)]
		public string Reward_Daily { get; set; }
		/// <summary>礼包奖励</summary>
		[ProtoMember(7)]
		public string Package_Pay { get; set; }
		/// <summary>礼包消耗</summary>
		[ProtoMember(8)]
		public string Package_Consume { get; set; }

	}
}
