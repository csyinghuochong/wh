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
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDFashion)} Id={config.Id}");
				}
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
		/// <summary>描述</summary>
		[ProtoMember(3)]
		public int Desc { get; set; }
		/// <summary>品质</summary>
		[ProtoMember(4)]
		public int Quality { get; set; }
		/// <summary>时装模型</summary>
		[ProtoMember(5)]
		public string Model { get; set; }
		/// <summary>部位</summary>
		[ProtoMember(6)]
		public int Part { get; set; }
		/// <summary>基础职业</summary>
		[ProtoMember(7)]
		public int[] Occupation { get; set; }
		/// <summary>获取类型 1-钻石 2-金币 3-券   4-稀有券  5-来源 9-直购</summary>
		[ProtoMember(8)]
		public int Get_Type { get; set; }
		/// <summary>获取值</summary>
		[ProtoMember(9)]
		public int Get_Value { get; set; }
		/// <summary>获取简述</summary>
		[ProtoMember(10)]
		public int Get_Desc_Short { get; set; }
		/// <summary>获取描述</summary>
		[ProtoMember(11)]
		public int Get_Desc { get; set; }
		/// <summary>顺序</summary>
		[ProtoMember(12)]
		public int Order_SL { get; set; }
		/// <summary>XX 值</summary>
		[ProtoMember(13)]
		public int Fashion_Value { get; set; }
		/// <summary>摄像机</summary>
		[ProtoMember(14)]
		public double[] Camera { get; set; }
		/// <summary>禁用</summary>
		[ProtoMember(15)]
		public int Is_Close { get; set; }

	}
}
