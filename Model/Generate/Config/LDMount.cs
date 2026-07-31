using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDMountCategory : ProtoObject, IMerge
    {
        public static LDMountCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDMount> dict = new Dictionary<int, LDMount>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDMount> list = new List<LDMount>();
		
        public LDMountCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDMountCategory s = o as LDMountCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDMount config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDMount)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDMount Get(int id)
        {
            this.dict.TryGetValue(id, out LDMount item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDMount)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDMount> GetAll()
        {
            return this.dict;
        }

        public LDMount GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDMount: ProtoObject, IConfig
	{
		/// <summary>ID</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(2)]
		public int Name { get; set; }
		/// <summary>描述</summary>
		[ProtoMember(3)]
		public int Desc { get; set; }
		/// <summary>获取描述</summary>
		[ProtoMember(4)]
		public int Desc_Get { get; set; }
		/// <summary>模型</summary>
		[ProtoMember(5)]
		public string Model { get; set; }
		/// <summary>特效</summary>
		[ProtoMember(6)]
		public string Effect { get; set; }
		/// <summary>品质</summary>
		[ProtoMember(7)]
		public int Quality { get; set; }
		/// <summary>标签</summary>
		[ProtoMember(8)]
		public string Tage { get; set; }
		/// <summary>生命资质</summary>
		[ProtoMember(9)]
		public int Aptitude_HP { get; set; }
		/// <summary>物攻资质</summary>
		[ProtoMember(10)]
		public int Aptitude_Atk { get; set; }
		/// <summary>法攻资质</summary>
		[ProtoMember(11)]
		public int Aptitude_MagAtk { get; set; }
		/// <summary>物防资质</summary>
		[ProtoMember(12)]
		public int Aptitude_Def { get; set; }
		/// <summary>法防资质</summary>
		[ProtoMember(13)]
		public int Aptitude_MagDef { get; set; }
		/// <summary>移动速度</summary>
		[ProtoMember(14)]
		public int Speed { get; set; }
		/// <summary>启用</summary>
		[ProtoMember(15)]
		public int Enable { get; set; }

	}
}
