using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDPrivilege_EffectCategory : ProtoObject, IMerge
    {
        public static LDPrivilege_EffectCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDPrivilege_Effect> dict = new Dictionary<int, LDPrivilege_Effect>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDPrivilege_Effect> list = new List<LDPrivilege_Effect>();
		
        public LDPrivilege_EffectCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDPrivilege_EffectCategory s = o as LDPrivilege_EffectCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDPrivilege_Effect config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDPrivilege_Effect)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDPrivilege_Effect Get(int id)
        {
            this.dict.TryGetValue(id, out LDPrivilege_Effect item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDPrivilege_Effect)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDPrivilege_Effect> GetAll()
        {
            return this.dict;
        }

        public LDPrivilege_Effect GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDPrivilege_Effect: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>描述</summary>
		[ProtoMember(2)]
		public int Desc { get; set; }
		/// <summary>排序参数</summary>
		[ProtoMember(3)]
		public int Order_Param { get; set; }

	}
}
