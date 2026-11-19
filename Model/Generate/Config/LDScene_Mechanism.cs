using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDScene_MechanismCategory : ProtoObject, IMerge
    {
        public static LDScene_MechanismCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDScene_Mechanism> dict = new Dictionary<int, LDScene_Mechanism>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDScene_Mechanism> list = new List<LDScene_Mechanism>();
		
        public LDScene_MechanismCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDScene_MechanismCategory s = o as LDScene_MechanismCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDScene_Mechanism config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDScene_Mechanism)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDScene_Mechanism Get(int id)
        {
            this.dict.TryGetValue(id, out LDScene_Mechanism item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDScene_Mechanism)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDScene_Mechanism> GetAll()
        {
            return this.dict;
        }

        public LDScene_Mechanism GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDScene_Mechanism: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>场景Id</summary>
		[ProtoMember(2)]
		public int Scene_Id { get; set; }
		/// <summary>类型 1：光墙</summary>
		[ProtoMember(3)]
		public int Type { get; set; }
		/// <summary>对应ID</summary>
		[ProtoMember(4)]
		public int Match_Id { get; set; }
		/// <summary>默认显示</summary>
		[ProtoMember(5)]
		public int Default_Display { get; set; }
		/// <summary>显示类型1 1=怪物进入战斗 2=怪物离开战斗 3=怪物击杀完 4=怪物未击杀完</summary>
		[ProtoMember(6)]
		public int Display_Request1 { get; set; }
		/// <summary>显示参数1 怪物进入/离开战斗=BOSSID 怪物未击杀/击杀完=怪物组ID</summary>
		[ProtoMember(7)]
		public int Display_Request_Param1 { get; set; }
		/// <summary>显示类型2 1=怪物进入战斗 2=怪物离开战斗 3=怪物击杀完 4=怪物未击杀完</summary>
		[ProtoMember(8)]
		public int Display_Request2 { get; set; }
		/// <summary>显示参数2 怪物进入/离开战斗=BOSSID 怪物未击杀/击杀完=怪物组ID</summary>
		[ProtoMember(9)]
		public int Display_Request_Param2 { get; set; }

	}
}
