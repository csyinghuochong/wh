using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDDroup_Group_SubCategory : ProtoObject, IMerge
    {
        public static LDDroup_Group_SubCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDDroup_Group_Sub> dict = new Dictionary<int, LDDroup_Group_Sub>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDDroup_Group_Sub> list = new List<LDDroup_Group_Sub>();
		
        public LDDroup_Group_SubCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDDroup_Group_SubCategory s = o as LDDroup_Group_SubCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDDroup_Group_Sub config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDDroup_Group_Sub)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDDroup_Group_Sub Get(int id)
        {
            this.dict.TryGetValue(id, out LDDroup_Group_Sub item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDDroup_Group_Sub)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDDroup_Group_Sub> GetAll()
        {
            return this.dict;
        }

        public LDDroup_Group_Sub GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDDroup_Group_Sub: ProtoObject, IConfig
	{
		/// <summary>子组ID</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>类型</summary>
		[ProtoMember(2)]
		public int Sub_Type1 { get; set; }
		/// <summary>ID</summary>
		[ProtoMember(3)]
		public int Sub_Id1 { get; set; }
		/// <summary>最小</summary>
		[ProtoMember(4)]
		public int Sub_Min1 { get; set; }
		/// <summary>最大</summary>
		[ProtoMember(5)]
		public int Sub_Max1 { get; set; }
		/// <summary>状态</summary>
		[ProtoMember(6)]
		public int Sub_Condition1 { get; set; }
		/// <summary>权重</summary>
		[ProtoMember(7)]
		public int Sub_Weight1 { get; set; }
		/// <summary>类型</summary>
		[ProtoMember(8)]
		public int Sub_Type2 { get; set; }
		/// <summary>ID</summary>
		[ProtoMember(9)]
		public int Sub_Id2 { get; set; }
		/// <summary>最小</summary>
		[ProtoMember(10)]
		public int Sub_Min2 { get; set; }
		/// <summary>最大</summary>
		[ProtoMember(11)]
		public int Sub_Max2 { get; set; }
		/// <summary>状态</summary>
		[ProtoMember(12)]
		public int Sub_Condition2 { get; set; }
		/// <summary>权重</summary>
		[ProtoMember(13)]
		public int Sub_Weight2 { get; set; }
		/// <summary>类型</summary>
		[ProtoMember(14)]
		public int Sub_Type3 { get; set; }
		/// <summary>ID</summary>
		[ProtoMember(15)]
		public int Sub_Id3 { get; set; }
		/// <summary>最小</summary>
		[ProtoMember(16)]
		public int Sub_Min3 { get; set; }
		/// <summary>最大</summary>
		[ProtoMember(17)]
		public int Sub_Max3 { get; set; }
		/// <summary>状态</summary>
		[ProtoMember(18)]
		public int Sub_Condition3 { get; set; }
		/// <summary>权重</summary>
		[ProtoMember(19)]
		public int Sub_Weight3 { get; set; }
		/// <summary>类型</summary>
		[ProtoMember(20)]
		public int Sub_Type4 { get; set; }
		/// <summary>ID</summary>
		[ProtoMember(21)]
		public int Sub_Id4 { get; set; }
		/// <summary>最小</summary>
		[ProtoMember(22)]
		public int Sub_Min4 { get; set; }
		/// <summary>最大</summary>
		[ProtoMember(23)]
		public int Sub_Max4 { get; set; }
		/// <summary>状态</summary>
		[ProtoMember(24)]
		public int Sub_Condition4 { get; set; }
		/// <summary>权重</summary>
		[ProtoMember(25)]
		public int Sub_Weight4 { get; set; }
		/// <summary>类型</summary>
		[ProtoMember(26)]
		public int Sub_Type5 { get; set; }
		/// <summary>ID</summary>
		[ProtoMember(27)]
		public int Sub_Id5 { get; set; }
		/// <summary>最小</summary>
		[ProtoMember(28)]
		public int Sub_Min5 { get; set; }
		/// <summary>最大</summary>
		[ProtoMember(29)]
		public int Sub_Max5 { get; set; }
		/// <summary>状态</summary>
		[ProtoMember(30)]
		public int Sub_Condition5 { get; set; }
		/// <summary>权重</summary>
		[ProtoMember(31)]
		public int Sub_Weight5 { get; set; }
		/// <summary>类型</summary>
		[ProtoMember(32)]
		public int Sub_Type6 { get; set; }
		/// <summary>ID</summary>
		[ProtoMember(33)]
		public int Sub_Id6 { get; set; }
		/// <summary>最小</summary>
		[ProtoMember(34)]
		public int Sub_Min6 { get; set; }
		/// <summary>最大</summary>
		[ProtoMember(35)]
		public int Sub_Max6 { get; set; }
		/// <summary>状态</summary>
		[ProtoMember(36)]
		public int Sub_Condition6 { get; set; }
		/// <summary>权重</summary>
		[ProtoMember(37)]
		public int Sub_Weight6 { get; set; }

	}
}
