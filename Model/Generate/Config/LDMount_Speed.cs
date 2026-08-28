using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDMount_SpeedCategory : ProtoObject, IMerge
    {
        public static LDMount_SpeedCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDMount_Speed> dict = new Dictionary<int, LDMount_Speed>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDMount_Speed> list = new List<LDMount_Speed>();
		
        public LDMount_SpeedCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDMount_SpeedCategory s = o as LDMount_SpeedCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDMount_Speed config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDMount_Speed)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDMount_Speed Get(int id)
        {
            this.dict.TryGetValue(id, out LDMount_Speed item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDMount_Speed)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDMount_Speed> GetAll()
        {
            return this.dict;
        }

        public LDMount_Speed GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDMount_Speed: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>最小等级</summary>
		[ProtoMember(2)]
		public int Lv_Min { get; set; }
		/// <summary>最大等级</summary>
		[ProtoMember(3)]
		public int Lv_Max { get; set; }
		/// <summary>速度</summary>
		[ProtoMember(4)]
		public int Speed { get; set; }

	}
}
