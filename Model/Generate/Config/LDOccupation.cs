using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDOccupationCategory : ProtoObject, IMerge
    {
        public static LDOccupationCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDOccupation> dict = new Dictionary<int, LDOccupation>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDOccupation> list = new List<LDOccupation>();
		
        public LDOccupationCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDOccupationCategory s = o as LDOccupationCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDOccupation config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDOccupation Get(int id)
        {
            this.dict.TryGetValue(id, out LDOccupation item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDOccupation)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDOccupation> GetAll()
        {
            return this.dict;
        }

        public LDOccupation GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDOccupation: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(2)]
		public int Name { get; set; }
		/// <summary>武器</summary>
		[ProtoMember(3)]
		public int[] Weapon_ { get; set; }
		/// <summary>护甲</summary>
		[ProtoMember(4)]
		public int[] Armor { get; set; }
		/// <summary>模型</summary>
		[ProtoMember(5)]
		public string ModelAsset { get; set; }
		/// <summary>支持换装</summary>
		[ProtoMember(6)]
		public int ChangeEquip { get; set; }
		/// <summary>初始血量</summary>
		[ProtoMember(7)]
		public long BaseHp { get; set; }
		/// <summary>初始攻击最小值</summary>
		[ProtoMember(8)]
		public long BaseMinAct { get; set; }
		/// <summary>初始攻击最大值</summary>
		[ProtoMember(9)]
		public long BaseMaxAct { get; set; }
		/// <summary>初始物防最小值</summary>
		[ProtoMember(10)]
		public long BaseMinDef { get; set; }
		/// <summary>初始物防最大值</summary>
		[ProtoMember(11)]
		public long BaseMaxDef { get; set; }
		/// <summary>初始魔防最小值</summary>
		[ProtoMember(12)]
		public long BaseMinAdf { get; set; }
		/// <summary>初始魔防最大值</summary>
		[ProtoMember(13)]
		public long BaseMaxAdf { get; set; }
		/// <summary>初始移动速度</summary>
		[ProtoMember(14)]
		public double BaseMoveSpeed { get; set; }
		/// <summary>初始暴击值</summary>
		[ProtoMember(15)]
		public double BaseCri { get; set; }
		/// <summary>初始命中值</summary>
		[ProtoMember(16)]
		public double BaseHit { get; set; }
		/// <summary>初始闪避值</summary>
		[ProtoMember(17)]
		public double BaseDodge { get; set; }
		/// <summary>初始物理免伤</summary>
		[ProtoMember(18)]
		public double BaseDefAdd { get; set; }
		/// <summary>初始魔法免伤</summary>
		[ProtoMember(19)]
		public double BaseAdfAdd { get; set; }
		/// <summary>初始免伤</summary>
		[ProtoMember(20)]
		public double DamgeAdd { get; set; }
		/// <summary>每级血量成长</summary>
		[ProtoMember(21)]
		public long LvUpHp { get; set; }
		/// <summary>每级攻击最小级成长</summary>
		[ProtoMember(22)]
		public long LvUpMinAct { get; set; }
		/// <summary>每级攻击最大值成长</summary>
		[ProtoMember(23)]
		public long LvUpMaxAct { get; set; }
		/// <summary>每级攻击最小级成长</summary>
		[ProtoMember(24)]
		public long LvUpMinMagAct { get; set; }
		/// <summary>每级攻击最大值成长</summary>
		[ProtoMember(25)]
		public long LvUpMaxMagAct { get; set; }
		/// <summary>初始物防最小值</summary>
		[ProtoMember(26)]
		public long LvUpMinDef { get; set; }
		/// <summary>初始物防最大值</summary>
		[ProtoMember(27)]
		public long LvUpMaxDef { get; set; }
		/// <summary>初始魔防最小值</summary>
		[ProtoMember(28)]
		public long LvUpMinAdf { get; set; }
		/// <summary>初始魔防最大值</summary>
		[ProtoMember(29)]
		public long LvUpMaxAdf { get; set; }
		/// <summary>初始化普通攻击</summary>
		[ProtoMember(30)]
		public int InitActSkillID { get; set; }
		/// <summary>初始化技能ID</summary>
		[ProtoMember(31)]
		public int[] InitSkillID { get; set; }
		/// <summary>转职ID</summary>
		[ProtoMember(32)]
		public int[] OccTwoID { get; set; }
		/// <summary>时装部件</summary>
		[ProtoMember(33)]
		public int[] FashionBase { get; set; }
		/// <summary>基础技能</summary>
		[ProtoMember(34)]
		public int[] BaseSkill { get; set; }

	}
}
