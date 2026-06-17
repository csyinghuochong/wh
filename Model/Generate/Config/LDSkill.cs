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
		/// <summary>NextId</summary>
		[ProtoMember(2)]
		public int NextId { get; set; }
		/// <summary>等级</summary>
		[ProtoMember(3)]
		public int Lv_Skill { get; set; }
		/// <summary>名称</summary>
		[ProtoMember(4)]
		public int Name { get; set; }
		/// <summary>描述</summary>
		[ProtoMember(5)]
		public int Desc { get; set; }
		/// <summary>图标</summary>
		[ProtoMember(6)]
		public string SkillIcon { get; set; }
		/// <summary>学习 等级</summary>
		[ProtoMember(7)]
		public int Lv_Learn { get; set; }
		/// <summary>升级 点数</summary>
		[ProtoMember(8)]
		public int CostSP { get; set; }
		/// <summary>升级 消耗</summary>
		[ProtoMember(9)]
		public string Cost { get; set; }
		/// <summary>被控制时 可否释放 0-否 1-是</summary>
		[ProtoMember(10)]
		public int OpenType { get; set; }
		/// <summary>技能类型 1-主动 2-被动 3-装备提升技能 5-加属性 GameObjectParameter 8-不算战力加属性</summary>
		[ProtoMember(11)]
		public int SkillType { get; set; }
		/// <summary>被动技能 触发类型 skilltype = 1  0-无  1-可打断技能 2-蓄力技能 skilltype = 2 0-无  1-普攻触发概率  2-血量低于 x% 触发 3-受到伤害触发概率  4-暴击触发  5-闪避触发 6-濒死触发  7-释放技能触发  8-更换武器触发 9-近战普攻触发 <=4  10-远程普攻触发 >4 11-触发天赋  12-自己和友军进入地图触发 13-眩晕时触发  14-站立不动时触发 15-切换时触发  16-普攻次数触发 17-普攻+释放技能触发  18-宠物对战开局前触发 19-普攻暴击时触发  20-翻滚CD时触发 21-受伤触发，只触发一次   22-技能触发，BUFF绑定上一个技能</summary>
		[ProtoMember(12)]
		public int[] PassiveSkillType { get; set; }
		/// <summary>被动技能 触发参数  对应前面 1-概率 2-血量比 3-概率 4-概率 5-概率 6-概率</summary>
		[ProtoMember(13)]
		public double[] PassiveSkillPro { get; set; }
		/// <summary>被动技能 触发一次  0-否 1-是</summary>
		[ProtoMember(14)]
		public int PassiveSkillTriggerOnce { get; set; }
		/// <summary>被动技能 施法目标  0-当前 1-范围内随机1个敌人 2-最近的敌人 3-最远的敌人 21-范围内随机2个敌人 101-敌人全体</summary>
		[ProtoMember(15)]
		public int SkillTargetTypeNum { get; set; }
		/// <summary>连招技能ID</summary>
		[ProtoMember(16)]
		public int ComboSkillID { get; set; }
		/// <summary>技能攻击类型</summary>
		[ProtoMember(17)]
		public int SkillActType { get; set; }
		/// <summary>伤害类型 1-物理 2-法术</summary>
		[ProtoMember(18)]
		public int DamgeType { get; set; }
		/// <summary>伤害元素 0-无 1-光 2-暗 3-火 4-水 5-电</summary>
		[ProtoMember(19)]
		public int DamgeElementType { get; set; }
		/// <summary>攻击系数</summary>
		[ProtoMember(20)]
		public double ActDamge { get; set; }
		/// <summary>怪物 攻击系数</summary>
		[ProtoMember(21)]
		public double MonsterActDamge { get; set; }
		/// <summary>固定值</summary>
		[ProtoMember(22)]
		public int DamgeValue { get; set; }
		/// <summary>必中 0-否 1-是</summary>
		[ProtoMember(23)]
		public int IfMustAct { get; set; }
		/// <summary>消耗魔法</summary>
		[ProtoMember(24)]
		public int SkillUseMP { get; set; }
		/// <summary>增加魔法</summary>
		[ProtoMember(25)]
		public int SkillAddMP { get; set; }
		/// <summary>公共CD 0-触发 1-不触发</summary>
		[ProtoMember(26)]
		public int IfPublicSkillCD { get; set; }
		/// <summary>冷却CD</summary>
		[ProtoMember(27)]
		public double SkillCD { get; set; }
		/// <summary>伤害范围类型 0-圆形 1-圆形 2-矩形 3-扇形</summary>
		[ProtoMember(28)]
		public int DamgeRangeType { get; set; }
		/// <summary>伤害范围</summary>
		[ProtoMember(29)]
		public double[] DamgeRange { get; set; }
		/// <summary>技能目标 0-立即，自身中心 1-立即，目标中心 2-技能圆形指示器</summary>
		[ProtoMember(30)]
		public int SkillTargetType { get; set; }
		/// <summary>指示器 0-无 1-手动 2-直线 3-60° 4-120°</summary>
		[ProtoMember(31)]
		public int SkillZhishiType { get; set; }
		/// <summary>释放区域 目标点 0-目标 1-自身 2-朝向最远处</summary>
		[ProtoMember(32)]
		public int SkillZhishiTargetType { get; set; }
		/// <summary>释放区域大小</summary>
		[ProtoMember(33)]
		public double SkillRangeSize { get; set; }
		/// <summary>技能指示器增加范围</summary>
		[ProtoMember(34)]
		public int SkillRangeZhiShiSize { get; set; }
		/// <summary>施法前吟唱时间</summary>
		[ProtoMember(35)]
		public double SkillFrontSingTime { get; set; }
		/// <summary>施法中吟唱时间</summary>
		[ProtoMember(36)]
		public double SkillSingTime { get; set; }
		/// <summary>技能僵直</summary>
		[ProtoMember(37)]
		public double SkillRigidity { get; set; }
		/// <summary>技能存在时间[毫秒]</summary>
		[ProtoMember(38)]
		public int SkillLiveTime { get; set; }
		/// <summary>技能效果延迟时间</summary>
		[ProtoMember(39)]
		public double SkillDelayTime { get; set; }
		/// <summary>技能移动速度</summary>
		[ProtoMember(40)]
		public double SkillMoveSpeed { get; set; }
		/// <summary>初始化 BUFFID</summary>
		[ProtoMember(41)]
		public int[] InitBuffID { get; set; }
		/// <summary>释放 BUFFID</summary>
		[ProtoMember(42)]
		public int[] BuffID { get; set; }
		/// <summary>只释放一次buff</summary>
		[ProtoMember(43)]
		public int[] OnlyOnceBuffID { get; set; }
		/// <summary>施法动作名称</summary>
		[ProtoMember(44)]
		public string SkillAnimation { get; set; }
		/// <summary>技能音效</summary>
		[ProtoMember(45)]
		public string SkillMusic { get; set; }
		/// <summary>技能特效ID</summary>
		[ProtoMember(46)]
		public int SkillHitEffectID { get; set; }
		/// <summary>特效ID</summary>
		[ProtoMember(47)]
		public int[] SkillEffectID { get; set; }
		/// <summary>脚本名称</summary>
		[ProtoMember(48)]
		public string GameObjectName { get; set; }
		/// <summary>每个脚本对应参数</summary>
		[ProtoMember(49)]
		public string GameObjectParameter { get; set; }
		/// <summary>所有脚本通用参数</summary>
		[ProtoMember(50)]
		public string ComObjParameter { get; set; }
		/// <summary>显示 0-是 1-否</summary>
		[ProtoMember(51)]
		public int IsShow { get; set; }
		/// <summary>施法时面对目标时间</summary>
		[ProtoMember(52)]
		public double IfLookAtTatgetTime { get; set; }
		/// <summary>触发技能时附带技能</summary>
		[ProtoMember(53)]
		public int[] AddSkillID { get; set; }
		/// <summary>技能触发时间</summary>
		[ProtoMember(54)]
		public double PassiveSkillTriggerTime { get; set; }
		/// <summary>施法是否 面对目标 做出动作 0-需要 1-不需要</summary>
		[ProtoMember(55)]
		public int IfLookAtTarget { get; set; }
		/// <summary>怪物技能延迟</summary>
		[ProtoMember(56)]
		public double MonsterDelayTime { get; set; }
		/// <summary>宠物互斥ID</summary>
		[ProtoMember(57)]
		public int HuChiID { get; set; }
		/// <summary>触发自身拥有技能</summary>
		[ProtoMember(58)]
		public int[] TriggerSelfSkillID { get; set; }
		/// <summary>施法 打断移动 0-是 1-否</summary>
		[ProtoMember(59)]
		public int IfStopMove { get; set; }
		/// <summary>技能持续伤害是否触发Buff</summary>
		[ProtoMember(60)]
		public int DamgeChiXuTrigerBuff { get; set; }
		/// <summary>技能持续伤害间隔时间</summary>
		[ProtoMember(61)]
		public int DamgeChiXuInterval { get; set; }
		/// <summary>技能持续伤害百分比</summary>
		[ProtoMember(62)]
		public double DamgeChiXuPro { get; set; }
		/// <summary>技能持续伤害固定值</summary>
		[ProtoMember(63)]
		public int DamgeChiXuValue { get; set; }
		/// <summary>显示 指示器 0-是 1-否</summary>
		[ProtoMember(64)]
		public int IfShowSkillZhiShi { get; set; }
		/// <summary>结束时技能</summary>
		[ProtoMember(65)]
		public int EndSkillId { get; set; }
		/// <summary>Buff触发技能</summary>
		[ProtoMember(66)]
		public string BuffToSkill { get; set; }
		/// <summary>技能伤害增加</summary>
		[ProtoMember(67)]
		public string SkillDamgeAddValue { get; set; }
		/// <summary>技能最多攻击人数</summary>
		[ProtoMember(68)]
		public int MaxAttackNumber { get; set; }
		/// <summary>额外附加属性</summary>
		[ProtoMember(69)]
		public string ExtraProperty { get; set; }
		/// <summary>指定攻击怪物</summary>
		[ProtoMember(70)]
		public int[] SpecifiedMonster { get; set; }
		/// <summary>附带固定战力</summary>
		[ProtoMember(71)]
		public int AddCombat { get; set; }

	}
}
