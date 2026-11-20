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
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDUnion)} Id={config.Id}");
				}
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
		/// <summary>经验</summary>
		[ProtoMember(2)]
		public int Exp { get; set; }
		/// <summary>人数限制</summary>
		[ProtoMember(3)]
		public int Limit_Player { get; set; }

	}
}
