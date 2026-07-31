using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDDrop_GroupCategory : ProtoObject, IMerge
    {
        public static LDDrop_GroupCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDDrop_Group> dict = new Dictionary<int, LDDrop_Group>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDDrop_Group> list = new List<LDDrop_Group>();
		
        public LDDrop_GroupCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDDrop_GroupCategory s = o as LDDrop_GroupCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDDrop_Group config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDDrop_Group)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDDrop_Group Get(int id)
        {
            this.dict.TryGetValue(id, out LDDrop_Group item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDDrop_Group)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDDrop_Group> GetAll()
        {
            return this.dict;
        }

        public LDDrop_Group GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDDrop_Group: ProtoObject, IConfig
	{
		/// <summary>Group_Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>Sub1</summary>
		[ProtoMember(2)]
		public int Sub1 { get; set; }
		/// <summary>Weight1</summary>
		[ProtoMember(3)]
		public int Weight1 { get; set; }
		/// <summary>Sub2</summary>
		[ProtoMember(4)]
		public int Sub2 { get; set; }
		/// <summary>Weight2</summary>
		[ProtoMember(5)]
		public int Weight2 { get; set; }
		/// <summary>Sub3</summary>
		[ProtoMember(6)]
		public int Sub3 { get; set; }
		/// <summary>Weight3</summary>
		[ProtoMember(7)]
		public int Weight3 { get; set; }
		/// <summary>Sub4</summary>
		[ProtoMember(8)]
		public int Sub4 { get; set; }
		/// <summary>Weight4</summary>
		[ProtoMember(9)]
		public int Weight4 { get; set; }
		/// <summary>Sub5</summary>
		[ProtoMember(10)]
		public int Sub5 { get; set; }
		/// <summary>Weight5</summary>
		[ProtoMember(11)]
		public int Weight5 { get; set; }

	}
}
