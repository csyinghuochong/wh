using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDFashionCategory : ProtoObject, IMerge
    {
        public static LDFashionCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDFashion> dict = new Dictionary<int, LDFashion>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDFashion> list = new List<LDFashion>();
		
        public LDFashionCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDFashionCategory s = o as LDFashionCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDFashion config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDFashion Get(int id)
        {
            this.dict.TryGetValue(id, out LDFashion item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDFashion)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDFashion> GetAll()
        {
            return this.dict;
        }

        public LDFashion GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDFashion: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(2)]
		public int Name { get; set; }
		/// <summary>属性描述</summary>
		[ProtoMember(3)]
		public int Desc_Att { get; set; }
		/// <summary>时装模型</summary>
		[ProtoMember(4)]
		public string Model { get; set; }
		/// <summary>职业</summary>
		[ProtoMember(5)]
		public int[] Occ { get; set; }
		/// <summary>时装部位 1-头 2-脸 3-身体</summary>
		[ProtoMember(6)]
		public int Position { get; set; }
		/// <summary>时装子类</summary>
		[ProtoMember(7)]
		public int SubType { get; set; }
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

	}
}
