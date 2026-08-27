using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDMount_ModelCategory : ProtoObject, IMerge
    {
        public static LDMount_ModelCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDMount_Model> dict = new Dictionary<int, LDMount_Model>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDMount_Model> list = new List<LDMount_Model>();
		
        public LDMount_ModelCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDMount_ModelCategory s = o as LDMount_ModelCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDMount_Model config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDMount_Model)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDMount_Model Get(int id)
        {
            this.dict.TryGetValue(id, out LDMount_Model item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDMount_Model)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDMount_Model> GetAll()
        {
            return this.dict;
        }

        public LDMount_Model GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDMount_Model: ProtoObject, IConfig
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
		/// <summary>速度</summary>
		[ProtoMember(7)]
		public int Speed { get; set; }

	}
}
