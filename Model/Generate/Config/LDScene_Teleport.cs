using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDScene_TeleportCategory : ProtoObject, IMerge
    {
        public static LDScene_TeleportCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDScene_Teleport> dict = new Dictionary<int, LDScene_Teleport>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDScene_Teleport> list = new List<LDScene_Teleport>();
		
        public LDScene_TeleportCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDScene_TeleportCategory s = o as LDScene_TeleportCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDScene_Teleport config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDScene_Teleport)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDScene_Teleport Get(int id)
        {
            this.dict.TryGetValue(id, out LDScene_Teleport item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDScene_Teleport)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDScene_Teleport> GetAll()
        {
            return this.dict;
        }

        public LDScene_Teleport GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDScene_Teleport: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>特殊名称 -1：读新场景名</summary>
		[ProtoMember(2)]
		public int Special_Name { get; set; }
		/// <summary>位置</summary>
		[ProtoMember(3)]
		public int[] Position { get; set; }
		/// <summary>新场景ID</summary>
		[ProtoMember(4)]
		public int Scene_Target { get; set; }
		/// <summary>目标位置</summary>
		[ProtoMember(5)]
		public int[] Pos_Target { get; set; }

	}
}
