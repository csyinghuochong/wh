using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDTask_GroupCategory : ProtoObject, IMerge
    {
        public static LDTask_GroupCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDTask_Group> dict = new Dictionary<int, LDTask_Group>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDTask_Group> list = new List<LDTask_Group>();
		
        public LDTask_GroupCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDTask_GroupCategory s = o as LDTask_GroupCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDTask_Group config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDTask_Group)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDTask_Group Get(int id)
        {
            this.dict.TryGetValue(id, out LDTask_Group item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDTask_Group)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDTask_Group> GetAll()
        {
            return this.dict;
        }

        public LDTask_Group GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDTask_Group: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>隶属</summary>
		[ProtoMember(2)]
		public int Belong { get; set; }
		/// <summary>页码</summary>
		[ProtoMember(3)]
		public int Group { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(4)]
		public int Name { get; set; }
		/// <summary>资源</summary>
		[ProtoMember(5)]
		public string Resources { get; set; }
		/// <summary>类型 1-日 2-周 3-月</summary>
		[ProtoMember(6)]
		public int Type { get; set; }

	}
}
