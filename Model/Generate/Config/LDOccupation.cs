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
		/// <summary>头像</summary>
		[ProtoMember(3)]
		public string HeadIcon { get; set; }
		/// <summary>模型</summary>
		[ProtoMember(4)]
		public string Model { get; set; }
		/// <summary>生命系数</summary>
		[ProtoMember(5)]
		public double Hp_Param { get; set; }
		/// <summary>速度</summary>
		[ProtoMember(6)]
		public int Speed { get; set; }
		/// <summary>默认普攻</summary>
		[ProtoMember(7)]
		public int Skill_Normal_Default { get; set; }
		/// <summary>技能</summary>
		[ProtoMember(8)]
		public int[] Skill { get; set; }
		/// <summary>默认加点</summary>
		[ProtoMember(9)]
		public int[] Add_Point_Default { get; set; }
		/// <summary>转职ID</summary>
		[ProtoMember(10)]
		public int[] TransferId { get; set; }
		/// <summary>初始装备</summary>
		[ProtoMember(11)]
		public int[] Equip_Init { get; set; }

	}
}
