using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDOccupation_TransferCategory : ProtoObject, IMerge
    {
        public static LDOccupation_TransferCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDOccupation_Transfer> dict = new Dictionary<int, LDOccupation_Transfer>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDOccupation_Transfer> list = new List<LDOccupation_Transfer>();
		
        public LDOccupation_TransferCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDOccupation_TransferCategory s = o as LDOccupation_TransferCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDOccupation_Transfer config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDOccupation_Transfer)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDOccupation_Transfer Get(int id)
        {
            this.dict.TryGetValue(id, out LDOccupation_Transfer item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDOccupation_Transfer)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDOccupation_Transfer> GetAll()
        {
            return this.dict;
        }

        public LDOccupation_Transfer GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDOccupation_Transfer: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(2)]
		public int Name { get; set; }
		/// <summary>描述</summary>
		[ProtoMember(3)]
		public int Desc { get; set; }
		/// <summary>技能</summary>
		[ProtoMember(4)]
		public int[] Skill { get; set; }

	}
}
