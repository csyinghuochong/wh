using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDDropCategory : ProtoObject, IMerge
    {
        public static LDDropCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDDrop> dict = new Dictionary<int, LDDrop>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDDrop> list = new List<LDDrop>();
		
        public LDDropCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDDropCategory s = o as LDDropCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDDrop config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDDrop Get(int id)
        {
            this.dict.TryGetValue(id, out LDDrop item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDDrop)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDDrop> GetAll()
        {
            return this.dict;
        }

        public LDDrop GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDDrop: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>类型 0-A 1-B 共存/互斥</summary>
		[ProtoMember(2)]
		public int Type { get; set; }
		/// <summary>组</summary>
		[ProtoMember(3)]
		public int Group1 { get; set; }
		/// <summary>值</summary>
		[ProtoMember(4)]
		public int Group_Value1 { get; set; }
		/// <summary>备注</summary>
		[ProtoMember(5)]
		public string Note1 { get; set; }
		/// <summary>组</summary>
		[ProtoMember(6)]
		public int Group2 { get; set; }
		/// <summary>值</summary>
		[ProtoMember(7)]
		public int Group_Value2 { get; set; }
		/// <summary>备注</summary>
		[ProtoMember(8)]
		public string Note2 { get; set; }
		/// <summary>组</summary>
		[ProtoMember(9)]
		public int Group3 { get; set; }
		/// <summary>值</summary>
		[ProtoMember(10)]
		public int Group_Value3 { get; set; }
		/// <summary>备注</summary>
		[ProtoMember(11)]
		public string Note3 { get; set; }
		/// <summary>组</summary>
		[ProtoMember(12)]
		public int Group4 { get; set; }
		/// <summary>值</summary>
		[ProtoMember(13)]
		public int Group_Value4 { get; set; }
		/// <summary>备注</summary>
		[ProtoMember(14)]
		public string Note4 { get; set; }
		/// <summary>组</summary>
		[ProtoMember(15)]
		public int Group5 { get; set; }
		/// <summary>值</summary>
		[ProtoMember(16)]
		public int Group_Value5 { get; set; }
		/// <summary>备注</summary>
		[ProtoMember(17)]
		public string Note5 { get; set; }
		/// <summary>组</summary>
		[ProtoMember(18)]
		public int Group6 { get; set; }
		/// <summary>值</summary>
		[ProtoMember(19)]
		public int Group_Value6 { get; set; }
		/// <summary>备注</summary>
		[ProtoMember(20)]
		public string Note6 { get; set; }
		/// <summary>组</summary>
		[ProtoMember(21)]
		public int Group7 { get; set; }
		/// <summary>值</summary>
		[ProtoMember(22)]
		public int Group_Value7 { get; set; }
		/// <summary>备注</summary>
		[ProtoMember(23)]
		public string Note7 { get; set; }
		/// <summary>组</summary>
		[ProtoMember(24)]
		public int Group8 { get; set; }
		/// <summary>值</summary>
		[ProtoMember(25)]
		public int Group_Value8 { get; set; }
		/// <summary>备注</summary>
		[ProtoMember(26)]
		public string Note8 { get; set; }

	}
}
