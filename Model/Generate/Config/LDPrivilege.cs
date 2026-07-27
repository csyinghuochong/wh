using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDPrivilegeCategory : ProtoObject, IMerge
    {
        public static LDPrivilegeCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDPrivilege> dict = new Dictionary<int, LDPrivilege>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDPrivilege> list = new List<LDPrivilege>();
		
        public LDPrivilegeCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDPrivilegeCategory s = o as LDPrivilegeCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDPrivilege config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDPrivilege Get(int id)
        {
            this.dict.TryGetValue(id, out LDPrivilege item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDPrivilege)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDPrivilege> GetAll()
        {
            return this.dict;
        }

        public LDPrivilege GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDPrivilege: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }

	}
}
