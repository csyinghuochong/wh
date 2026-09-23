using System;
using UnityEngine;

namespace ET
{
    /// <summary>服务端技能体 100ms 心跳，转调 OnUpdate。</summary>
    [Timer(TimerType.SkillEntityTimer)]
    public class SkillEntityTimer : ATimer<SkillEntityComponent>
    {
        /// <summary>定时器回调：驱动飞行、Skill_2/3/4。</summary>
        public override void Run(SkillEntityComponent self)
        {
            try
            {
                self.OnUpdate();
            }
            catch (Exception e)
            {
                Log.Error($"SkillEntity timer error: {self.Id}\n{e}");
            }
        }
    }

    /// <summary>技能体挂上 Unit 时的占位 Awake，真正初始化走 Init。</summary>
    [ObjectSystem]
    public class SkillEntityComponentAwake : AwakeSystem<SkillEntityComponent>
    {
        /// <summary>组件创建，逻辑在 Init。</summary>
        public override void Awake(SkillEntityComponent self)
        {
        }
    }

    /// <summary>技能体销毁时摘掉心跳。</summary>
    [ObjectSystem]
    public class SkillEntityComponentDestroy : DestroySystem<SkillEntityComponent>
    {
        /// <summary>移除 SkillEntityTimer，避免 Unit 删后还 Tick。</summary>
        public override void Destroy(SkillEntityComponent self)
        {
            TimerComponent.Instance?.Remove(ref self.Timer);
        }
    }

    /// <summary>Skill_1 创建 / Skill_2 间隔 / Skill_3 追到 / Skill_4 消亡。间隔与 Buff 同一套时间轴。</summary>
    public static class SkillEntityComponentSystem
    {
        /// <summary>贴身判定距离（米），追踪飞到此距离停下。</summary>
        private const float CollideReach = 0.5f;

        /// <summary>
        /// 创建后初始化：时间轴、出生点、同步 Numeric（MoveType/Track/StartTime），飞行类抬到人物高度+1。
        /// </summary>
        public static void Init(
            this SkillEntityComponent self,
            Skill_TreeEditor skillHandler,
            long masterId,
            LDSummon summonConfig,
            SummonRuntimeData runtime)
        {
            long now = TimeHelper.ServerNow();
            self.Masterid = masterId;
            self.BuffState = BuffState.Running;
            self.SkillHandler = skillHandler;
            self.SummonConfig = summonConfig;
            self.Runtime = runtime ?? new SummonRuntimeData();
            self.BeginTime = now;
            self.PassTime = 0;

            Unit unit = self.GetParent<Unit>();
            self.StartPosition = unit != null ? unit.Position : default;
            self.FlyDirection = unit != null ? unit.Rotation * Vector3.forward : Vector3.forward;
            self.FlyDirection.y = 0f;
            if (self.FlyDirection.sqrMagnitude > 1e-6f)
            {
                self.FlyDirection.Normalize();
            }
            else
            {
                self.FlyDirection = Vector3.forward;
            }

            SummonRuntimeData rt = self.Runtime;
            self.BuffEndTime = now + (rt.MaxDurationMs > 0 ? rt.MaxDurationMs : 60000);
            self.InterValTime = rt.IntervalMs;
            self.InterValTimeBegin = self.BeginTime + (self.InterValTime > 0 ? self.InterValTime : 0);
            self.Timer = TimerComponent.Instance.NewRepeatedTimer(100, TimerType.SkillEntityTimer, self);

            NumericComponent numeric = unit?.GetComponent<NumericComponent>();
            numeric?.ApplyValue(NumericType.SkillEntity_MoveType, rt.MoveType, false);
            numeric?.ApplyValue(NumericType.SkillEntity_TrackTargetId, rt.TrackTargetId, false);
            numeric?.ApplyValue(NumericType.SkillEntity_StartTime, now, false);

            if (unit != null && rt.MoveType != SkillEntityMoveType.Still_0)
            {
                unit.Position = FlyHeightHelper.WithFlyYFromFlyer(unit.Position, unit, self.StartPosition.y);
            }
        }

