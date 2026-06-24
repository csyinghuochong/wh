using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDTitleCategory : ProtoObject, IMerge
    {
        public static LDTitleCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDTitle> dict = new Dictionary<int, LDTitle>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDTitle> list = new List<LDTitle>();
		
        public LDTitleCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDTitleCategory s = o as LDTitleCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDTitle config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDTitle Get(int id)
        {
            this.dict.TryGetValue(id, out LDTitle item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDTitle)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDTitle> GetAll()
        {
            return this.dict;
        }

        public LDTitle GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDTitle: ProtoObject, IConfig
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
		/// <summary>获取描述</summary>
		[ProtoMember(4)]
		public int Desc_Get { get; set; }
		/// <summary>图标</summary>
		[ProtoMember(5)]
		public string Icon { get; set; }
		/// <summary>属性</summary>
		[ProtoMember(6)]
		public string Attribute { get; set; }
		/// <summary>有效期 单位：秒</summary>
		[ProtoMember(7)]
		public int ValidityTime { get; set; }
		/// <summary>序列帧 动画</summary>
		[ProtoMember(8)]
		public string AnimatorAsset { get; set; }
		/// <summary>序列帧 动画数量</summary>
		[ProtoMember(9)]
		public int AnimatorNumber { get; set; }
		/// <summary>缩放</summary>
		[ProtoMember(10)]
		public double size { get; set; }
		/// <summary>X偏移</summary>
		[ProtoMember(11)]
		public double MoveX { get; set; }
		/// <summary>Y偏移</summary>
		[ProtoMember(12)]
		public double MoveY { get; set; }
		/// <summary>启用</summary>
		[ProtoMember(13)]
		public int Enable { get; set; }

	}
}
