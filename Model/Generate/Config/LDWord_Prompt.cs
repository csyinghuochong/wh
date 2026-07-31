using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDWord_PromptCategory : ProtoObject, IMerge
    {
        public static LDWord_PromptCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDWord_Prompt> dict = new Dictionary<int, LDWord_Prompt>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDWord_Prompt> list = new List<LDWord_Prompt>();
		
        public LDWord_PromptCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDWord_PromptCategory s = o as LDWord_PromptCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDWord_Prompt config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDWord_Prompt)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDWord_Prompt Get(int id)
        {
            this.dict.TryGetValue(id, out LDWord_Prompt item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDWord_Prompt)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDWord_Prompt> GetAll()
        {
            return this.dict;
        }

        public LDWord_Prompt GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDWord_Prompt: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>int</summary>
		[ProtoMember(2)]
		public string Key { get; set; }
		/// <summary>中文</summary>
		[ProtoMember(3)]
		public string CN { get; set; }

	}
}
