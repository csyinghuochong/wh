using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDPetCategory : ProtoObject, IMerge
    {
        public static LDPetCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDPet> dict = new Dictionary<int, LDPet>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDPet> list = new List<LDPet>();
		
        public LDPetCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDPetCategory s = o as LDPetCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDPet config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDPet)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDPet Get(int id)
        {
            this.dict.TryGetValue(id, out LDPet item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDPet)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDPet> GetAll()
        {
            return this.dict;
        }

        public LDPet GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDPet: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(2)]
		public int Name { get; set; }
		/// <summary>头像</summary>
		[ProtoMember(3)]
		public string Icon { get; set; }
		/// <summary>模型</summary>
		[ProtoMember(4)]
		public string Model { get; set; }
		/// <summary>品质</summary>
		[ProtoMember(5)]
		public int Quality { get; set; }
		/// <summary>星级上限</summary>
		[ProtoMember(6)]
		public int Star_Limit { get; set; }
		/// <summary>生命资质</summary>
		[ProtoMember(7)]
		public int[] Aptitude_Hp { get; set; }
		/// <summary>物攻资质</summary>
		[ProtoMember(8)]
		public int[] Aptitude_Atk_P { get; set; }
		/// <summary>法攻资质</summary>
		[ProtoMember(9)]
		public int[] Aptitude_Atk_M { get; set; }
		/// <summary>物防资质</summary>
		[ProtoMember(10)]
		public int[] Aptitude_Def_P { get; set; }
		/// <summary>法防资质</summary>
		[ProtoMember(11)]
		public int[] Aptitude_Def_M { get; set; }
		/// <summary>变异提升</summary>
		[ProtoMember(12)]
		public int[] Aptitude_Add_Change { get; set; }
		/// <summary>星级提升上限 当前值按算法提升</summary>
		[ProtoMember(13)]
		public int[] Aptitude_Add_Star { get; set; }

	}
}
