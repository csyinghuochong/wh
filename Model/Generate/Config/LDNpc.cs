using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDNPCCategory : ProtoObject, IMerge
    {
        public static LDNPCCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDNPC> dict = new Dictionary<int, LDNPC>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDNPC> list = new List<LDNPC>();
		
        public LDNPCCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDNPCCategory s = o as LDNPCCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDNPC config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDNPC Get(int id)
        {
            this.dict.TryGetValue(id, out LDNPC item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDNPC)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDNPC> GetAll()
        {
            return this.dict;
        }

        public LDNPC GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDNPC: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(2)]
		public int Name { get; set; }
		/// <summary>头顶描述</summary>
		[ProtoMember(3)]
		public int Desc_Head { get; set; }
		/// <summary>对话描述</summary>
		[ProtoMember(4)]
		public int Desc_Dialogue { get; set; }
		/// <summary>头像</summary>
		[ProtoMember(5)]
		public string Icon { get; set; }
		/// <summary>模型</summary>
		[ProtoMember(6)]
		public string Model { get; set; }
		/// <summary>类型</summary>
		[ProtoMember(7)]
		public int Type { get; set; }
		/// <summary>参数</summary>
		[ProtoMember(8)]
		public int[] Param { get; set; }
		/// <summary>雷达 显示</summary>
		[ProtoMember(9)]
		public int Rader { get; set; }

	}
}
