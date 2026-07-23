using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDAttribute_ShowCategory : ProtoObject, IMerge
    {
        public static LDAttribute_ShowCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDAttribute_Show> dict = new Dictionary<int, LDAttribute_Show>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDAttribute_Show> list = new List<LDAttribute_Show>();
		
        public LDAttribute_ShowCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDAttribute_ShowCategory s = o as LDAttribute_ShowCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDAttribute_Show config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDAttribute_Show Get(int id)
        {
            this.dict.TryGetValue(id, out LDAttribute_Show item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDAttribute_Show)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDAttribute_Show> GetAll()
        {
            return this.dict;
        }

        public LDAttribute_Show GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDAttribute_Show: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>类型</summary>
		[ProtoMember(2)]
		public int Type { get; set; }
		/// <summary>位置</summary>
		[ProtoMember(3)]
		public int Position { get; set; }
		/// <summary>属性ID</summary>
		[ProtoMember(4)]
		public int Attribute_Id { get; set; }

	}
}
