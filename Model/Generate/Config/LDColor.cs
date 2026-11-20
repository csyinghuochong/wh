using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDColorCategory : ProtoObject, IMerge
    {
        public static LDColorCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDColor> dict = new Dictionary<int, LDColor>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDColor> list = new List<LDColor>();
		
        public LDColorCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDColorCategory s = o as LDColorCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDColor config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDColor)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDColor Get(int id)
        {
            this.dict.TryGetValue(id, out LDColor item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDColor)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDColor> GetAll()
        {
            return this.dict;
        }

        public LDColor GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDColor: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>颜色</summary>
		[ProtoMember(2)]
		public string Color { get; set; }

	}
}
