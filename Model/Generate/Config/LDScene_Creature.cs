using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDScene_CreatureCategory : ProtoObject, IMerge
    {
        public static LDScene_CreatureCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDScene_Creature> dict = new Dictionary<int, LDScene_Creature>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDScene_Creature> list = new List<LDScene_Creature>();
		
        public LDScene_CreatureCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDScene_CreatureCategory s = o as LDScene_CreatureCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDScene_Creature config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDScene_Creature Get(int id)
        {
            this.dict.TryGetValue(id, out LDScene_Creature item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDScene_Creature)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDScene_Creature> GetAll()
        {
            return this.dict;
        }

        public LDScene_Creature GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDScene_Creature: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>场景Id</summary>
		[ProtoMember(2)]
		public int Scene_Id { get; set; }
		/// <summary>类型 1：NPC 2：怪物</summary>
		[ProtoMember(3)]
		public int Type { get; set; }
		/// <summary>对应ID</summary>
		[ProtoMember(4)]
		public int Match_Id { get; set; }
		/// <summary>刷新时间</summary>
		[ProtoMember(5)]
		public int Refresh { get; set; }
		/// <summary>位置</summary>
		[ProtoMember(6)]
		public int[] Position { get; set; }
		/// <summary>朝向</summary>
		[ProtoMember(7)]
		public int Rotation { get; set; }

	}
}
