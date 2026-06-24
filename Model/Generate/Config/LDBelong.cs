using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDBelongCategory : ProtoObject, IMerge
    {
        public static LDBelongCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDBelong> dict = new Dictionary<int, LDBelong>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDBelong> list = new List<LDBelong>();
		
        public LDBelongCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDBelongCategory s = o as LDBelongCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDBelong config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDBelong Get(int id)
        {
            this.dict.TryGetValue(id, out LDBelong item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDBelong)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDBelong> GetAll()
        {
            return this.dict;
        }

        public LDBelong GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDBelong: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>所属ID</summary>
		[ProtoMember(2)]
		public int Belong_Id { get; set; }
		/// <summary>排序</summary>
		[ProtoMember(3)]
		public int Order_SL { get; set; }
		/// <summary>红点穿透</summary>
		[ProtoMember(4)]
		public int Red_Dot_Penetrate { get; set; }
		/// <summary>红点消失 0-默认 1-点击本次消失 2-点击永久消失</summary>
		[ProtoMember(5)]
		public int Red_Dot_Vanish_Type { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(6)]
		public int Name { get; set; }
		/// <summary>描述</summary>
		[ProtoMember(7)]
		public int Desc { get; set; }
		/// <summary>资源</summary>
		[ProtoMember(8)]
		public string Resources { get; set; }
		/// <summary>开服天数 非0生效</summary>
		[ProtoMember(9)]
		public int Open_Day { get; set; }
		/// <summary>角色等级 非0生效</summary>
		[ProtoMember(10)]
		public int Role_Level { get; set; }
		/// <summary>关系 0-且 1-或</summary>
		[ProtoMember(11)]
		public int Relationship { get; set; }

	}
}
