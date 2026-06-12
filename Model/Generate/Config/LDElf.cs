using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDElfCategory : ProtoObject, IMerge
    {
        public static LDElfCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDElf> dict = new Dictionary<int, LDElf>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDElf> list = new List<LDElf>();
		
        public LDElfCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDElfCategory s = o as LDElfCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDElf config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDElf Get(int id)
        {
            this.dict.TryGetValue(id, out LDElf item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDElf)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDElf> GetAll()
        {
            return this.dict;
        }

        public LDElf GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDElf: ProtoObject, IConfig
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
		/// <summary>能力描述</summary>
		[ProtoMember(4)]
		public int Desc_Ability { get; set; }
		/// <summary>获取描述</summary>
		[ProtoMember(5)]
		public int Desc_Get { get; set; }
		/// <summary>图标显示</summary>
		[ProtoMember(6)]
		public string Icon { get; set; }
		/// <summary>模型</summary>
		[ProtoMember(7)]
		public string Assets { get; set; }
		/// <summary>额外属性</summary>
		[ProtoMember(8)]
		public string AddProperty { get; set; }
		/// <summary>序列帧动画</summary>
		[ProtoMember(9)]
		public string AnimatorAsset { get; set; }
		/// <summary>序列帧动画数量</summary>
		[ProtoMember(10)]
		public int AnimatorNumber { get; set; }
		/// <summary>有效期(秒)</summary>
		[ProtoMember(11)]
		public int ValidityTime { get; set; }
		/// <summary>缩放大小</summary>
		[ProtoMember(12)]
		public double size { get; set; }
		/// <summary>X偏移</summary>
		[ProtoMember(13)]
		public double MoveX { get; set; }
		/// <summary>Y便宜</summary>
		[ProtoMember(14)]
		public double MoveY { get; set; }
		/// <summary>功能类型</summary>
		[ProtoMember(15)]
		public int FunctionType { get; set; }
		/// <summary>功能参数</summary>
		[ProtoMember(16)]
		public string FunctionValue { get; set; }
		/// <summary>自动拾取</summary>
		[ProtoMember(17)]
		public int AutoPick { get; set; }

	}
}
