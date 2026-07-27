using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDFundCategory : ProtoObject, IMerge
    {
        public static LDFundCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDFund> dict = new Dictionary<int, LDFund>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDFund> list = new List<LDFund>();
		
        public LDFundCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDFundCategory s = o as LDFundCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDFund config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDFund Get(int id)
        {
            this.dict.TryGetValue(id, out LDFund item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDFund)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDFund> GetAll()
        {
            return this.dict;
        }

        public LDFund GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDFund: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }

	}
}
