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
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDPrivilege)} Id={config.Id}");
				}
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
		/// <summary>组</summary>
		[ProtoMember(2)]
		public int Group { get; set; }
		/// <summary>效果Id</summary>
		[ProtoMember(3)]
		public int Effect_Id { get; set; }
		/// <summary>效果值</summary>
		[ProtoMember(4)]
		public int Effect_Value { get; set; }
		/// <summary>0-新 1-亮 2-无 3-藏</summary>
		[ProtoMember(5)]
		public int Show_Type { get; set; }
		/// <summary>排序</summary>
		[ProtoMember(6)]
		public int Order_SL { get; set; }

	}
}
