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

    public static class SkillEntityComponentSystem
    {
        public static void Init(
            this SkillEntityComponent self,
            Skill_TreeEditor skillHandler,
            long masterId,
            LDSummon summonConfig,
            SummonRuntimeData runtime)
        {
            self.PassTime = 0;
            self.Masterid = masterId;
            self.BuffState = BuffState.Running;
            self.SkillHandler = skillHandler;
            self.SummonConfig = summonConfig;
            self.Runtime = runtime ?? new SummonRuntimeData();
            self.BeginTime = TimeHelper.ServerNow();
            self.LastActionTime = self.BeginTime;
            self.DamageRange = 1f;
            self.DelayTime = 0;

            long durationMs = self.Runtime.MaxDurationMs;
            if (durationMs <= 0)
            {
                durationMs = 60000;
            }

            self.BuffEndTime = self.BeginTime + durationMs;
            self.Timer = TimerComponent.Instance.NewFrameTimer(TimerType.SkillEntityTimer, self);

            Unit unit = self.GetParent<Unit>();
            NumericComponent numeric = unit.GetComponent<NumericComponent>();
            if (numeric != null)
            {
                numeric.Set(NumericType.SkillEntity_MoveType, self.Runtime.MoveType, false);
                numeric.Set(NumericType.SkillEntity_TrackTargetId, self.Runtime.TrackTargetId, false);
            }

            if (self.Runtime.ActionSkillId > 0 && LDSkillCategory.Instance.Contain(self.Runtime.ActionSkillId))
            {
                LDSkill actionSkill = LDSkillCategory.Instance.Get(self.Runtime.ActionSkillId);
                if (actionSkill.Range_Type_Param1 > 0)
                {
                    self.DamageRange = (float)actionSkill.Range_Type_Param1;
                }
            }

            if (self.Runtime.TriggerOnCreate && self.Runtime.ActionSkillId > 0)
            {
                TriggerActionSkill(self, 0);
            }
        }

        public static void OnUpdate(this SkillEntityComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            if (unit == null || unit.IsDisposed)
            {
                self.BuffState = BuffState.Finished;
                return;
            }

            SummonRuntimeData runtime = self.Runtime ?? new SummonRuntimeData();
            long now = TimeHelper.ServerNow();
            self.PassTime = now - self.BeginTime;

            Unit master = unit.GetParent<UnitComponent>()?.Get(self.Masterid);
            if (ShouldDestroyByMasterDeath(runtime, master))
            {
                DestroySkillEntity(self, unit, runtime.DestroySkillId, runtime.DestroySkillLevel);
                return;
            }

            if (now >= self.BuffEndTime)
            {
                DestroySkillEntity(self, unit, runtime.DestroySkillId, runtime.DestroySkillLevel);
                return;
            }

            UpdateMovement(self, unit, runtime, master);

            if (runtime.ActionType == 0)
            {
                TryTriggerIntervalAction(self, unit, runtime, now);
            }
            else
            {
                TryTriggerCollisionAction(self, unit, runtime, now);
            }

            if (runtime.MaxActionCount > 0 && runtime.ActionCount >= runtime.MaxActionCount
                && (runtime.DestroyMode == 1 || runtime.DestroyMode == 11))
            {
                DestroySkillEntity(self, unit, runtime.DestroySkillId, runtime.DestroySkillLevel);
            }
        }

        public static void SetTrackTarget(this SkillEntityComponent self, Unit target, bool lockTarget)
        {
            if (self.Runtime == null)
            {
                self.Runtime = new SummonRuntimeData();
            }

            self.Runtime.TrackTargetId = target?.Id ?? 0;
            self.Runtime.LockTarget = lockTarget;
            NumericComponent numeric = self.GetParent<Unit>()?.GetComponent<NumericComponent>();
            numeric?.Set(NumericType.SkillEntity_TrackTargetId, self.Runtime.TrackTargetId, false);
            if (self.SkillHandler != null && target != null)
            {
                self.SkillHandler.TheUnitTarget = target;
            }
        }

        private static void UpdateMovement(
            SkillEntityComponent self,
            Unit unit,
            SummonRuntimeData runtime,
            Unit master)
        {
            UnitComponent unitComponent = unit.GetParent<UnitComponent>();
            Unit trackTarget = runtime.TrackTargetId > 0 ? unitComponent?.Get(runtime.TrackTargetId) : null;
            if (trackTarget == null && !runtime.LockTarget)
            {
                trackTarget = self.SkillHandler?.TheUnitTarget;
            }

            if (runtime.MoveType == 0 || trackTarget == null || trackTarget.IsDisposed)
            {
                return;
            }

            NumericComponent numeric = unit.GetComponent<NumericComponent>();
            float speed = numeric != null ? numeric.GetAsFloat(NumericType.Speed_Current_15) : 0f;
            if (speed <= 0f)
            {
                // LDSummon.Speed：1000 = 1 米/秒
                speed = self.SummonConfig?.Speed > 0 ? self.SummonConfig.Speed / 1000f : 1f;
            }

            Vector3 dir;
            if (runtime.MoveType == 2)
            {
                dir = trackTarget.Position - unit.Position;
                dir.y = 0f;
                if (dir.sqrMagnitude <= 0.25f)
                {
                    if (runtime.DeleteOnTrackReach)
                    {
                        DestroySkillEntity(self, unit, runtime.DestroySkillId, runtime.DestroySkillLevel);
                    }
                    return;
                }
            }
            else
            {
                dir = unit.Rotation * Vector3.forward;
                dir.y = 0f;
            }

            if (dir.sqrMagnitude <= 1e-6f)
            {
                return;
            }

            dir.Normalize();
            float step = speed * 0.033f;
            Vector3 nextPos = unit.Position + dir * step;

            MapComponent map = unit.DomainScene()?.GetComponent<MapComponent>();
            if (runtime.DeleteOnBlock && map != null)
            {
                Vector3 blocked = map.GetCanChongJiPath(unit, unit.Position, nextPos);
                if ((blocked - nextPos).sqrMagnitude > 0.01f)
                {
                    DestroySkillEntity(self, unit, runtime.DestroySkillId, runtime.DestroySkillLevel);
                    return;
                }
                nextPos = blocked;
            }

            unit.Position = nextPos;
            unit.Rotation = Quaternion.LookRotation(dir, Vector3.up);
        }

        private static void TryTriggerIntervalAction(
            SkillEntityComponent self,
            Unit unit,
            SummonRuntimeData runtime,
            long now)
        {
            if (runtime.ActionSkillId <= 0)
            {
                return;
            }

            long intervalMs = runtime.IntervalMs > 0 ? runtime.IntervalMs : 1000;
            if (now - self.LastActionTime < intervalMs)
            {
                return;
            }

            TriggerActionSkill(self, 0);
            self.LastActionTime = now;
        }

        private static void TryTriggerCollisionAction(
            SkillEntityComponent self,
            Unit unit,
            SummonRuntimeData runtime,
            long now)
        {
            if (runtime.ActionSkillId <= 0 || self.SkillHandler == null)
            {
                return;
            }

            List<Unit> units = unit.GetParent<UnitComponent>()?.GetAll();
            if (units == null)
            {
                return;
            }

            for (int i = units.Count - 1; i >= 0; i--)
            {
                Unit other = units[i];
                if (other == null || other.IsDisposed || other.Id == unit.Id || other.Id == self.Masterid)
                {
                    continue;
                }

                if (!self.SkillHandler.SkillCanAttackUnit(other))
                {
                    continue;
                }

                if ((other.Position - unit.Position).sqrMagnitude > self.DamageRange * self.DamageRange)
                {
                    continue;
                }

                TriggerActionSkill(self, other.Id);
                if (runtime.MaxActionCount > 0 && runtime.ActionCount >= runtime.MaxActionCount)
                {
                    break;
                }
            }
        }

        private static void TriggerActionSkill(SkillEntityComponent self, long targetId)
        {
            SummonRuntimeData runtime = self.Runtime;
            if (runtime == null || runtime.ActionSkillId <= 0)
            {
                return;
            }

            Unit unit = self.GetParent<Unit>();
            Unit master = unit?.GetParent<UnitComponent>()?.Get(self.Masterid);
            if (master == null || master.IsDisposed)
            {
                return;
            }

            if (!LDSkillCategory.Instance.Contain(runtime.ActionSkillId))
            {
                Log.Warning($"SkillEntity action skill missing: {runtime.ActionSkillId} summon={runtime.SummonId}");
                return;
            }

            SkillManagerComponent skillManager = master.GetComponent<SkillManagerComponent>();
            if (skillManager == null)
            {
                return;
            }

            long resolvedTargetId = targetId;
            if (resolvedTargetId <= 0)
            {
                resolvedTargetId = runtime.TrackTargetId > 0
                    ? runtime.TrackTargetId
                    : self.SkillHandler?.TheUnitTarget?.Id ?? 0;
            }

            SkillInfo skillInfo = new SkillInfo
            {
                WeaponSkillID = runtime.ActionSkillId,
                TargetID = resolvedTargetId,
                PosX = unit.Position.x,
                PosY = unit.Position.y,
                PosZ = unit.Position.z,
                TargetAngle = AngleHelper.GetQuaternionAngle(unit.Rotation),
            };

            Skill_TreeEditor handler = skillManager.SkillFactory(skillInfo, master);
            handler.TheUnitTarget = unit.GetParent<UnitComponent>()?.Get(resolvedTargetId);
            handler.CollectSkillTargets();

            if (SkillEditorTreeRegistry.TryGetTree(runtime.ActionSkillId, out SkillEditorSkillLogic logic))
            {
                SkillEditorTreeExecutor.Execute(handler, logic);
            }

            handler.SetSkillState(SkillState.Finished);
            handler.OnFinished();
            ObjectPool.Instance.Recycle(handler);
            runtime.ActionCount++;
        }

        private static bool ShouldDestroyByMasterDeath(SummonRuntimeData runtime, Unit master)
        {
            if (runtime.DestroyMode != 10 && runtime.DestroyMode != 11)
            {
                return false;
            }

            return master == null || master.IsDisposed || IsUnitDead(master);
        }

        private static void DestroySkillEntity(
            SkillEntityComponent self,
            Unit unit,
            int destroySkillId,
            int destroySkillLevel)
        {
            if (self.BuffState == BuffState.Finished)
            {
                return;
            }

            self.BuffState = BuffState.Finished;
            TimerComponent.Instance?.Remove(ref self.Timer);

            if (destroySkillId > 0 && LDSkillCategory.Instance.Contain(destroySkillId))
            {
                SummonRuntimeData runtime = self.Runtime;
                if (runtime != null)
                {
                    int oldActionSkill = runtime.ActionSkillId;
                    runtime.ActionSkillId = destroySkillId;
                    TriggerActionSkill(self, 0);
                    runtime.ActionSkillId = oldActionSkill;
                }
            }

            unit.GetParent<UnitComponent>()?.Remove(unit.Id);
        }

        private static bool IsUnitDead(Unit unit)
        {
            NumericComponent numeric = unit.GetComponent<NumericComponent>();
            if (numeric == null)
            {
                return false;
            }

            return numeric.GetAsInt(NumericType.Now_Dead) == 1
                || numeric.GetAsLong(NumericType.HP_Current_8) <= 0;
        }
    }
}
