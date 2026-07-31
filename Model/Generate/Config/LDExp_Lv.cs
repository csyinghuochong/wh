using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDExp_LvCategory : ProtoObject, IMerge
    {
        public static LDExp_LvCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDExp_Lv> dict = new Dictionary<int, LDExp_Lv>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDExp_Lv> list = new List<LDExp_Lv>();
		
        public LDExp_LvCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDExp_LvCategory s = o as LDExp_LvCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDExp_Lv config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDExp_Lv)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDExp_Lv Get(int id)
        {
            this.dict.TryGetValue(id, out LDExp_Lv item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDExp_Lv)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDExp_Lv> GetAll()
        {
            return this.dict;
        }

        public LDExp_Lv GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDExp_Lv: ProtoObject, IConfig
	{
		/// <summary>等级</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>角色升级经验</summary>
		[ProtoMember(2)]
		public int Exp_Role { get; set; }
		/// <summary>角色标准生命</summary>
		[ProtoMember(3)]
		public int Hp_Standard { get; set; }

	}
}
