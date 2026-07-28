using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDGem_HoleCategory : ProtoObject, IMerge
    {
        public static LDGem_HoleCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDGem_Hole> dict = new Dictionary<int, LDGem_Hole>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDGem_Hole> list = new List<LDGem_Hole>();
		
        public LDGem_HoleCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDGem_HoleCategory s = o as LDGem_HoleCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDGem_Hole config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDGem_Hole Get(int id)
        {
            this.dict.TryGetValue(id, out LDGem_Hole item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDGem_Hole)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDGem_Hole> GetAll()
        {
            return this.dict;
        }

        public LDGem_Hole GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDGem_Hole: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(2)]
		public int Name { get; set; }
		/// <summary>图标</summary>
		[ProtoMember(3)]
		public string Icon { get; set; }
		/// <summary>位置</summary>
		[ProtoMember(4)]
		public int Group { get; set; }
		/// <summary>权重</summary>
		[ProtoMember(5)]
		public int Weight { get; set; }

	}
}
