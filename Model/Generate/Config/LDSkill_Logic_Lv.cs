using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDSkill_Logic_LvCategory : ProtoObject, IMerge
    {
        public static LDSkill_Logic_LvCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDSkill_Logic_Lv> dict = new Dictionary<int, LDSkill_Logic_Lv>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDSkill_Logic_Lv> list = new List<LDSkill_Logic_Lv>();
		
        public LDSkill_Logic_LvCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDSkill_Logic_LvCategory s = o as LDSkill_Logic_LvCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDSkill_Logic_Lv config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDSkill_Logic_Lv Get(int id)
        {
            this.dict.TryGetValue(id, out LDSkill_Logic_Lv item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDSkill_Logic_Lv)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDSkill_Logic_Lv> GetAll()
        {
            return this.dict;
        }

        public LDSkill_Logic_Lv GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDSkill_Logic_Lv: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>逻辑ID</summary>
		[ProtoMember(2)]
		public int Logic_Id { get; set; }
		/// <summary>技能Lv</summary>
		[ProtoMember(3)]
		public int Skill_Lv { get; set; }
		/// <summary>参数1</summary>
		[ProtoMember(4)]
		public int Logic_Lv_Param1 { get; set; }
		/// <summary>参数2</summary>
		[ProtoMember(5)]
		public int Logic_Lv_Param2 { get; set; }
		/// <summary>参数3</summary>
		[ProtoMember(6)]
		public int Logic_Lv_Param3 { get; set; }
		/// <summary>参数4</summary>
		[ProtoMember(7)]
		public int Logic_Lv_Param4 { get; set; }
		/// <summary>参数5</summary>
		[ProtoMember(8)]
		public int Logic_Lv_Param5 { get; set; }
		/// <summary>参数6</summary>
		[ProtoMember(9)]
		public int Logic_Lv_Param6 { get; set; }
		/// <summary>参数7</summary>
		[ProtoMember(10)]
		public int Logic_Lv_Param7 { get; set; }
		/// <summary>参数8</summary>
		[ProtoMember(11)]
		public int Logic_Lv_Param8 { get; set; }
		/// <summary>参数9</summary>
		[ProtoMember(12)]
		public int Logic_Lv_Param9 { get; set; }
		/// <summary>参数10</summary>
		[ProtoMember(13)]
		public int Logic_Lv_Param10 { get; set; }

	}
}
