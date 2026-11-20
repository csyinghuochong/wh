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
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDRoleHead)} Id={config.Id}");
				}
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
		/// <summary>名称</summary>
		[ProtoMember(2)]
		public int Name { get; set; }
		/// <summary>描述</summary>
		[ProtoMember(3)]
		public int Desc { get; set; }
		/// <summary>来源</summary>
		[ProtoMember(4)]
		public int Desc_Access_Method { get; set; }
		/// <summary>头像</summary>
		[ProtoMember(5)]
		public string Icon { get; set; }
		/// <summary>职业</summary>
		[ProtoMember(6)]
		public int Occupation { get; set; }
		/// <summary>性别</summary>
		[ProtoMember(7)]
		public int Sex { get; set; }
		/// <summary>排序</summary>
		[ProtoMember(8)]
		public int Order_LS { get; set; }
		/// <summary>跳转</summary>
		[ProtoMember(9)]
		public int Jump { get; set; }

	}
}
