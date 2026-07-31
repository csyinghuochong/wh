using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDSkill_LvCategory : ProtoObject, IMerge
    {
        public static LDSkill_LvCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDSkill_Lv> dict = new Dictionary<int, LDSkill_Lv>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDSkill_Lv> list = new List<LDSkill_Lv>();
		
        public LDSkill_LvCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDSkill_LvCategory s = o as LDSkill_LvCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDSkill_Lv config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDSkill_Lv)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDSkill_Lv Get(int id)
        {
            this.dict.TryGetValue(id, out LDSkill_Lv item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDSkill_Lv)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDSkill_Lv> GetAll()
        {
            return this.dict;
        }

        public LDSkill_Lv GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDSkill_Lv: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>技能 ID</summary>
		[ProtoMember(2)]
		public int Skill_Id { get; set; }
		/// <summary>等级</summary>
		[ProtoMember(3)]
		public int Skill_Lv { get; set; }
		/// <summary>描述</summary>
		[ProtoMember(4)]
		public int Desc { get; set; }
		/// <summary>学习 等级</summary>
		[ProtoMember(5)]
		public int Learn_Lv { get; set; }
		/// <summary>学习 消耗</summary>
		[ProtoMember(6)]
		public string Cost { get; set; }
		/// <summary>释放消耗</summary>
		[ProtoMember(7)]
		public string Consume { get; set; }

	}
}
