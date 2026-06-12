using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class LDSkillBuffCategory : ProtoObject, IMerge
    {
        public static LDSkillBuffCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, LDSkillBuff> dict = new Dictionary<int, LDSkillBuff>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<LDSkillBuff> list = new List<LDSkillBuff>();
		
        public LDSkillBuffCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            LDSkillBuffCategory s = o as LDSkillBuffCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            foreach (LDSkillBuff config in list)
            {
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public LDSkillBuff Get(int id)
        {
            this.dict.TryGetValue(id, out LDSkillBuff item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (LDSkillBuff)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, LDSkillBuff> GetAll()
        {
            return this.dict;
        }

        public LDSkillBuff GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class LDSkillBuff: ProtoObject, IConfig
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
		/// <summary>图标类型</summary>
		[ProtoMember(4)]
		public string BuffIconType { get; set; }
		/// <summary>图标</summary>
		[ProtoMember(5)]
		public string BuffIcon { get; set; }
		/// <summary>广播目标类型 0-全部 1-队友</summary>
		[ProtoMember(6)]
		public int BroadcastType { get; set; }
		/// <summary>Buff等级</summary>
		[ProtoMember(7)]
		public int BuffLv { get; set; }
		/// <summary>切换场景保留 0-不保留 1-切场景保留 2-离线保留</summary>
		[ProtoMember(8)]
		public int Transfer { get; set; }
		/// <summary>Buff存在时间 单位：毫秒</summary>
		[ProtoMember(9)]
		public int BuffTime { get; set; }
		/// <summary>Buff延迟生效时间</summary>
		[ProtoMember(10)]
		public int BuffDelayTime { get; set; }
		/// <summary>循环触发时间 单位：秒</summary>
		[ProtoMember(11)]
		public int BuffLoopTime { get; set; }
		/// <summary>目标类型 1-自身 2-队友 3-己方 4-敌方 5-全部</summary>
		[ProtoMember(12)]
		public int TargetType { get; set; }
		/// <summary>Buff脚本</summary>
		[ProtoMember(13)]
		public string BuffScript { get; set; }
		/// <summary>Buff类型 0 1-操作属性 2-操作状态 3-触发技能 4-装备技能 5-移除状态 6-奔跑大赛 7-免除技能上海 8-BUFF层数触发技能</summary>
		[ProtoMember(14)]
		public int BuffType { get; set; }
		/// <summary>Buff增益减益 0 1-增益 2-减益</summary>
		[ProtoMember(15)]
		public int BuffBenefitType { get; set; }
		/// <summary>Buff参数操作类型 1时表示属性 2时表示状态</summary>
		[ProtoMember(16)]
		public int buffParameterType { get; set; }
		/// <summary>Buff参数操作值 buffType=1 具体属性值 buffType=2 配置 0 buffParameterType=3164时 变身怪物ID</summary>
		[ProtoMember(17)]
		public double buffParameterValue { get; set; }
		/// <summary>Buff参数操作值2 护盾类技能配置 护盾被敌人打破 触发的技能ID</summary>
		[ProtoMember(18)]
		public string buffParameterValue2 { get; set; }
		/// <summary>buff操作参数值类型</summary>
		[ProtoMember(19)]
		public int buffParameterValueType { get; set; }
		/// <summary>buff操作参数值类型定义 0-整数 1-小数</summary>
		[ProtoMember(20)]
		public int buffParameterValueDef { get; set; }
		/// <summary>Buff是否叠加 0-不叠加 1-叠加 2-关联BUFF只存在1种</summary>
		[ProtoMember(21)]
		public int BuffAddClass { get; set; }
		/// <summary>Buff是叠加层数上限</summary>
		[ProtoMember(22)]
		public int BuffAddClassMax { get; set; }
		/// <summary>buff叠加后时间统一 0-各自计时 1-刷新计时</summary>
		[ProtoMember(23)]
		public int BuffAddSync { get; set; }
		/// <summary>唯一buffID</summary>
		[ProtoMember(24)]
		public string WeiYiBuffID { get; set; }
		/// <summary>伤害类型 1-物理攻击 2-魔法攻击</summary>
		[ProtoMember(25)]
		public int DamgeType { get; set; }
		/// <summary>伤害系数</summary>
		[ProtoMember(26)]
		public double DamgePro { get; set; }
		/// <summary>固定伤害值</summary>
		[ProtoMember(27)]
		public int DamgeValue { get; set; }
		/// <summary>是否立即释放 0-立即释放 1-延迟释放</summary>
		[ProtoMember(28)]
		public int IfImmediatelyUse { get; set; }
		/// <summary>是否在主界面显示BuffIcon 0-不显示 1-显示</summary>
		[ProtoMember(29)]
		public int IfShowIconTips { get; set; }
		/// <summary>buff特效</summary>
		[ProtoMember(30)]
		public int BuffEffectID { get; set; }
		/// <summary>附加目标类型 0-所有人 1-玩家 2-NPC    3-怪物 4-掉落   5-传送 6-宠物   7-精灵 8-动物   9-植物 10-弹道</summary>
		[ProtoMember(31)]
		public int[] BuffTargetType { get; set; }
		/// <summary>移除机制 0-默认 1-移动 2-被攻击 3-释放技能</summary>
		[ProtoMember(32)]
		public int[] Remove { get; set; }
		/// <summary>移动触发 0-默认 1-只有移动时触发</summary>
		[ProtoMember(33)]
		public int MoveAction { get; set; }
		/// <summary>叠加层数触发技能</summary>
		[ProtoMember(34)]
		public int[] AddSkill { get; set; }
		/// <summary>被击杀后是否移除机制 0-死亡移除 1-死亡不移除</summary>
		[ProtoMember(35)]
		public int DeadNoRemove { get; set; }

	}
}
