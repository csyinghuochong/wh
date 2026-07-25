using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    [Timer(TimerType.SkillEntityTimer)]
    public class SkillEntityTimer : ATimer<SkillEntityComponent>
    {
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

    [ObjectSystem]
    public class SkillEntityComponentAwake : AwakeSystem<SkillEntityComponent>
    {
        public override void Awake(SkillEntityComponent self)
        {
        }
    }

    [ObjectSystem]
    public class SkillEntityComponentDestroy : DestroySystem<SkillEntityComponent>
    {
        public override void Destroy(SkillEntityComponent self)
        {
            TimerComponent.Instance?.Remove(ref self.Timer);
        }
    }

    /// <summary>
    /// 技能体：按 Summon.Speed 飞向 1535 指定目标 → 碰撞后释放 Summon.Skill_1 →
    /// Skill_1 以目标为圆心（Base_Position=1）按 Range_Type_Param1 范围造成伤害。
    /// </summary>
    public static class SkillEntityComponentSystem
    {
        /// <summary>与目标 XZ 贴身判定（米），仅用于「碰到」再放 Skill_1；与客户端一致</summary>
        private const float CollideReach = 0.5f;

        public static void Init(
            this SkillEntityComponent self,
            Skill_TreeEditor skillHandler,
            long masterId,
            LDSummon summonConfig,
            SummonRuntimeData runtime)
        {
            long now = TimeHelper.ServerNow();
            self.PassTime = 0;
            self.Masterid = masterId;
            self.BuffState = BuffState.Running;
            self.SkillHandler = skillHandler;
            self.SummonConfig = summonConfig;
            self.Runtime = runtime ?? new SummonRuntimeData();
            self.DelayTime = 0;
            self.BeginTime = now;
            self.LastActionTime = now;
            self.LastUpdateTime = now;

            Unit unit = self.GetParent<Unit>();
            self.StartPosition = unit != null ? unit.Position : default;
            self.FlyDirection = unit != null ? (unit.Rotation * UnityEngine.Vector3.forward) : UnityEngine.Vector3.forward;
            self.FlyDirection.y = 0f;
            if (self.FlyDirection.sqrMagnitude > 1e-6f)
            {
                self.FlyDirection.Normalize();
            }
            else
            {
                self.FlyDirection = UnityEngine.Vector3.forward;
            }

            // 碰撞后必放表 Skill_1
            if (summonConfig != null && summonConfig.Skill_1 > 0)
            {
                self.Runtime.ActionSkillId = summonConfig.Skill_1;
            }

            long durationMs = self.Runtime.MaxDurationMs > 0 ? self.Runtime.MaxDurationMs : 60000;
            self.BuffEndTime = now + durationMs;

            // 伤害范围仅作非追踪兜底；真正 AOE 半径读 Skill_1.Range_Type_Param1
            self.DamageRange = CollideReach;
            if (self.Runtime.ActionSkillId > 0 && LDSkillCategory.Instance.Contain(self.Runtime.ActionSkillId))
            {
                LDSkill actionSkill = LDSkillCategory.Instance.Get(self.Runtime.ActionSkillId);
                if (actionSkill.Range_Type_Param1 > 0)
                {
                    self.DamageRange = (float)actionSkill.Range_Type_Param1;
                }
            }

            self.Timer = TimerComponent.Instance.NewFrameTimer(TimerType.SkillEntityTimer, self);

            NumericComponent numeric = unit?.GetComponent<NumericComponent>();
            numeric?.ApplyValue(NumericType.SkillEntity_MoveType, self.Runtime.MoveType, false);
            numeric?.ApplyValue(NumericType.SkillEntity_TrackTargetId, self.Runtime.TrackTargetId, false);
            // 与客户端共用同一 BeginTime
            numeric?.ApplyValue(NumericType.StartTime, now, false);

            Log.Info(
                $"SkillEntity Init unit={unit?.Id} summon={self.Runtime.SummonId} skill_1={self.Runtime.ActionSkillId} " +
                $"move={self.Runtime.MoveType} track={self.Runtime.TrackTargetId} speed={summonConfig?.Speed}");
        }

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

            if (rt.ActionSkillId <= 0 && self.SummonConfig != null && self.SummonConfig.Skill_1 > 0)
            {
                rt.ActionSkillId = self.SummonConfig.Skill_1;
            }

            long now = TimeHelper.ServerNow();
            self.PassTime = now - self.BeginTime;
            UnitComponent uc = unit.GetParent<UnitComponent>();
            Unit master = uc?.Get(self.Masterid);
            Unit trackTarget = ResolveTrackTarget(self, uc, rt);

            if (NeedDestroyByMasterDead(rt, master) || now >= self.BuffEndTime)
            {
                FinishAndRemove(self, unit, rt.DestroySkillId);
                return;
            }

            float dt = CalcDt(self, now);
            self.LastUpdateTime = now;

            // —— 飞行（按 BeginTime 时间轴，与客户端同一公式）——
            Fly(self, unit, rt, trackTarget);

            // —— 碰撞后放 Skill_1 ——
            if (rt.ActionType == SkillEntityActionType.Interval_0)
            {
                TryIntervalFire(self, rt, now);
            }
            else
            {
                TryCollideFire(self, unit, rt, trackTarget, master, uc);
            }

            if (rt.MaxActionCount > 0 && rt.ActionCount >= rt.MaxActionCount
                && (rt.DestroyMode == SkillEntityDestroyMode.OnActionCount_1
                    || rt.DestroyMode == SkillEntityDestroyMode.OnActionCountOrMasterDead_11))
            {
                FinishAndRemove(self, unit, rt.DestroySkillId);
            }
        }

        // ==================== 飞行 ====================

        private static void Fly(SkillEntityComponent self, Unit unit, SummonRuntimeData rt, Unit trackTarget)
        {
            if (rt.MoveType == SkillEntityMoveType.Still_0)
            {
                return;
            }

            float speed = GetFlySpeed(self, unit);
            float traveled = speed * (self.PassTime * 0.001f);

            if (rt.MoveType == SkillEntityMoveType.Track_2)
            {
                if (trackTarget == null || trackTarget.IsDisposed)
                {
                    return;
                }

                // 与客户端同一公式：从出生点沿当前目标方向飞 traveled，贴身后停
                Vector3 start = self.StartPosition;
                float dx = trackTarget.Position.x - start.x;
                float dz = trackTarget.Position.z - start.z;
                float total = (float)Math.Sqrt(dx * dx + dz * dz);
                if (total <= 1e-4f)
                {
                    unit.Position = new Vector3(trackTarget.Position.x, trackTarget.Position.y, trackTarget.Position.z);
                    return;
                }

                Vector3 dir = new Vector3(dx / total, 0f, dz / total);
                float maxTravel = Math.Max(0f, total - CollideReach);
                float move = Math.Min(traveled, maxTravel);
                Vector3 next = start + dir * move;
                next.y = trackTarget.Position.y;
                ApplyMove(self, unit, rt, next, dir);
                return;
            }

            // 直线：出生点 + 朝向 * traveled
            Vector3 forward = unit.Rotation * Vector3.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude <= 1e-6f)
            {
                forward = self.FlyDirection;
            }

            if (forward.sqrMagnitude <= 1e-6f)
            {
                return;
            }

            forward.Normalize();
            Vector3 straight = self.StartPosition + forward * (speed * (self.PassTime * 0.001f));
            straight.y = unit.Position.y;
            ApplyMove(self, unit, rt, straight, forward);
        }

        private static void ApplyMove(SkillEntityComponent self, Unit unit, SummonRuntimeData rt, Vector3 next, Vector3 dir)
        {
            if (rt.DeleteOnBlock)
            {
                MapComponent map = unit.DomainScene()?.GetComponent<MapComponent>();
                if (map != null)
                {
                    Vector3 blocked = map.GetCanChongJiPath(unit, unit.Position, next);
                    if ((blocked - next).sqrMagnitude > 0.01f)
                    {
                        FinishAndRemove(self, unit, rt.DestroySkillId);
                        return;
                    }

                    next = blocked;
                }
            }

            unit.Position = next;
            unit.Rotation = Quaternion.LookRotation(dir, Vector3.up);
        }

        private static float GetFlySpeed(SkillEntityComponent self, Unit unit)
        {
            // 优先 LDSummon.Speed（1000=1m/s），与表一致
            if (self.SummonConfig != null && self.SummonConfig.Speed > 0)
            {
                return self.SummonConfig.Speed / 1000f;
            }

            float speed = unit.GetComponent<NumericComponent>()?.GetAsFloat(NumericType.Speed_Current_15) ?? 0f;
            return speed > 0f ? speed : 1f;
        }

        private static float CalcDt(SkillEntityComponent self, long now)
        {
            float dt = (now - self.LastUpdateTime) * 0.001f;
            if (dt <= 0f)
            {
                return 0.1f;
            }

            return dt > 0.25f ? 0.25f : dt;
        }

        // ==================== 触发 Skill_1 ====================

        private static void TryIntervalFire(SkillEntityComponent self, SummonRuntimeData rt, long now)
        {
            if (rt.ActionSkillId <= 0)
            {
                return;
            }

            long interval = rt.IntervalMs > 0 ? rt.IntervalMs : 1000;
            if (now - self.LastActionTime < interval)
            {
                return;
            }

            FireSkill1(self, rt.TrackTargetId);
            self.LastActionTime = now;
        }

        private static void TryCollideFire(
            SkillEntityComponent self,
            Unit unit,
            SummonRuntimeData rt,
            Unit trackTarget,
            Unit master,
            UnitComponent uc)
        {
            if (rt.ActionSkillId <= 0)
            {
                return;
            }

            // 追踪：必须碰到指定目标
            if (rt.MoveType == SkillEntityMoveType.Track_2)
            {
                if (trackTarget == null || trackTarget.IsDisposed || trackTarget.Id == self.Masterid)
                {
                    return;
                }

                if (XZSqr(unit.Position, trackTarget.Position) > CollideReach * CollideReach)
                {
                    return;
                }

                FireSkill1(self, trackTarget.Id);
                return;
            }

            // 非追踪：碰到任意可攻击单位
            List<Unit> all = uc?.GetAll();
            if (all == null || master == null)
            {
                return;
            }

            float rangeSq = CollideReach * CollideReach;
            for (int i = all.Count - 1; i >= 0; i--)
            {
                Unit other = all[i];
                if (other == null || other.IsDisposed || other.Id == unit.Id || other.Id == self.Masterid)
                {
                    continue;
                }

                if (XZSqr(unit.Position, other.Position) > rangeSq)
                {
                    continue;
                }

                if (!master.IsCanAttackUnit(other, false, false))
                {
                    continue;
                }

                FireSkill1(self, other.Id);
                if (rt.MaxActionCount > 0 && rt.ActionCount >= rt.MaxActionCount)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 释放 Summon.Skill_1：圆心=碰撞目标，半径=Skill.Range_Type_Param1（153501 为 3）。
        /// </summary>
        public static void FireSkill1(this SkillEntityComponent self, long hitTargetId)
        {
            SummonRuntimeData rt = self.Runtime;
            Unit skillEntity = self.GetParent<Unit>();
            UnitComponent uc = skillEntity?.GetParent<UnitComponent>();
            Unit master = uc?.Get(self.Masterid);

            if (rt == null || skillEntity == null || master == null || master.IsDisposed)
            {
                Log.Error($"FireSkill1 abort master/unit missing masterId={self.Masterid}");
                return;
            }

            if (rt.ActionSkillId <= 0 && self.SummonConfig != null)
            {
                rt.ActionSkillId = self.SummonConfig.Skill_1;
            }

            int skill1 = rt.ActionSkillId;
            if (skill1 <= 0 || !LDSkillCategory.Instance.Contain(skill1))
            {
                Log.Error($"FireSkill1 abort skill_1 invalid={skill1} summon={rt.SummonId}");
                return;
            }

            SkillManagerComponent skillManager = master.GetComponent<SkillManagerComponent>();
            if (skillManager == null)
            {
                return;
            }

            long tid = hitTargetId > 0 ? hitTargetId : rt.TrackTargetId;
            Unit hitTarget = tid > 0 ? uc.Get(tid) : null;
            // Base_Position=1：范围圆心在目标
            Vector3 center = hitTarget != null ? hitTarget.Position : skillEntity.Position;
            LDSkill actionSkill = LDSkillCategory.Instance.Get(skill1);
            float aoeRadius = actionSkill.Range_Type_Param1 > 0 ? (float)actionSkill.Range_Type_Param1 : 3f;

            SkillInfo skillInfo = new SkillInfo
            {
                SkillID = skill1,
                WeaponSkillID = skill1,
                TargetID = tid,
                PosX = center.x,
                PosY = center.y,
                PosZ = center.z,
                TargetAngle = AngleHelper.GetQuaternionAngle(skillEntity.Rotation),
            };

            // TheUnitFrom=技能体 → 树里 caster.parent = 主人
            Skill_TreeEditor handler = skillManager.SkillFactory(skillInfo, skillEntity);
            handler.TheUnitTarget = hitTarget;
            handler.TargetPosition = center;
            handler.ICheckShape.Clear();
            handler.ICheckShape.Add(handler.CreateCheckShape(skillInfo.TargetAngle));
            handler.HurtIds.Clear();

            // 收集目标：圆心=碰撞目标，半径=Skill_1 范围参数1（3）
            CollectAoeTargets(handler, master, uc, actionSkill, center, aoeRadius, tid);

            if (SkillEditorTreeRegistry.TryGetTree(skill1, out SkillEditorSkillLogic logic))
            {
                SkillEditorTreeExecutor.Execute(handler, logic);
            }
            else
            {
                Log.Error($"FireSkill1 tree missing skill_1={skill1}");
            }

            int hurtCount = handler.HurtIds?.Count ?? 0;
            handler.SetSkillState(SkillState.Finished);
            handler.OnFinished();
            ObjectPool.Instance.Recycle(handler);
            rt.ActionCount++;

            Log.Info(
                $"FireSkill1 ok entity={skillEntity.Id} skill_1={skill1} hit={tid} " +
                $"aoeR={aoeRadius} hurtCount={hurtCount} actionCount={rt.ActionCount}");
        }

        /// <summary>以 center 为圆心、radius 为半径（XZ），用主人阵营筛敌，填入 HurtIds 供 for_root 遍历。</summary>
        private static void CollectAoeTargets(
            Skill_TreeEditor handler,
            Unit master,
            UnitComponent uc,
            LDSkill actionSkill,
            Vector3 center,
            float radius,
            long primaryTargetId)
        {
            float radiusSq = radius * radius;
            List<Unit> all = uc.GetAll();
            for (int i = 0; i < all.Count; i++)
            {
                Unit u = all[i];
                if (u == null || u.IsDisposed || u.Id == master.Id)
                {
                    continue;
                }

                if (XZSqr(center, u.Position) > radiusSq)
                {
                    continue;
                }

                if (!LDSkillHelper.IsValidTarget(master, u, actionSkill))
                {
                    continue;
                }

                handler.OnAddHurtIds(u.Id);
            }

            // 保证主目标一定进列表
            if (primaryTargetId > 0 && !handler.HurtIds.Contains(primaryTargetId))
            {
                handler.OnAddHurtIds(primaryTargetId);
            }
        }

        // ==================== 工具 ====================

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

        private static float XZSqr(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            return dx * dx + dz * dz;
        }

        private static bool NeedDestroyByMasterDead(SummonRuntimeData rt, Unit master)
        {
            if (rt.DestroyMode != SkillEntityDestroyMode.OnMasterDead_10
                && rt.DestroyMode != SkillEntityDestroyMode.OnActionCountOrMasterDead_11)
            {
                return false;
            }

            if (master == null || master.IsDisposed)
            {
                return true;
            }

            NumericComponent n = master.GetComponent<NumericComponent>();
            return n != null && (n.GetAsInt(NumericType.Now_Dead) == 1 || n.GetAsLong(NumericType.HP_Current_8) <= 0);
        }

        private static void FinishAndRemove(SkillEntityComponent self, Unit unit, int destroySkillId)
        {
            if (self.BuffState == BuffState.Finished)
            {
                return;
            }

            self.BuffState = BuffState.Finished;
            TimerComponent.Instance?.Remove(ref self.Timer);

            if (destroySkillId > 0 && LDSkillCategory.Instance.Contain(destroySkillId))
            {
                int old = self.Runtime.ActionSkillId;
                self.Runtime.ActionSkillId = destroySkillId;
                FireSkill1(self, self.Runtime.TrackTargetId);
                self.Runtime.ActionSkillId = old;
            }

            unit.GetParent<UnitComponent>()?.Remove(unit.Id);
        }
    }
}
