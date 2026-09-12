using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDUnion_IconCategory : ProtoObject, IMerge
    {
        public static LDUnion_IconCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDUnion_Icon> dict = new Dictionary<int, LDUnion_Icon>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDUnion_Icon> list = new List<LDUnion_Icon>();
		
        public LDUnion_IconCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDUnion_IconCategory s = o as LDUnion_IconCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDUnion_Icon config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDUnion_Icon)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDUnion_Icon Get(int id)
        {
            this.dict.TryGetValue(id, out LDUnion_Icon item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDUnion_Icon)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDUnion_Icon> GetAll()
        {
            return this.dict;
        }

        public LDUnion_Icon GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDUnion_Icon: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>资源</summary>
		[ProtoMember(2)]
		public string Resources { get; set; }

	}
}
