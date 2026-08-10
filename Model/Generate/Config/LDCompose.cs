using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDComposeCategory : ProtoObject, IMerge
    {
        public static LDComposeCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDCompose> dict = new Dictionary<int, LDCompose>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDCompose> list = new List<LDCompose>();
		
        public LDComposeCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDComposeCategory s = o as LDComposeCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDCompose config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDCompose)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDCompose Get(int id)
        {
            this.dict.TryGetValue(id, out LDCompose item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDCompose)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDCompose> GetAll()
        {
            return this.dict;
        }

        public LDCompose GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDCompose: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>对应 隶属</summary>
		[ProtoMember(2)]
		public int Belong_Id { get; set; }

	}
}
