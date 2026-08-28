using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDMount_StageCategory : ProtoObject, IMerge
    {
        public static LDMount_StageCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDMount_Stage> dict = new Dictionary<int, LDMount_Stage>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDMount_Stage> list = new List<LDMount_Stage>();
		
        public LDMount_StageCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDMount_StageCategory s = o as LDMount_StageCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDMount_Stage config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDMount_Stage)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDMount_Stage Get(int id)
        {
            this.dict.TryGetValue(id, out LDMount_Stage item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDMount_Stage)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDMount_Stage> GetAll()
        {
            return this.dict;
        }

        public LDMount_Stage GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDMount_Stage: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>坐骑ID</summary>
		[ProtoMember(2)]
		public int Mount_Id { get; set; }
		/// <summary>最小等级</summary>
		[ProtoMember(3)]
		public int Lv_Min { get; set; }
		/// <summary>最大等级</summary>
		[ProtoMember(4)]
		public int Lv_Max { get; set; }
		/// <summary>图标</summary>
		[ProtoMember(5)]
		public string Icon { get; set; }
		/// <summary>模型</summary>
		[ProtoMember(6)]
		public string Model { get; set; }

	}
}
