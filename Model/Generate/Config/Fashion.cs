using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class FashionCategory : ProtoObject, IMerge
    {
        public static FashionCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, Fashion> dict = new Dictionary<int, Fashion>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<Fashion> list = new List<Fashion>();
		
        public FashionCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            FashionCategory s = o as FashionCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (Fashion config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public Fashion Get(int id)
        {
            this.dict.TryGetValue(id, out Fashion item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (Fashion)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, Fashion> GetAll()
        {
            return this.dict;
        }

        public Fashion GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class Fashion: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>时装名字</summary>
		[ProtoMember(2)]
		public string Name { get; set; }
		/// <summary>职业</summary>
		[ProtoMember(3)]
		public int[] Occ { get; set; }
		/// <summary>时装部位 1-头 2-脸 3-身体</summary>
		[ProtoMember(4)]
		public int Position { get; set; }
		/// <summary>时装子类</summary>
		[ProtoMember(5)]
		public int SubType { get; set; }
		/// <summary>时装名字</summary>
		[ProtoMember(6)]
		public string Name_EN { get; set; }
		/// <summary>时装模型</summary>
		[ProtoMember(7)]
		public string Model { get; set; }
		/// <summary>激活条件</summary>
		[ProtoMember(8)]
		public string ActiveCost { get; set; }
		/// <summary>时装属性加成Key</summary>
		[ProtoMember(9)]
		public int[] PropertyKey { get; set; }
		/// <summary>时装属性加成Value</summary>
		[ProtoMember(10)]
		public long[] PropertyValue { get; set; }
		/// <summary>摄像机参数</summary>
		[ProtoMember(11)]
		public double[] Camera { get; set; }
		/// <summary>时装描述</summary>
		[ProtoMember(12)]
		public string PropertyDes { get; set; }
		/// <summary>时装描述</summary>
		[ProtoMember(13)]
		public string PropertyDes_EN { get; set; }

	}
}
