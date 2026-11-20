using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDInteractionCategory : ProtoObject, IMerge
    {
        public static LDInteractionCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDInteraction> dict = new Dictionary<int, LDInteraction>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDInteraction> list = new List<LDInteraction>();
		
        public LDInteractionCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDInteractionCategory s = o as LDInteractionCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDInteraction config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDInteraction)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDInteraction Get(int id)
        {
            this.dict.TryGetValue(id, out LDInteraction item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDInteraction)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDInteraction> GetAll()
        {
            return this.dict;
        }

        public LDInteraction GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDInteraction: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(2)]
		public int Name { get; set; }
		/// <summary>模型</summary>
		[ProtoMember(3)]
		public string Model { get; set; }
		/// <summary>交互距离</summary>
		[ProtoMember(4)]
		public double Distance { get; set; }

	}
}
