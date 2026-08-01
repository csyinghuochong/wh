using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDItemTypeCategory : ProtoObject, IMerge
    {
        public static LDItemTypeCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDItemType> dict = new Dictionary<int, LDItemType>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDItemType> list = new List<LDItemType>();
		
        public LDItemTypeCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDItemTypeCategory s = o as LDItemTypeCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDItemType config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDItemType)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDItemType Get(int id)
        {
            this.dict.TryGetValue(id, out LDItemType item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDItemType)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDItemType> GetAll()
        {
            return this.dict;
        }

        public LDItemType GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDItemType: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(2)]
		public int Name { get; set; }
		/// <summary>可装配</summary>
		[ProtoMember(3)]
		public int Is_Configure { get; set; }

	}
}
