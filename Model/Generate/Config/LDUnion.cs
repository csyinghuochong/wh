using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDUnionCategory : ProtoObject, IMerge
    {
        public static LDUnionCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDUnion> dict = new Dictionary<int, LDUnion>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDUnion> list = new List<LDUnion>();
		
        public LDUnionCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDUnionCategory s = o as LDUnionCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDUnion config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDUnion Get(int id)
        {
            this.dict.TryGetValue(id, out LDUnion item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDUnion)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDUnion> GetAll()
        {
            return this.dict;
        }

        public LDUnion GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDUnion: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>升级经验</summary>
		[ProtoMember(2)]
		public int Exp { get; set; }
		/// <summary>修炼上限</summary>
		[ProtoMember(3)]
		public int XiuLianLevel { get; set; }
		/// <summary>人员上限</summary>
		[ProtoMember(4)]
		public int PeopleNum { get; set; }
		/// <summary>捐献消耗金币</summary>
		[ProtoMember(5)]
		public int DonateGold { get; set; }
		/// <summary>捐献消耗钻石</summary>
		[ProtoMember(6)]
		public int DonateDiamond { get; set; }
		/// <summary>捐献增加经验 mix|max</summary>
		[ProtoMember(7)]
		public int[] DonateExp { get; set; }
		/// <summary>捐献增加贡献值 min|max</summary>
		[ProtoMember(8)]
		public int[] DonateReward { get; set; }
		/// <summary>升级全员奖励</summary>
		[ProtoMember(9)]
		public string UpAllReward { get; set; }
		/// <summary>捐献增加家族金币</summary>
		[ProtoMember(10)]
		public int[] AddUnionGold { get; set; }
		/// <summary>家族金币上限</summary>
		[ProtoMember(11)]
		public int UnionGoldLimit { get; set; }

	}
}
