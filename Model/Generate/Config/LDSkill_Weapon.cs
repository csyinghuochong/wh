using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDSkill_WeaponCategory : ProtoObject, IMerge
    {
        public static LDSkill_WeaponCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDSkill_Weapon> dict = new Dictionary<int, LDSkill_Weapon>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDSkill_Weapon> list = new List<LDSkill_Weapon>();
		
        public LDSkill_WeaponCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDSkill_WeaponCategory s = o as LDSkill_WeaponCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDSkill_Weapon config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDSkill_Weapon Get(int id)
        {
            this.dict.TryGetValue(id, out LDSkill_Weapon item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDSkill_Weapon)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDSkill_Weapon> GetAll()
        {
            return this.dict;
        }

        public LDSkill_Weapon GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDSkill_Weapon: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>武器1</summary>
		[ProtoMember(2)]
		public int Weapon_1 { get; set; }
		/// <summary>武器2</summary>
		[ProtoMember(3)]
		public int Weapon_2 { get; set; }
		/// <summary>武器3</summary>
		[ProtoMember(4)]
		public int Weapon_3 { get; set; }
		/// <summary>武器4</summary>
		[ProtoMember(5)]
		public int Weapon_4 { get; set; }
		/// <summary>武器5</summary>
		[ProtoMember(6)]
		public int Weapon_5 { get; set; }
		/// <summary>武器6</summary>
		[ProtoMember(7)]
		public int Weapon_6 { get; set; }
		/// <summary>武器11</summary>
		[ProtoMember(8)]
		public int Weapon_11 { get; set; }
		/// <summary>武器12</summary>
		[ProtoMember(9)]
		public int Weapon_12 { get; set; }
		/// <summary>武器13</summary>
		[ProtoMember(10)]
		public int Weapon_13 { get; set; }

	}
}
