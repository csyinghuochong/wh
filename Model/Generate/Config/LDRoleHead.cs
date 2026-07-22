using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDRoleHeadCategory : ProtoObject, IMerge
    {
        public static LDRoleHeadCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDRoleHead> dict = new Dictionary<int, LDRoleHead>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDRoleHead> list = new List<LDRoleHead>();
		
        public LDRoleHeadCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDRoleHeadCategory s = o as LDRoleHeadCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDRoleHead config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDRoleHead Get(int id)
        {
            this.dict.TryGetValue(id, out LDRoleHead item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDRoleHead)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDRoleHead> GetAll()
        {
            return this.dict;
        }

        public LDRoleHead GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDRoleHead: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>类型</summary>
		[ProtoMember(2)]
		public int Type { get; set; }
		/// <summary>头像</summary>
		[ProtoMember(3)]
		public string Icon { get; set; }

	}
}
