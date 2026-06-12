using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDExpCategory : ProtoObject, IMerge
    {
        public static LDExpCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDExp> dict = new Dictionary<int, LDExp>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDExp> list = new List<LDExp>();
		
        public LDExpCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDExpCategory s = o as LDExpCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDExp config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDExp Get(int id)
        {
            this.dict.TryGetValue(id, out LDExp item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDExp)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDExp> GetAll()
        {
            return this.dict;
        }

        public LDExp GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDExp: ProtoObject, IConfig
	{
		/// <summary>等级</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>角色升级经验</summary>
		[ProtoMember(2)]
		public int Exp_Role { get; set; }

	}
}
