using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDEffectCategory : ProtoObject, IMerge
    {
        public static LDEffectCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDEffect> dict = new Dictionary<int, LDEffect>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDEffect> list = new List<LDEffect>();
		
        public LDEffectCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDEffectCategory s = o as LDEffectCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDEffect config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDEffect Get(int id)
        {
            this.dict.TryGetValue(id, out LDEffect item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDEffect)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDEffect> GetAll()
        {
            return this.dict;
        }

        public LDEffect GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDEffect: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>资源</summary>
		[ProtoMember(2)]
		public string Resource { get; set; }
		/// <summary>最大数量</summary>
		[ProtoMember(3)]
		public int Max_Num { get; set; }
		/// <summary>缩放值</summary>
		[ProtoMember(4)]
		public double Scale { get; set; }
		/// <summary>绝对</summary>
		[ProtoMember(5)]
		public int Absolute { get; set; }

	}
}
