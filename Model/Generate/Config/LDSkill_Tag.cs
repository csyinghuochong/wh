using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDSkill_TagCategory : ProtoObject, IMerge
    {
        public static LDSkill_TagCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDSkill_Tag> dict = new Dictionary<int, LDSkill_Tag>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDSkill_Tag> list = new List<LDSkill_Tag>();
		
        public LDSkill_TagCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDSkill_TagCategory s = o as LDSkill_TagCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDSkill_Tag config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDSkill_Tag)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDSkill_Tag Get(int id)
        {
            this.dict.TryGetValue(id, out LDSkill_Tag item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDSkill_Tag)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDSkill_Tag> GetAll()
        {
            return this.dict;
        }

        public LDSkill_Tag GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDSkill_Tag: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(2)]
		public int Name { get; set; }
		/// <summary>底图颜色</summary>
		[ProtoMember(3)]
		public int Color { get; set; }

	}
}
