using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDWord_NameCategory : ProtoObject, IMerge
    {
        public static LDWord_NameCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDWord_Name> dict = new Dictionary<int, LDWord_Name>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDWord_Name> list = new List<LDWord_Name>();
		
        public LDWord_NameCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDWord_NameCategory s = o as LDWord_NameCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDWord_Name config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDWord_Name)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDWord_Name Get(int id)
        {
            this.dict.TryGetValue(id, out LDWord_Name item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDWord_Name)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDWord_Name> GetAll()
        {
            return this.dict;
        }

        public LDWord_Name GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDWord_Name: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>组</summary>
		[ProtoMember(2)]
		public int Group { get; set; }
		/// <summary>参数</summary>
		[ProtoMember(3)]
		public int Param { get; set; }
		/// <summary>中文</summary>
		[ProtoMember(4)]
		public string CN { get; set; }

	}
}
