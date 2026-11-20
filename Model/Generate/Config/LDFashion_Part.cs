using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDFashion_PartCategory : ProtoObject, IMerge
    {
        public static LDFashion_PartCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDFashion_Part> dict = new Dictionary<int, LDFashion_Part>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDFashion_Part> list = new List<LDFashion_Part>();
		
        public LDFashion_PartCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDFashion_PartCategory s = o as LDFashion_PartCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDFashion_Part config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDFashion_Part)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDFashion_Part Get(int id)
        {
            this.dict.TryGetValue(id, out LDFashion_Part item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDFashion_Part)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDFashion_Part> GetAll()
        {
            return this.dict;
        }

        public LDFashion_Part GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDFashion_Part: ProtoObject, IConfig
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

	}
}
