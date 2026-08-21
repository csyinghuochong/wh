using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDSkill_MakeCategory : ProtoObject, IMerge
    {
        public static LDSkill_MakeCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDSkill_Make> dict = new Dictionary<int, LDSkill_Make>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDSkill_Make> list = new List<LDSkill_Make>();
		
        public LDSkill_MakeCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDSkill_MakeCategory s = o as LDSkill_MakeCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDSkill_Make config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDSkill_Make)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDSkill_Make Get(int id)
        {
            this.dict.TryGetValue(id, out LDSkill_Make item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDSkill_Make)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDSkill_Make> GetAll()
        {
            return this.dict;
        }

        public LDSkill_Make GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDSkill_Make: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>组</summary>
		[ProtoMember(2)]
		public int Group { get; set; }
		/// <summary>制造 时间</summary>
		[ProtoMember(3)]
		public int Make_Time { get; set; }
		/// <summary>类型</summary>
		[ProtoMember(4)]
		public int Make_Type_1 { get; set; }
		/// <summary>Id</summary>
		[ProtoMember(5)]
		public int Make_Id_1 { get; set; }
		/// <summary>数量</summary>
		[ProtoMember(6)]
		public int Make_Num_1 { get; set; }
		/// <summary>权重</summary>
		[ProtoMember(7)]
		public int Make_Weight_1 { get; set; }
		/// <summary>类型</summary>
		[ProtoMember(8)]
		public int Make_Type_2 { get; set; }
		/// <summary>Id</summary>
		[ProtoMember(9)]
		public int Make_Id_2 { get; set; }
		/// <summary>数量</summary>
		[ProtoMember(10)]
		public int Make_Num_2 { get; set; }
		/// <summary>权重</summary>
		[ProtoMember(11)]
		public int Make_Weight_2 { get; set; }
		/// <summary>类型</summary>
		[ProtoMember(12)]
		public int Make_Type_3 { get; set; }
		/// <summary>Id</summary>
		[ProtoMember(13)]
		public int Make_Id_3 { get; set; }
		/// <summary>数量</summary>
		[ProtoMember(14)]
		public int Make_Num_3 { get; set; }
		/// <summary>权重</summary>
		[ProtoMember(15)]
		public int Make_Weight_3 { get; set; }
		/// <summary>类型</summary>
		[ProtoMember(16)]
		public int Make_Type_4 { get; set; }
		/// <summary>Id</summary>
		[ProtoMember(17)]
		public int Make_Id_4 { get; set; }
		/// <summary>数量</summary>
		[ProtoMember(18)]
		public int Make_Num_4 { get; set; }
		/// <summary>权重</summary>
		[ProtoMember(19)]
		public int Make_Weight_4 { get; set; }
		/// <summary>类型</summary>
		[ProtoMember(20)]
		public int Make_Type_5 { get; set; }
		/// <summary>Id</summary>
		[ProtoMember(21)]
		public int Make_Id_5 { get; set; }
		/// <summary>数量</summary>
		[ProtoMember(22)]
		public int Make_Num_5 { get; set; }
		/// <summary>权重</summary>
		[ProtoMember(23)]
		public int Make_Weight_5 { get; set; }
		/// <summary>消耗 活力</summary>
		[ProtoMember(24)]
		public int Consume_Item_12 { get; set; }
		/// <summary>消耗 金钱</summary>
		[ProtoMember(25)]
		public int Consume_Item_5 { get; set; }
		/// <summary>消耗</summary>
		[ProtoMember(26)]
		public string Consume { get; set; }

	}
}
