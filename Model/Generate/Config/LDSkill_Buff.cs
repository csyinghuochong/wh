using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDSkill_BuffCategory : ProtoObject, IMerge
    {
        public static LDSkill_BuffCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDSkill_Buff> dict = new Dictionary<int, LDSkill_Buff>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDSkill_Buff> list = new List<LDSkill_Buff>();
		
        public LDSkill_BuffCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDSkill_BuffCategory s = o as LDSkill_BuffCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDSkill_Buff config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDSkill_Buff Get(int id)
        {
            this.dict.TryGetValue(id, out LDSkill_Buff item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDSkill_Buff)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDSkill_Buff> GetAll()
        {
            return this.dict;
        }

        public LDSkill_Buff GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDSkill_Buff: ProtoObject, IConfig
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
		/// <summary>图标</summary>
		[ProtoMember(4)]
		public string Icon { get; set; }
		/// <summary>特殊显示 优先级 LS 0-不显示</summary>
		[ProtoMember(5)]
		public int Icon_Special_Show_Order_LS { get; set; }
		/// <summary>显示 优先级 LS 0-不显示</summary>
		[ProtoMember(6)]
		public int Icon_Show_Order_LS { get; set; }
		/// <summary>类型 0-无 1-增益 2-减益</summary>
		[ProtoMember(7)]
		public int Type_Effect { get; set; }
		/// <summary>添加类型 0-替换 1-叠加 2-延长 3-共存</summary>
		[ProtoMember(8)]
		public int Type_Add { get; set; }
		/// <summary>类型参数 0-无 1-上限数 2-无 3-共存数</summary>
		[ProtoMember(9)]
		public int Type_Add_Param { get; set; }
		/// <summary>自定组</summary>
		[ProtoMember(10)]
		public int[] Group { get; set; }
		/// <summary>互斥组</summary>
		[ProtoMember(11)]
		public int[] Group_Mutex { get; set; }
		/// <summary>互斥ID</summary>
		[ProtoMember(12)]
		public int[] Id_Mutex { get; set; }
		/// <summary>初始化 技能</summary>
		[ProtoMember(13)]
		public int Skill_Init { get; set; }
		/// <summary>刷新触发 技能</summary>
		[ProtoMember(14)]
		public int Skill_Refresh { get; set; }
		/// <summary>触发 技能</summary>
		[ProtoMember(15)]
		public int Skill_Trigger { get; set; }
		/// <summary>时间结束 技能</summary>
		[ProtoMember(16)]
		public int Skill_TimeEnd { get; set; }
		/// <summary>消失 技能</summary>
		[ProtoMember(17)]
		public int Skill_Remove { get; set; }
		/// <summary>特殊效果 添加</summary>
		[ProtoMember(18)]
		public int[] Special_Effect { get; set; }
		/// <summary>特殊效果 免疫</summary>
		[ProtoMember(19)]
		public int[] Special_Effect_Immune { get; set; }
		/// <summary>免疫组</summary>
		[ProtoMember(20)]
		public int[] Group_Immune { get; set; }
		/// <summary>移动移除 0-否 1-是</summary>
		[ProtoMember(21)]
		public int Remove_Move { get; set; }
		/// <summary>战斗移除 0-否 1-是</summary>
		[ProtoMember(22)]
		public int Remove_Battle_Enter { get; set; }
		/// <summary>脱战移除 0-否 1-是</summary>
		[ProtoMember(23)]
		public int Remove_Battle_Leave { get; set; }
		/// <summary>死亡移除 0-否 1-是</summary>
		[ProtoMember(24)]
		public int Remove_Dead { get; set; }
		/// <summary>安全区移除 0-否 1-是</summary>
		[ProtoMember(25)]
		public int Remove_SafeArea { get; set; }
		/// <summary>切场景移除 0-否 1-是</summary>
		[ProtoMember(26)]
		public int Remove_ChangeScene { get; set; }
		/// <summary>下线移除 0-否 1-是</summary>
		[ProtoMember(27)]
		public int Remove_Logout { get; set; }
		/// <summary>特效1</summary>
		[ProtoMember(28)]
		public string VFX_1 { get; set; }
		/// <summary>挂点1</summary>
		[ProtoMember(29)]
		public string VFX_Socket1 { get; set; }
		/// <summary>特效2</summary>
		[ProtoMember(30)]
		public string VFX_2 { get; set; }
		/// <summary>挂点2</summary>
		[ProtoMember(31)]
		public string VFX_Socket2 { get; set; }
		/// <summary>特效3</summary>
		[ProtoMember(32)]
		public string VFX_3 { get; set; }
		/// <summary>挂点3</summary>
		[ProtoMember(33)]
		public string VFX_Socket3 { get; set; }
		/// <summary>材质</summary>
		[ProtoMember(34)]
		public string Buff_Shader { get; set; }

	}
}
