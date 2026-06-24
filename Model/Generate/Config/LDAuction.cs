using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDAuctionCategory : ProtoObject, IMerge
    {
        public static LDAuctionCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDAuction> dict = new Dictionary<int, LDAuction>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDAuction> list = new List<LDAuction>();
		
        public LDAuctionCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDAuctionCategory s = o as LDAuctionCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDAuction config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDAuction Get(int id)
        {
            this.dict.TryGetValue(id, out LDAuction item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDAuction)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDAuction> GetAll()
        {
            return this.dict;
        }

        public LDAuction GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDAuction: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }

	}
}
