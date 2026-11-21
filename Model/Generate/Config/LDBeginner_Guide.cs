using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDBeginner_GuideCategory : ProtoObject, IMerge
    {
        public static LDBeginner_GuideCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDBeginner_Guide> dict = new Dictionary<int, LDBeginner_Guide>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDBeginner_Guide> list = new List<LDBeginner_Guide>();
		
        public LDBeginner_GuideCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDBeginner_GuideCategory s = o as LDBeginner_GuideCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDBeginner_Guide config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDBeginner_Guide)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDBeginner_Guide Get(int id)
        {
            this.dict.TryGetValue(id, out LDBeginner_Guide item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDBeginner_Guide)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDBeginner_Guide> GetAll()
        {
            return this.dict;
        }

        public LDBeginner_Guide GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDBeginner_Guide: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>下个引导</summary>
		[ProtoMember(2)]
		public int[] Next_Id { get; set; }
		/// <summary>触发条件 0=仅前序步骤进入 1=默认 2=等级 3=交付任务 4=接任务 5=首次获得装备</summary>
		[ProtoMember(3)]
		public int Trigger_Condition { get; set; }
		/// <summary>触发参数（int[]） 2：等级 3/4：任务ID 0/1/5：空</summary>
		[ProtoMember(4)]
		public int[] Trigger_Condition_Param { get; set; }
		/// <summary>完成条件 0、null=点击任意区域 1=点击指定区域 2=滑动</summary>
		[ProtoMember(5)]
		public int Complete_Requirement { get; set; }
		/// <summary>定位类型Position_Id 1任务 2穿戴 3入口 4页签 5关卡 6技能 7学习 8配置 9槽位 10返回 11骑乘 12出战 详见批注</summary>
		[ProtoMember(6)]
		public int Position_Id { get; set; }
		/// <summary>定位参数Position_Param（int[]） 1：空=首个有效任务；单值=任务ID 3/4/10：Belong条目Id 5：Scene_Id；6/7：技能ID 9：方案|Position|方向 2/8/11：空；12：待确定</summary>
		[ProtoMember(7)]
		public int[] Position_Param { get; set; }
		/// <summary>引导类型 1=圆形 2=矩形</summary>
		[ProtoMember(8)]
		public int Type { get; set; }
		/// <summary>引导缩放</summary>
		[ProtoMember(9)]
		public double Zoom { get; set; }
		/// <summary>引导区域宽|高 0|0=使用目标实际尺寸 （约定，需代码接入）</summary>
		[ProtoMember(10)]
		public double[] Size { get; set; }
		/// <summary>引导文本 Word_ID</summary>
		[ProtoMember(11)]
		public int Text { get; set; }
		/// <summary>文本偏移X|Y 0|0=自动靠近、居中 （约定，需代码接入）</summary>
		[ProtoMember(12)]
		public double[] Text_Offset { get; set; }
		/// <summary>中断回溯步骤ID 0/空=不回溯 本表回到组首步骤</summary>
		[ProtoMember(13)]
		public int Break_Backup_Id { get; set; }
		/// <summary>关闭开关 0/空=启用 1=禁用</summary>
		[ProtoMember(14)]
		public int Is_Close { get; set; }

	}
}
