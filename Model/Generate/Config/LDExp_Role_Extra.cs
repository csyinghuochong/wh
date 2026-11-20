using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDExp_Role_ExtraCategory : ProtoObject, IMerge
    {
        public static LDExp_Role_ExtraCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDExp_Role_Extra> dict = new Dictionary<int, LDExp_Role_Extra>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDExp_Role_Extra> list = new List<LDExp_Role_Extra>();
		
        public LDExp_Role_ExtraCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDExp_Role_ExtraCategory s = o as LDExp_Role_ExtraCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDExp_Role_Extra config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDExp_Role_Extra)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDExp_Role_Extra Get(int id)
        {
            this.dict.TryGetValue(id, out LDExp_Role_Extra item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDExp_Role_Extra)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDExp_Role_Extra> GetAll()
        {
            return this.dict;
        }

        public LDExp_Role_Extra GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDExp_Role_Extra: ProtoObject, IConfig
	{
		/// <summary>等级差</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>额外百分比</summary>
		[ProtoMember(2)]
		public int Exp_Extra_Rate { get; set; }

	}
}
