using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDSkill_BattleCategory : ProtoObject, IMerge
    {
        public static LDSkill_BattleCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDSkill_Battle> dict = new Dictionary<int, LDSkill_Battle>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDSkill_Battle> list = new List<LDSkill_Battle>();
		
        public LDSkill_BattleCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDSkill_BattleCategory s = o as LDSkill_BattleCategory;
            this.list.AddRange(s.list);
        }
		
		public override void EndInit()
		{
			foreach (LDSkill_Battle config in list)
			{
				config.EndInit();
				if (this.dict.ContainsKey(config.Id))
				{
					throw new Exception($"配置表重复Id: 表={nameof(LDSkill_Battle)} Id={config.Id}");
				}
				this.dict.Add(config.Id, config);
			}
			this.AfterEndInit();
		}
		
        public LDSkill_Battle Get(int id)
        {
            this.dict.TryGetValue(id, out LDSkill_Battle item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDSkill_Battle)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDSkill_Battle> GetAll()
        {
            return this.dict;
        }

        public LDSkill_Battle GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDSkill_Battle: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(2)]
		public int Name { get; set; }
		/// <summary>简述</summary>
		[ProtoMember(3)]
		public int Desc_Brief { get; set; }
		/// <summary>图标</summary>
		[ProtoMember(4)]
		public string Icon { get; set; }
		/// <summary>BUFF 替换技能 前置优先</summary>
		[ProtoMember(5)]
		public string Replace_Skill { get; set; }
		/// <summary>宠物技能 关联性</summary>
		[ProtoMember(6)]
		public int[] Pet_Relate { get; set; }
		/// <summary>技能类型 0-无 1-瞬发 2-吟唱 3-引导 8-属性 9-被动</summary>
		[ProtoMember(7)]
		public int Type { get; set; }
		/// <summary>属性 值</summary>
		[ProtoMember(8)]
		public string Attribute { get; set; }
		/// <summary>被动 类型</summary>
		[ProtoMember(9)]
		public int Type_Passive { get; set; }
		/// <summary>公共 CD</summary>
		[ProtoMember(10)]
		public double PublicCD { get; set; }
		/// <summary>冷却 CD</summary>
		[ProtoMember(11)]
		public double SkillCD { get; set; }
		/// <summary>吟唱 引导 时长</summary>
		[ProtoMember(12)]
		public double Skill_Time { get; set; }
		/// <summary>引导 触发 间隔</summary>
		[ProtoMember(13)]
		public double Time_Interval { get; set; }
		/// <summary>进入CD 0-立即 1-结束</summary>
		[ProtoMember(14)]
		public int Enter_CD { get; set; }
		/// <summary>打断自身 普通攻击 0-否 1-是</summary>
		[ProtoMember(15)]
		public int Stop_Normal_Attack { get; set; }
		/// <summary>限制 移动 0-否 1-是</summary>
		[ProtoMember(16)]
		public int Limit_Move { get; set; }
		/// <summary>限制 转向 0-否 1-是</summary>
		[ProtoMember(17)]
		public int Limit_Rotate { get; set; }
		/// <summary>打断 技能 0-否 1-是</summary>
		[ProtoMember(18)]
		public int Interrupt_1 { get; set; }
		/// <summary>可被 打断 0-否 1-是</summary>
		[ProtoMember(19)]
		public int Interrupt_2 { get; set; }
		/// <summary>沉默 释放 0-否 1-是</summary>
		[ProtoMember(20)]
		public int Use_Silence { get; set; }
		/// <summary>眩晕 释放 0-否 1-是</summary>
		[ProtoMember(21)]
		public int Use_Stun { get; set; }
		/// <summary>目标对象 0-无需目标 1-需要目标 2-强制释放</summary>
		[ProtoMember(22)]
		public int NeedTarget { get; set; }
		/// <summary>目标类型 0-自身 1-友军 2-已方 3-敌人 9-全部</summary>
		[ProtoMember(23)]
		public int Target_Type { get; set; }
		/// <summary>释法 距离</summary>
		[ProtoMember(24)]
		public double Cast_Range { get; set; }
		/// <summary>索敌 距离</summary>
		[ProtoMember(25)]
		public double Search_Range { get; set; }
		/// <summary>基础点 0-自身 1-目标</summary>
		[ProtoMember(26)]
		public int Base_Position { get; set; }
		/// <summary>范围类型 0-单体 1-圆形 2-扇形 3-单侧矩形 4-中心矩形</summary>
		[ProtoMember(27)]
		public int Range_Type { get; set; }
		/// <summary>范围参数1 0-无 1-半径 2-半径 3-长 4-长</summary>
		[ProtoMember(28)]
		public double Range_Type_Param1 { get; set; }
		/// <summary>范围参数2 0-无 1-无 2-角度 3-宽 4-宽</summary>
		[ProtoMember(29)]
		public double Range_Type_Param2 { get; set; }
		/// <summary>目标 过滤</summary>
		[ProtoMember(30)]
		public int[] Target_Filter { get; set; }
		/// <summary>目标 优先级</summary>
		[ProtoMember(31)]
		public int[] Target_Priority { get; set; }
		/// <summary>优先级 是否顺序 0-否 1-是</summary>
		[ProtoMember(32)]
		public int Target_Priority_Param { get; set; }
		/// <summary>目标 筛选方式 0-最近优先 1-朝向最近优先 2-最远优先 3-朝向最远优先 4-属性最多 5-属性最少 9-手动锁定</summary>
		[ProtoMember(33)]
		public int[] Target_Select_Type { get; set; }
		/// <summary>目标筛选 属性ID</summary>
		[ProtoMember(34)]
		public int Target_Select_Type_Param { get; set; }
		/// <summary>施法 面对目标 0-否 1-是</summary>
		[ProtoMember(35)]
		public int LookTarget { get; set; }
		/// <summary>施法 自身 属性 要求</summary>
		[ProtoMember(36)]
		public string Self_Attribute_Limit { get; set; }
		/// <summary>施法 自身 BUFF 要求</summary>
		[ProtoMember(37)]
		public int[] Self_Buff_Limit { get; set; }
		/// <summary>施法 目标 BUFF 要求</summary>
		[ProtoMember(38)]
		public int[] Target_Buff_Limit { get; set; }
		/// <summary>生效 时间  发射 时间</summary>
		[ProtoMember(39)]
		public double Time_1 { get; set; }
		/// <summary>硬直 时间</summary>
		[ProtoMember(40)]
		public double Time_2 { get; set; }
		/// <summary>总 时间</summary>
		[ProtoMember(41)]
		public double Time_3 { get; set; }
		/// <summary>攻击动作</summary>
		[ProtoMember(42)]
		public string Attack_Animation { get; set; }
		/// <summary>攻击特效</summary>
		[ProtoMember(43)]
		public string Attack_VFX { get; set; }
		/// <summary>攻击挂点</summary>
		[ProtoMember(44)]
		public string Attack_Socket { get; set; }
		/// <summary>攻击材质</summary>
		[ProtoMember(45)]
		public string Attack_Shader { get; set; }
		/// <summary>受击动作</summary>
		[ProtoMember(46)]
		public string Hit_Animation { get; set; }
		/// <summary>受击特效</summary>
		[ProtoMember(47)]
		public string Hit_VFX { get; set; }
		/// <summary>受击挂点</summary>
		[ProtoMember(48)]
		public string Hit_Socket { get; set; }
		/// <summary>受击材质</summary>
		[ProtoMember(49)]
		public string Hit_Shader { get; set; }
		/// <summary>攻击音效</summary>
		[ProtoMember(50)]
		public string Attack_Audio { get; set; }
		/// <summary>动作震动 开时时间</summary>
		[ProtoMember(51)]
		public double Vibrate_Act_Begin { get; set; }
		/// <summary>动作 震动时间</summary>
		[ProtoMember(52)]
		public double Vibrate_Act_Time { get; set; }
		/// <summary>动作 震动次数</summary>
		[ProtoMember(53)]
		public int Vibrate_Act_Times { get; set; }
		/// <summary>动作 震动强度 1-9</summary>
		[ProtoMember(54)]
		public int Vibrate_Act_Intensity { get; set; }
		/// <summary>受击震动 0-否 1-是</summary>
		[ProtoMember(55)]
		public int Vibrate_Hit { get; set; }
		/// <summary>受击 震动时间</summary>
		[ProtoMember(56)]
		public double Vibrate_Hit_Time { get; set; }
		/// <summary>受击 震动强度 1-9</summary>
		[ProtoMember(57)]
		public int Vibrate_Hit_Intensity { get; set; }
		/// <summary>子弹 特效</summary>
		[ProtoMember(58)]
		public int Bullet_Effect { get; set; }
		/// <summary>子弹 速度</summary>
		[ProtoMember(59)]
		public int Bullet_Speed { get; set; }
		/// <summary>子弹 时间</summary>
		[ProtoMember(60)]
		public int Bullet_Time_Max { get; set; }

	}
}
