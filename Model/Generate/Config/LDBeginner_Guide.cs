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
		/// <summary>触发条件 1=默认触发 2=等级触发 3=任务完成触发 4=任务接取触发</summary>
		[ProtoMember(3)]
		public int Trigger_Condition { get; set; }
		/// <summary>触发条件参数 1=对应等级 2=对应任务ID 3=对应任务ID</summary>
		[ProtoMember(4)]
		public int[] Trigger_Condition_Param { get; set; }
		/// <summary>完成条件 0、null=点击任意区域 1=点击指定区域 2=滑动</summary>
		[ProtoMember(5)]
		public int Complete_Requirement { get; set; }
		/// <summary>锚点=界面按钮ID</summary>
		[ProtoMember(6)]
		public int Position_Id { get; set; }
		/// <summary>引导类型 1=圆形 2=矩形</summary>
		[ProtoMember(7)]
		public int Type { get; set; }
		/// <summary>引导缩放</summary>
		[ProtoMember(8)]
		public double Zoom { get; set; }
		/// <summary>引导区域长宽</summary>
		[ProtoMember(9)]
		public double[] Size { get; set; }
		/// <summary>引导文本 Word_ID</summary>
		[ProtoMember(10)]
		public int Text { get; set; }
		/// <summary>文本偏移：相对引导位置 第1个参数=X（左右偏移） 第2个参数=Y（上下偏移）</summary>
		[ProtoMember(11)]
		public double[] Text_Offset { get; set; }
		/// <summary>中断回溯点 0、null=不回溯</summary>
		[ProtoMember(12)]
		public int Break_Backup_Id { get; set; }
		/// <summary>禁用 0、null=启用 1=禁用</summary>
		[ProtoMember(13)]
		public int Is_Close { get; set; }

	}
}
