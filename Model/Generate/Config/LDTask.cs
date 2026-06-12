using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDTaskCategory : ProtoObject, IMerge
    {
        public static LDTaskCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDTask> dict = new Dictionary<int, LDTask>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDTask> list = new List<LDTask>();
		
        public LDTaskCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDTaskCategory s = o as LDTaskCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDTask config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDTask Get(int id)
        {
            this.dict.TryGetValue(id, out LDTask item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDTask)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDTask> GetAll()
        {
            return this.dict;
        }

        public LDTask GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDTask: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>页码</summary>
		[ProtoMember(2)]
		public int Page { get; set; }
		/// <summary>排序</summary>
		[ProtoMember(3)]
		public int Order_SL { get; set; }
		/// <summary>条件类型</summary>
		[ProtoMember(4)]
		public int Condition_Type { get; set; }
		/// <summary>主参数</summary>
		[ProtoMember(5)]
		public int Param1 { get; set; }
		/// <summary>副参数</summary>
		[ProtoMember(6)]
		public int Param2 { get; set; }
		/// <summary>文本替换 非0生效</summary>
		[ProtoMember(7)]
		public int Special_Word { get; set; }
		/// <summary>奖励</summary>
		[ProtoMember(8)]
		public string Reward { get; set; }

	}
}
