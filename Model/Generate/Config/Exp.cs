using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class ExpCategory : ProtoObject, IMerge
    {
        public static ExpCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, Exp> dict = new Dictionary<int, Exp>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<Exp> list = new List<Exp>();
		
        public ExpCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            ExpCategory s = o as ExpCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (Exp config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public Exp Get(int id)
        {
            this.dict.TryGetValue(id, out Exp item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (Exp)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, Exp> GetAll()
        {
            return this.dict;
        }

        public Exp GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class Exp: ProtoObject, IConfig
	{
		/// <summary>等级</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>角色升级经验</summary>
		[ProtoMember(2)]
		public int Exp_Role { get; set; }

	}
}
