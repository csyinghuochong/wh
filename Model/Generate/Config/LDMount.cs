using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDMountCategory : ProtoObject, IMerge
    {
        public static LDMountCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDMount> dict = new Dictionary<int, LDMount>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDMount> list = new List<LDMount>();
		
        public LDMountCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDMountCategory s = o as LDMountCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDMount config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDMount)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDMount Get(int id)
        {
            this.dict.TryGetValue(id, out LDMount item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDMount)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDMount> GetAll()
        {
            return this.dict;
        }

        public LDMount GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDMount: ProtoObject, IConfig
	{
		/// <summary>ID</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(2)]
		public int Name { get; set; }
		/// <summary>体资质</summary>
		[ProtoMember(3)]
		public int[] Aptitude_1 { get; set; }
		/// <summary>力资质</summary>
		[ProtoMember(4)]
		public int[] Aptitude_2 { get; set; }
		/// <summary>智资质</summary>
		[ProtoMember(5)]
		public int[] Aptitude_3 { get; set; }
		/// <summary>念资质</summary>
		[ProtoMember(6)]
		public int[] Aptitude_4 { get; set; }
		/// <summary>敏资质</summary>
		[ProtoMember(7)]
		public int[] Aptitude_5 { get; set; }
		/// <summary>迅资质</summary>
		[ProtoMember(8)]
		public int[] Aptitude_6 { get; set; }
		/// <summary>启用</summary>
		[ProtoMember(9)]
		public int Enable { get; set; }

	}
}
