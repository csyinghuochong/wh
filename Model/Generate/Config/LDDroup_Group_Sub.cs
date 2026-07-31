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
		/// <summary>Sub_Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>Sub_Type1</summary>
		[ProtoMember(2)]
		public int Sub_Type1 { get; set; }
		/// <summary>Sub_Id1</summary>
		[ProtoMember(3)]
		public int Sub_Id1 { get; set; }
		/// <summary>Sub_Min1</summary>
		[ProtoMember(4)]
		public int Sub_Min1 { get; set; }
		/// <summary>Sub_Max1</summary>
		[ProtoMember(5)]
		public int Sub_Max1 { get; set; }
		/// <summary>Sub_Weight</summary>
		[ProtoMember(6)]
		public int Sub_Weight { get; set; }
		/// <summary>Sub_Type1</summary>
		[ProtoMember(7)]
		public int Sub_Type2 { get; set; }
		/// <summary>Sub_Id1</summary>
		[ProtoMember(8)]
		public int Sub_Id2 { get; set; }
		/// <summary>Sub_Min1</summary>
		[ProtoMember(9)]
		public int Sub_Min2 { get; set; }
		/// <summary>Sub_Max1</summary>
		[ProtoMember(10)]
		public int Sub_Max2 { get; set; }
		/// <summary>Sub_Weight</summary>
		[ProtoMember(11)]
		public int Sub_Weight2 { get; set; }

	}
}
