using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class FunctionConfigCategory : ProtoObject, IMerge
    {
        public static FunctionConfigCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, FunctionConfig> dict = new Dictionary<int, FunctionConfig>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<FunctionConfig> list = new List<FunctionConfig>();
		
        public FunctionConfigCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            FunctionConfigCategory s = o as FunctionConfigCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (FunctionConfig config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public FunctionConfig Get(int id)
        {
            this.dict.TryGetValue(id, out FunctionConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (FunctionConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, FunctionConfig> GetAll()
        {
            return this.dict;
        }

        public FunctionConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class FunctionConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>UI</summary>
		[ProtoMember(2)]
		public string Name { get; set; }
		/// <summary>开启条件类型 1-等级 2-任务</summary>
		[ProtoMember(3)]
		public int[] ConditionType { get; set; }
		/// <summary>开启条件参数</summary>
		[ProtoMember(4)]
		public int[] ConditionParam { get; set; }
		/// <summary>开启时间</summary>
		[ProtoMember(5)]
		public string OpenTime { get; set; }
		/// <summary>开始时间 周1-周7</summary>
		[ProtoMember(6)]
		public int[] OpenDay { get; set; }
		/// <summary>是否开启</summary>
		[ProtoMember(7)]
		public string IfOpen { get; set; }
		/// <summary>刷怪场景</summary>
		[ProtoMember(8)]
		public int SceneId { get; set; }
		/// <summary>刷怪配置</summary>
		[ProtoMember(9)]
		public int[] CreateMonsterPosi { get; set; }

	}
}
