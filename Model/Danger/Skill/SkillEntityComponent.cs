namespace ET
{
    /// <summary>CREATE_SUMMON / UNIT_ADD_SUMMON 运行时参数。</summary>
    public class SummonRuntimeData
    {
        public int SummonId;
        /// <summary>0=计时触发，1=碰撞生效</summary>
        public int ActionType;
        /// <summary>0=静止，1=直线，2=追踪</summary>
        public int MoveType;
        public long TrackTargetId;
        public bool DeleteOnBlock;
        public bool DeleteOnTrackReach;
        public long MaxDurationMs;
        public long IntervalMs;
        public int MaxActionCount;
        public bool TriggerOnCreate;
        public int ActionSkillId;
        public int ActionSkillLevel;
        /// <summary>1=次数，10=主人死亡，11=次数+主人死亡</summary>
        public int DestroyMode;
        public int DestroySkillId;
        public int DestroySkillLevel;
        public bool LockTarget;

        public int ActionCount;
        public long LastActionTime;
    }

    /// <summary>
    /// 技能体（UnitType.SkillEntity）服务端运行时：移动 / 碰撞 / 作用技能 / 消亡。
    /// 客户端表现见 Unity SkillEntityComponent（本地飞行）。
    /// </summary>
    public class SkillEntityComponent : Entity, IAwake, IDestroy
    {
        public long PassTime;
        public long BuffEndTime;
        public long BeginTime;
        public long DelayTime;
        public float DamageRange;
        public long Masterid;
        public Skill_TreeEditor SkillHandler;
        public LDSummon SummonConfig;
        public SummonRuntimeData Runtime;

        public BuffState BuffState;
        public long Timer;
        public long DamgeChiXuLastTime;
        public long LastActionTime;
    }
}
