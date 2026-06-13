using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDSceneCategory : ProtoObject, IMerge
    {
        public static LDSceneCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDScene> dict = new Dictionary<int, LDScene>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDScene> list = new List<LDScene>();
		
        public LDSceneCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDSceneCategory s = o as LDSceneCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDScene config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDScene Get(int id)
        {
            this.dict.TryGetValue(id, out LDScene item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDScene)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDScene> GetAll()
        {
            return this.dict;
        }

        public LDScene GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDScene: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(2)]
		public int Name { get; set; }
		/// <summary>描述</summary>
		[ProtoMember(3)]
		public int Desc { get; set; }
		/// <summary>准入 等级</summary>
		[ProtoMember(4)]
		public int Lv_Enter { get; set; }
		/// <summary>推荐 等级</summary>
		[ProtoMember(5)]
		public int Lv_Suggest { get; set; }
		/// <summary>人数 限制</summary>
		[ProtoMember(6)]
		public int Limit_Player { get; set; }
		/// <summary>次数 限制</summary>
		[ProtoMember(7)]
		public int Limit_Times { get; set; }
		/// <summary>类型</summary>
		[ProtoMember(8)]
		public int MapType { get; set; }
		/// <summary>出生点</summary>
		[ProtoMember(9)]
		public double[] Pos_Born { get; set; }
		/// <summary>传送ID</summary>
		[ProtoMember(10)]
		public int[] Teleport_Id { get; set; }
		/// <summary>NPC</summary>
		[ProtoMember(11)]
		public int[] NpcList { get; set; }
		/// <summary>小地图 0-不显示 1-显示</summary>
		[ProtoMember(12)]
		public int If_MiniMap { get; set; }
		/// <summary>允许坐骑 0-否 1-是</summary>
		[ProtoMember(13)]
		public int If_Mount { get; set; }
		/// <summary>摄像机</summary>
		[ProtoMember(14)]
		public double[] CameraPos { get; set; }
		/// <summary>Loading</summary>
		[ProtoMember(15)]
		public int[] LoadingRes { get; set; }
		/// <summary>音乐</summary>
		[ProtoMember(16)]
		public string Music { get; set; }

	}
}
