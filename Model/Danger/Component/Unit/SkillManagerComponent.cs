using System.Collections.Generic;

namespace ET
{


    public class SkillManagerComponent : Entity, IAwake, IDestroy
    {
        public List<Skill_TreeEditor> Skills = new List<Skill_TreeEditor>();
        public List<SkillInfo> DelaySkillList = new List<SkillInfo>();
        /// <summary>施法临时 SkillInfo 列表（复用，避免每次 new List）。</summary>
        public List<SkillInfo> TempSkillInfos = new List<SkillInfo>(4);
        /// <summary>广播用 SkillInfo 列表（复用）。</summary>
        public List<SkillInfo> BroadcastSkillInfos = new List<SkillInfo>(4);
        /// <summary>同步给客户端的当前技能列表（复用）。</summary>
        public List<SkillInfo> MessageSkillInfos = new List<SkillInfo>(8);
        public Dictionary<int, SkillCDItem> SkillCDs = new Dictionary<int, SkillCDItem>();  //技能CD列表
        /// <summary>普通技能公共 CD 结束时间</summary>
        public long SkillPublicCDTime;
        /// <summary>道具/药水技能公共 CD 结束时间（与技能公共 CD 互不影响）</summary>
        public long ItemPublicCDTime;
        public int FangunComboNumber;
        public long FangunLastTime;
        public int FangunSkillId;
        public long LastLianJiTime = 0;
        public long Timer;

        /// <summary>怪物吟唱前摇计时。到点后才 OnUseSkill。</summary>
        public long SingTimer;
        /// <summary>吟唱中缓存的施法指令；SkillID==0 表示未在吟唱。</summary>
        public C2M_SkillCmd SingSkillCmd = new C2M_SkillCmd();

        public M2C_SkillCmd M2C_SkillCmd = new M2C_SkillCmd();
        public M2C_UnitFinishSkill M2C_UnitFinishSkill = new M2C_UnitFinishSkill();
        public UnitComponent SelfUnitComponent;
        public Unit SelfUnit;
    }
}
