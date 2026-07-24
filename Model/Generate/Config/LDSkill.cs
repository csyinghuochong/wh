using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDSkillCategory : ProtoObject, IMerge
    {
        public static LDSkillCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDSkill> dict = new Dictionary<int, LDSkill>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDSkill> list = new List<LDSkill>();
		
        public LDSkillCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDSkillCategory s = o as LDSkillCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDSkill config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDSkill Get(int id)
        {
            this.dict.TryGetValue(id, out LDSkill item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDSkill)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDSkill> GetAll()
        {
            return this.dict;
        }

        public LDSkill GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDSkill: ProtoObject, IConfig
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
		public string SkillIcon { get; set; }
		/// <summary>BUFF 替换技能 前置优先</summary>
		[ProtoMember(5)]
		public string Replace_Skill { get; set; }
		/// <summary>技能类型 0-无 1-瞬发 2-吟唱 3-引导 9-被动</summary>
		[ProtoMember(6)]
		public int Type { get; set; }
		/// <summary>被动 类型</summary>
		[ProtoMember(7)]
		public int Type_Passive { get; set; }
		/// <summary>公共 CD</summary>
		[ProtoMember(8)]
		public double PublicCD { get; set; }
		/// <summary>冷却 CD</summary>
		[ProtoMember(9)]
		public double SkillCD { get; set; }
		/// <summary>吟唱 引导 时长</summary>
		[ProtoMember(10)]
		public double Skill_Time { get; set; }
		/// <summary>引导 触发 间隔</summary>
		[ProtoMember(11)]
		public double Time_Interval { get; set; }
		/// <summary>进入CD 0-立即 1-结束</summary>
		[ProtoMember(12)]
		public int Enter_CD { get; set; }
		/// <summary>打断自身 普通攻击 0-否 1-是</summary>
		[ProtoMember(13)]
		public int Stop_Normal_Attack { get; set; }
		/// <summary>限制 移动 0-否 1-是</summary>
		[ProtoMember(14)]
		public int Limit_Move { get; set; }
		/// <summary>限制 转向 0-否 1-是</summary>
		[ProtoMember(15)]
		public int Limit_Rotate { get; set; }
		/// <summary>主动 打断 0-否 1-是</summary>
		[ProtoMember(16)]
		public int Interrupt { get; set; }
		/// <summary>沉默 释放 0-否 1-是</summary>
		[ProtoMember(17)]
		public int Use_Silence { get; set; }
		/// <summary>眩晕 释放 0-否 1-是</summary>
		[ProtoMember(18)]
		public int Use_Stun { get; set; }
		/// <summary>目标对象 0-无需目标 1-需要目标 2-强制释放</summary>
		[ProtoMember(19)]
		public int NeedTarget { get; set; }
		/// <summary>目标类型 0-自身 1-友军 2-已方 3-敌人 9-全部</summary>
		[ProtoMember(20)]
		public int Target_Type { get; set; }
		/// <summary>释法 距离</summary>
		[ProtoMember(21)]
		public double Cast_Range { get; set; }
		/// <summary>索敌 距离</summary>
		[ProtoMember(22)]
		public double Search_Range { get; set; }
		/// <summary>基础点 0-自身 1-目标</summary>
		[ProtoMember(23)]
		public int Base_Position { get; set; }
		/// <summary>范围类型 0-单体 1-圆形 2-扇形 3-单侧矩形 4-中心矩形</summary>
		[ProtoMember(24)]
		public int Range_Type { get; set; }
		/// <summary>范围参数1 0-无 1-半径 2-半径 3-长 4-长</summary>
		[ProtoMember(25)]
		public double Range_Type_Param1 { get; set; }
		/// <summary>范围参数2 0-无 1-无 2-角度 3-宽 4-宽</summary>
		[ProtoMember(26)]
		public double Range_Type_Param2 { get; set; }
		/// <summary>目标 过滤</summary>
		[ProtoMember(27)]
		public int[] Target_Filter { get; set; }
		/// <summary>目标 优先级</summary>
		[ProtoMember(28)]
		public int[] Target_Priority { get; set; }
		/// <summary>优先级 是否顺序 0-否 1-是</summary>
		[ProtoMember(29)]
		public int Target_Priority_Param { get; set; }
		/// <summary>目标 筛选方式 0-最近优先 1-朝向最近优先 2-最远优先 3-朝向最远优先 4-属性最多 5-属性最少 9-手动锁定</summary>
		[ProtoMember(30)]
		public int[] Target_Select_Type { get; set; }
		/// <summary>目标筛选 属性ID</summary>
		[ProtoMember(31)]
		public int Target_Select_Type_Param { get; set; }
		/// <summary>施法 面对目标 0-否 1-是</summary>
		[ProtoMember(32)]
		public int LookTarget { get; set; }
		/// <summary>施法 自身属性要求 类型_参数_值|... 0-等于 1-大于等于 2-小于等于</summary>
		[ProtoMember(33)]
		public string Self_Attribute_Limit { get; set; }
		/// <summary>施法 自身 BUFF 要求</summary>
		[ProtoMember(34)]
		public int[] Self_Buff_Limit { get; set; }
		/// <summary>施法 目标 BUFF 要求</summary>
		[ProtoMember(35)]
		public int[] Target_Buff_Limit { get; set; }
		/// <summary>作用时间（前摇） 非子弹：生效时间 子弹：发射时间</summary>
		[ProtoMember(36)]
		public double Time_1 { get; set; }
		/// <summary>硬直 时间</summary>
		[ProtoMember(37)]
		public double Time_2 { get; set; }
		/// <summary>总 时间</summary>
		[ProtoMember(38)]
		public double Time_3 { get; set; }
		/// <summary>攻击动作</summary>
		[ProtoMember(39)]
		public string Attack_Animation { get; set; }
		/// <summary>攻击特效</summary>
		[ProtoMember(40)]
		public string Attack_VFX { get; set; }
		/// <summary>攻击挂点</summary>
		[ProtoMember(41)]
		public string Attack_Socket { get; set; }
		/// <summary>攻击材质</summary>
		[ProtoMember(42)]
		public string Attack_Shader { get; set; }
		/// <summary>受击动作</summary>
		[ProtoMember(43)]
		public string Hit_Animation { get; set; }
		/// <summary>受击特效</summary>
		[ProtoMember(44)]
		public string Hit_VFX { get; set; }
		/// <summary>受击挂点</summary>
		[ProtoMember(45)]
		public string Hit_Socket { get; set; }
		/// <summary>受击材质</summary>
		[ProtoMember(46)]
		public string Hit_Shader { get; set; }
		/// <summary>攻击音效</summary>
		[ProtoMember(47)]
		public string Attack_Audio { get; set; }
		/// <summary>动作震动 开时时间</summary>
		[ProtoMember(48)]
		public double Vibrate_Act_Begin { get; set; }
		/// <summary>动作 震动时间</summary>
		[ProtoMember(49)]
		public double Vibrate_Act_Time { get; set; }
		/// <summary>动作 震动次数</summary>
		[ProtoMember(50)]
		public int Vibrate_Act_Times { get; set; }
		/// <summary>动作 震动强度 1-9</summary>
		[ProtoMember(51)]
		public int Vibrate_Act_Intensity { get; set; }
		/// <summary>受击震动 0-否 1-是</summary>
		[ProtoMember(52)]
		public int Vibrate_Hit { get; set; }
		/// <summary>受击 震动时间</summary>
		[ProtoMember(53)]
		public double Vibrate_Hit_Time { get; set; }
		/// <summary>受击 震动强度 1-9</summary>
		[ProtoMember(54)]
		public int Vibrate_Hit_Intensity { get; set; }
		/// <summary>子弹 特效</summary>
		[ProtoMember(55)]
		public int Bullet_Effect { get; set; }
		/// <summary>子弹 速度</summary>
		[ProtoMember(56)]
		public int Bullet_Speed { get; set; }
		/// <summary>子弹 时间</summary>
		[ProtoMember(57)]
		public int Bullet_Time_Max { get; set; }

	}
}
