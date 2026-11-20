using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDMailCategory : ProtoObject, IMerge
    {
        public static LDMailCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDMail> dict = new Dictionary<int, LDMail>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDMail> list = new List<LDMail>();
		
        public LDMailCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDMailCategory s = o as LDMailCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDMail config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDMail)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDMail Get(int id)
        {
            this.dict.TryGetValue(id, out LDMail item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDMail)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDMail> GetAll()
        {
            return this.dict;
        }

        public LDMail GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDMail: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>int</summary>
		[ProtoMember(2)]
		public string Key { get; set; }
		/// <summary>标题</summary>
		[ProtoMember(3)]
		public int Title { get; set; }
		/// <summary>发件人</summary>
		[ProtoMember(4)]
		public int Sender { get; set; }
		/// <summary>内容</summary>
		[ProtoMember(5)]
		public int Content { get; set; }
		/// <summary>隶属</summary>
		[ProtoMember(6)]
		public int Belong_Id { get; set; }

	}
}
