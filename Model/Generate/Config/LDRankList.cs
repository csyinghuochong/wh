using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDRankListCategory : ProtoObject, IMerge
    {
        public static LDRankListCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDRankList> dict = new Dictionary<int, LDRankList>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDRankList> list = new List<LDRankList>();
		
        public LDRankListCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDRankListCategory s = o as LDRankListCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDRankList config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDRankList Get(int id)
        {
            this.dict.TryGetValue(id, out LDRankList item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDRankList)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDRankList> GetAll()
        {
            return this.dict;
        }

        public LDRankList GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDRankList: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>类型</summary>
		[ProtoMember(2)]
		public int Type { get; set; }
		/// <summary>排名</summary>
		[ProtoMember(3)]
		public int Rank_Min { get; set; }
		/// <summary>排名</summary>
		[ProtoMember(4)]
		public int Rank_Max { get; set; }
		/// <summary>奖励</summary>
		[ProtoMember(5)]
		public string Reward { get; set; }

	}
}
