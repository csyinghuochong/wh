namespace ET
{
    /// <summary>CREATE_SUMMON / UNIT_ADD_SUMMON 运行时参数。</summary>
    public class SummonRuntimeData
    {
        public int SummonId;
        /// <summary>旧版 UNIT_ADD_SUMMON「作用类型」。</summary>
        public int ActionType;
        /// <summary>0静止 1直线 2追踪。</summary>
        public int MoveType;
        public long TrackTargetId;
        public bool DeleteOnBlock;
        /// <summary>追到目标后删除。</summary>
        public bool DeleteOnTrackReach;
        public long MaxDurationMs;
        public long IntervalMs;
        public int MaxActionCount;
        public bool TriggerOnCreate;
        /// <summary>Skill_1 创建技能。</summary>
        public int CreateSkillId;
        public int CreateSkillLevel;
        /// <summary>Skill_2 间隔/碰撞技能。</summary>
        public int ActionSkillId;
        public int ActionSkillLevel;
        /// <summary>Skill_3 追到技能。</summary>
        public int TrackSkillId;
        public int TrackSkillLevel;
        public int DestroyMode;
        /// <summary>Skill_4 消亡技能。</summary>
        public int DestroySkillId;
        public int DestroySkillLevel;
        public bool DestroyOnCount;
        public bool DestroyOnMasterDead;
        public bool DestroyOnTargetDead;
        public bool LockTarget;
        public int ActionCount;
        public bool TrackSkillFired;
    }

    /// <summary>技能体服务端运行时。客户端表现见 Unity SkillEntityComponent。</summary>
    public class SkillEntityComponent : Entity, IAwake, IDestroy
    {
        public BuffState BuffState;
        public long Timer;
        /// <summary>创建时刻 ServerNow。</summary>
        public long BeginTime;
        /// <summary>已运行毫秒：Now - BeginTime。</summary>
        public long PassTime;
        /// <summary>到期时刻，超时走 Skill_4。</summary>
        public long BuffEndTime;
        /// <summary>Skill_2 间隔毫秒，与 Buff 同一套轴。</summary>
        public long InterValTime;
        /// <summary>下一次 Skill_2 可触发的绝对时间。</summary>
        public long InterValTimeBegin;
        public long Masterid;
        public Skill_TreeEditor SkillHandler;
        public LDSummon SummonConfig;
        public SummonRuntimeData Runtime;
        /// <summary>出生点（人物脚底），XZ 飞行起点；Y 作飞行高度回退。</summary>
        public UnityEngine.Vector3 StartPosition;
        /// <summary>直线飞行方向（XZ）。</summary>
        public UnityEngine.Vector3 FlyDirection;
    }
}
