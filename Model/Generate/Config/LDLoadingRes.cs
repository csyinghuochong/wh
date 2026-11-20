using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDLoadingResCategory : ProtoObject, IMerge
    {
        public static LDLoadingResCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDLoadingRes> dict = new Dictionary<int, LDLoadingRes>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDLoadingRes> list = new List<LDLoadingRes>();
		
        public LDLoadingResCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDLoadingResCategory s = o as LDLoadingResCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDLoadingRes config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDLoadingRes)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDLoadingRes Get(int id)
        {
            this.dict.TryGetValue(id, out LDLoadingRes item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDLoadingRes)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDLoadingRes> GetAll()
        {
            return this.dict;
        }

        public LDLoadingRes GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDLoadingRes: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>通用</summary>
		[ProtoMember(2)]
		public int Is_Common { get; set; }
		/// <summary>背景</summary>
		[ProtoMember(3)]
		public string Background { get; set; }

	}
}