        /// <summary>Skill_1：创建时立刻打一发创建技能（CreateSkillId，否则旧版 TriggerOnCreate 用 ActionSkillId）。</summary>
        public static void FireCreateSkill(this SkillEntityComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            SummonRuntimeData rt = self.Runtime;
            if (unit == null || rt == null)
            {
                return;
            }

            int skillId = rt.CreateSkillId > 0 ? rt.CreateSkillId : (rt.TriggerOnCreate ? rt.ActionSkillId : 0);
            if (skillId <= 0)
            {
                return;
            }

            Unit target = ResolveTrackTarget(self, unit.GetParent<UnitComponent>(), rt);
            SkillManagerComponentSystem.ExecuteLinkedSkill(skillId, unit, target ?? unit);
        }

        /// <summary>运行中改追踪目标，并同步 Numeric 给客户端。</summary>
        public static void SetTrackTarget(this SkillEntityComponent self, Unit target, bool lockTarget)
        {
            if (self.Runtime == null)
            {
                self.Runtime = new SummonRuntimeData();
            }

            self.Runtime.TrackTargetId = target?.Id ?? 0;
            self.Runtime.LockTarget = lockTarget;
            self.GetParent<Unit>()?.GetComponent<NumericComponent>()
                ?.ApplyValue(NumericType.SkillEntity_TrackTargetId, self.Runtime.TrackTargetId, false);
        }

        /// <summary>
        /// 心跳：飞 → Skill_2 间隔 → Skill_3 追到 → 到期/次数/死亡则 Skill_4 删除。
        /// </summary>
        public static void OnUpdate(this SkillEntityComponent self)
        {
            if (self.BuffState == BuffState.Finished)
            {
                return;
            }

            Unit unit = self.GetParent<Unit>();
            SummonRuntimeData rt = self.Runtime;
            if (unit == null || unit.IsDisposed || rt == null)
            {
                self.BuffState = BuffState.Finished;
                return;
            }

            long now = TimeHelper.ServerNow();
            self.PassTime = now - self.BeginTime;
            UnitComponent uc = unit.GetParent<UnitComponent>();
            Unit master = uc?.Get(self.Masterid);
            Unit trackTarget = ResolveTrackTarget(self, uc, rt);

            Fly(self, unit, rt, trackTarget, master);

            // Skill_2：任意运动类型（静止/直线/追踪）飞行途中都按间隔打
            if (self.InterValTime > 0 && rt.ActionSkillId > 0
                && (rt.MaxActionCount <= 0 || rt.ActionCount < rt.MaxActionCount))
            {
                long endTime = self.BuffEndTime > 0 ? self.BuffEndTime : long.MaxValue;
                if (self.InterValTimeBegin <= now && self.InterValTimeBegin <= endTime)
                {
                    long fireAt = self.InterValTimeBegin;
                    SkillManagerComponentSystem.ExecuteLinkedSkill(rt.ActionSkillId, unit, trackTarget ?? unit);
                    rt.ActionCount++;
                    if (self.BuffState == BuffState.Finished)
                    {
                        return;
                    }

                    if (self.InterValTimeBegin == fireAt)
                    {
                        self.InterValTimeBegin += self.InterValTime;
                    }
                }
            }

            if (self.BuffState == BuffState.Finished)
            {
                return;
            }

            // Skill_3：追到目标一次
            if (rt.MoveType == SkillEntityMoveType.Track_2 && !rt.TrackSkillFired && Reached(unit, trackTarget))
            {
                rt.TrackSkillFired = true;
                if (rt.TrackSkillId > 0)
                {
                    SkillManagerComponentSystem.ExecuteLinkedSkill(rt.TrackSkillId, unit, trackTarget);
                }

                if (self.BuffState == BuffState.Finished)
                {
                    return;
                }

                if (rt.DeleteOnTrackReach)
                {
                    FinishAndRemove(self, unit);
                    return;
                }
            }

            bool timeEnd = self.BuffEndTime > 0 && now >= self.BuffEndTime;
            bool countEnd = rt.DestroyOnCount && rt.MaxActionCount > 0 && rt.ActionCount >= rt.MaxActionCount;
            bool masterDead = rt.DestroyOnMasterDead && IsDead(master);
            bool targetDead = rt.DestroyOnTargetDead && rt.TrackTargetId > 0 && IsDead(trackTarget);
            if (timeEnd || countEnd || masterDead || targetDead)
            {
                FinishAndRemove(self, unit);
            }
        }

