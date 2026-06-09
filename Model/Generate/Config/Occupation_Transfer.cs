using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class Occupation_TransferCategory : ProtoObject, IMerge
    {
        public static Occupation_TransferCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, Occupation_Transfer> dict = new Dictionary<int, Occupation_Transfer>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<Occupation_Transfer> list = new List<Occupation_Transfer>();
		
        public Occupation_TransferCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            Occupation_TransferCategory s = o as Occupation_TransferCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (Occupation_Transfer config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public Occupation_Transfer Get(int id)
        {
            this.dict.TryGetValue(id, out Occupation_Transfer item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (Occupation_Transfer)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, Occupation_Transfer> GetAll()
        {
            return this.dict;
        }

        public Occupation_Transfer GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class Occupation_Transfer: ProtoObject, IConfig
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
		/// <summary>初始化技能ID</summary>
		[ProtoMember(4)]
		public int[] SkillID { get; set; }
		/// <summary>天赋1级</summary>
		[ProtoMember(5)]
		public int[] Talent { get; set; }
		/// <summary>转职显示技能</summary>
		[ProtoMember(6)]
		public int[] ShowTalentSkill { get; set; }
		/// <summary>职业能力</summary>
		[ProtoMember(7)]
		public int[] Capacitys { get; set; }
		/// <summary>武器类型</summary>
		[ProtoMember(8)]
		public int WeaponType { get; set; }
		/// <summary>护甲专精</summary>
		[ProtoMember(9)]
		public int ArmorMastery { get; set; }
		/// <summary>转职显示被动技能</summary>
		[ProtoMember(10)]
		public int[] ShowPassiveSkill { get; set; }
		/// <summary>觉醒技能</summary>
		[ProtoMember(11)]
		public int[] JueXingSkill { get; set; }

	}
}
