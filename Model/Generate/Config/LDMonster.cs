using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDMonsterCategory : ProtoObject, IMerge
    {
        public static LDMonsterCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDMonster> dict = new Dictionary<int, LDMonster>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDMonster> list = new List<LDMonster>();
		
        public LDMonsterCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDMonsterCategory s = o as LDMonsterCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDMonster config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDMonster Get(int id)
        {
            this.dict.TryGetValue(id, out LDMonster item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDMonster)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDMonster> GetAll()
        {
            return this.dict;
        }

        public LDMonster GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDMonster: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(2)]
		public int Name { get; set; }
		/// <summary>类型</summary>
		[ProtoMember(3)]
		public int Type { get; set; }
		/// <summary>头像</summary>
		[ProtoMember(4)]
		public string Icon { get; set; }
		/// <summary>模型</summary>
		[ProtoMember(5)]
		public string Model { get; set; }
		/// <summary>等级</summary>
		[ProtoMember(6)]
		public int Lv { get; set; }

	}
}
