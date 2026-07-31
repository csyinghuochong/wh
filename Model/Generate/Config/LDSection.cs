using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDSectionCategory : ProtoObject, IMerge
    {
        public static LDSectionCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDSection> dict = new Dictionary<int, LDSection>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDSection> list = new List<LDSection>();
		
        public LDSectionCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDSectionCategory s = o as LDSectionCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDSection config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDSection)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDSection Get(int id)
        {
            this.dict.TryGetValue(id, out LDSection item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDSection)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDSection> GetAll()
        {
            return this.dict;
        }

        public LDSection GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDSection: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(2)]
		public int Name { get; set; }
		/// <summary>场景ID</summary>
		[ProtoMember(3)]
		public int[] Scene_Id { get; set; }

	}
}
