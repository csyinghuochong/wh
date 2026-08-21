using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDSkill_Make_GroupCategory : ProtoObject, IMerge
    {
        public static LDSkill_Make_GroupCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDSkill_Make_Group> dict = new Dictionary<int, LDSkill_Make_Group>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDSkill_Make_Group> list = new List<LDSkill_Make_Group>();
		
        public LDSkill_Make_GroupCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDSkill_Make_GroupCategory s = o as LDSkill_Make_GroupCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDSkill_Make_Group config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDSkill_Make_Group)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDSkill_Make_Group Get(int id)
        {
            this.dict.TryGetValue(id, out LDSkill_Make_Group item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDSkill_Make_Group)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDSkill_Make_Group> GetAll()
        {
            return this.dict;
        }

        public LDSkill_Make_Group GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDSkill_Make_Group: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(2)]
		public int Name { get; set; }
		/// <summary>类型</summary>
		[ProtoMember(3)]
		public int Type { get; set; }

	}
}
