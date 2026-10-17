using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDPet_StarCategory : ProtoObject, IMerge
    {
        public static LDPet_StarCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDPet_Star> dict = new Dictionary<int, LDPet_Star>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDPet_Star> list = new List<LDPet_Star>();
		
        public LDPet_StarCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDPet_StarCategory s = o as LDPet_StarCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDPet_Star config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDPet_Star)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDPet_Star Get(int id)
        {
            this.dict.TryGetValue(id, out LDPet_Star item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDPet_Star)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDPet_Star> GetAll()
        {
            return this.dict;
        }

        public LDPet_Star GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDPet_Star: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>神兽转换</summary>
		[ProtoMember(2)]
		public string Best_Convert { get; set; }

	}
}
