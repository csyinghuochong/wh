using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class MountCategory : ProtoObject, IMerge
    {
        public static MountCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, Mount> dict = new Dictionary<int, Mount>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<Mount> list = new List<Mount>();
		
        public MountCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            MountCategory s = o as MountCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (Mount config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public Mount Get(int id)
        {
            this.dict.TryGetValue(id, out Mount item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (Mount)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, Mount> GetAll()
        {
            return this.dict;
        }

        public Mount GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class Mount: ProtoObject, IConfig
	{
		/// <summary>ID</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(2)]
		public int Name { get; set; }
		/// <summary>描述</summary>
		[ProtoMember(3)]
		public int Desc { get; set; }
		/// <summary>获取描述</summary>
		[ProtoMember(4)]
		public int Desc_Get { get; set; }
		/// <summary>模型</summary>
		[ProtoMember(5)]
		public string ModelID { get; set; }
		/// <summary>品质</summary>
		[ProtoMember(6)]
		public int Quality { get; set; }
		/// <summary>额外属性</summary>
		[ProtoMember(7)]
		public string AddProperty { get; set; }
		/// <summary>对应骑乘Buff</summary>
		[ProtoMember(8)]
		public int MoveBuffID { get; set; }
		/// <summary>拖尾特效</summary>
		[ProtoMember(9)]
		public string TuoWeiEffectID { get; set; }
		/// <summary>启用</summary>
		[ProtoMember(10)]
		public int Enable { get; set; }

	}
}
