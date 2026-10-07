using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDWord_BlockCategory : ProtoObject, IMerge
    {
        public static LDWord_BlockCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDWord_Block> dict = new Dictionary<int, LDWord_Block>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDWord_Block> list = new List<LDWord_Block>();
		
        public LDWord_BlockCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDWord_BlockCategory s = o as LDWord_BlockCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDWord_Block config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDWord_Block)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDWord_Block Get(int id)
        {
            this.dict.TryGetValue(id, out LDWord_Block item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDWord_Block)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDWord_Block> GetAll()
        {
            return this.dict;
        }

        public LDWord_Block GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDWord_Block: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>中文</summary>
		[ProtoMember(2)]
		public string CN { get; set; }

	}
}
