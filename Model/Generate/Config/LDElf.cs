using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDElfCategory : ProtoObject, IMerge
    {
        public static LDElfCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDElf> dict = new Dictionary<int, LDElf>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDElf> list = new List<LDElf>();
		
        public LDElfCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDElfCategory s = o as LDElfCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDElf config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDElf)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDElf Get(int id)
        {
            this.dict.TryGetValue(id, out LDElf item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDElf)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDElf> GetAll()
        {
            return this.dict;
        }

        public LDElf GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDElf: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(2)]
		public int Name { get; set; }
		/// <summary>属性描述</summary>
		[ProtoMember(3)]
		public int Desc_Att { get; set; }
		/// <summary>能力描述</summary>
		[ProtoMember(4)]
		public int Desc_Ability { get; set; }
		/// <summary>获取描述</summary>
		[ProtoMember(5)]
		public int Desc_Get { get; set; }

	}
}