        /// <summary>
        /// 按 PassTime * Speed 从出生点算位置。静止不飞；直线沿朝向；追踪沿出生点到目标 XZ，高度走 FlyHeightHelper。
        /// </summary>
        private static void Fly(SkillEntityComponent self, Unit unit, SummonRuntimeData rt, Unit trackTarget, Unit master)
        {
            if (rt.MoveType == SkillEntityMoveType.Still_0)
            {
                return;
            }

            float speed = self.SummonConfig != null && self.SummonConfig.Speed > 0
                ? (float)self.SummonConfig.Speed
                : (unit.GetComponent<NumericComponent>()?.GetAsFloat(NumericType.Speed_Current_15) ?? 1f);
            if (speed <= 0f)
            {
                speed = 1f;
            }

            float traveled = speed * (self.PassTime * 0.001f);
            Vector3 next;
            Vector3 dir;
            float flyY = FlyHeightHelper.GetFlyY(master, self.StartPosition.y);

            if (rt.MoveType == SkillEntityMoveType.Track_2)
            {
                if (trackTarget == null || trackTarget.IsDisposed)
                {
                    return;
                }

                Vector3 start = self.StartPosition;
                float dx = trackTarget.Position.x - start.x;
                float dz = trackTarget.Position.z - start.z;
                float total = (float)Math.Sqrt(dx * dx + dz * dz);
                if (total <= 1e-4f)
                {
                    Vector3 at = trackTarget.Position;
                    at.y = flyY;
                    unit.Position = at;
                    return;
                }

                dir = new Vector3(dx / total, 0f, dz / total);
                float maxTravel = Math.Max(0f, total - CollideReach);
                float move = Math.Min(traveled, maxTravel);
                next = move >= maxTravel ? trackTarget.Position - dir * CollideReach : start + dir * move;
            }
            else
            {
                dir = unit.Rotation * Vector3.forward;
                dir.y = 0f;
                if (dir.sqrMagnitude <= 1e-6f)
                {
                    dir = self.FlyDirection;
                }

                if (dir.sqrMagnitude <= 1e-6f)
                {
                    return;
                }

                dir.Normalize();
                next = self.StartPosition + dir * traveled;
            }

            next.y = flyY;
            unit.Position = next;
            unit.Rotation = Quaternion.LookRotation(dir, Vector3.up);
        }

        /// <summary>Skill_4：停心跳、打消亡技能、从场景删 Unit。</summary>
        private static void FinishAndRemove(SkillEntityComponent self, Unit unit)
        {
            if (self.BuffState == BuffState.Finished)
            {
                return;
            }

            self.BuffState = BuffState.Finished;
            TimerComponent.Instance?.Remove(ref self.Timer);

            int destroySkillId = self.Runtime?.DestroySkillId ?? 0;
            if (destroySkillId > 0)
            {
                Unit target = ResolveTrackTarget(self, unit.GetParent<UnitComponent>(), self.Runtime);
                SkillManagerComponentSystem.ExecuteLinkedSkill(destroySkillId, unit, target ?? unit);
            }

            unit.GetParent<UnitComponent>()?.Remove(unit.Id);
        }

        /// <summary>解析追踪目标：Runtime.TrackTargetId，没有则技能树当前目标。</summary>
        private static Unit ResolveTrackTarget(SkillEntityComponent self, UnitComponent uc, SummonRuntimeData rt)
        {
            if (rt.TrackTargetId > 0)
            {
                Unit t = uc?.Get(rt.TrackTargetId);
                if (t != null && !t.IsDisposed)
                {
                    return t;
                }
            }

            Unit fallback = self.SkillHandler?.TheUnitTarget;
            return fallback != null && !fallback.IsDisposed ? fallback : null;
        }

        /// <summary>XZ 距离是否已进入贴身范围。</summary>
        private static bool Reached(Unit unit, Unit trackTarget)
        {
            return unit != null && trackTarget != null && !trackTarget.IsDisposed
                   && XZSqr(unit.Position, trackTarget.Position) <= CollideReach * CollideReach + 0.01f;
        }

        /// <summary>Unit 为空、已销毁、Now_Dead 或 HP≤0 视为死亡。</summary>
        private static bool IsDead(Unit unit)
        {
            if (unit == null || unit.IsDisposed)
            {
                return true;
            }

            NumericComponent n = unit.GetComponent<NumericComponent>();
            return n != null && (n.GetAsInt(NumericType.Now_Dead) == 1 || n.GetAsLong(NumericType.HP_Current_8) <= 0);
        }

        /// <summary>XZ 平面距离平方，忽略高度。</summary>
        private static float XZSqr(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            return dx * dx + dz * dz;
        }
    }
}
