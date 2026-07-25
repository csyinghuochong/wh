namespace ET
{
    /// <summary>CREATE_SUMMON / UNIT_ADD_SUMMON 运行时参数。</summary>
    public class SummonRuntimeData
    {
        public int SummonId;
        /// <summary><see cref="SkillEntityActionType"/></summary>
        public int ActionType;
        /// <summary><see cref="SkillEntityMoveType"/></summary>
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
        /// <summary><see cref="SkillEntityDestroyMode"/></summary>
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
        /// <summary>上次移动结算时间，用于按真实 dt 飞行（服务端 FrameTimer=100ms）</summary>
        public long LastUpdateTime;
    }
}
