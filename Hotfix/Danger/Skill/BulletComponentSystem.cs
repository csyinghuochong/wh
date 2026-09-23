using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    [Timer(TimerType.BulletTimer)]
    public class BulletTimer : ATimer<BulletComponent>
    {
        public override void Run(BulletComponent self)
        {
            try
            {
                self.OnUpdate();
            }
            catch (Exception e)
            {
                Log.Error($"Bullet timer error: {self.Id}\n{e}");
            }
        }
    }

    [ObjectSystem]
    public class BulletComponentAwake : AwakeSystem<BulletComponent>
    {
        public override void Awake(BulletComponent self)
        {
        }
    }

    [ObjectSystem]
    public class BulletComponentDestroy : DestroySystem<BulletComponent>
    {
        public override void Destroy(BulletComponent self)
        {
            TimerComponent.Instance?.Remove(ref self.Timer);
        }
    }

    /// <summary>
    /// 子弹：Time_1 只负责发射。有目标追踪至碰撞，无目标直线飞行并可碰敌；命中后再选目标并执行技能树（对齐技能体 FireSkill1）。无目标飞到 Bullet_Time_Max 未命中则删除。
    /// </summary>
    public static class BulletComponentSystem
    {
        private const float CollideReach = 0.5f;

        public static void Init(this BulletComponent self, long masterId, LDSkill_Battle ldSkill, long targetId, SkillInfo skillInfo)
        {
            long now = TimeHelper.ServerNow();
            self.LdSkill = ldSkill;
            self.MasterId = masterId;
            self.TrackTargetId = targetId;
            self.SkillId = ldSkill != null ? ldSkill.Id : 0;
            self.WeaponSkillId = skillInfo != null ? skillInfo.WeaponSkillID : self.SkillId;
            if (self.WeaponSkillId <= 0)
            {
                self.WeaponSkillId = self.SkillId;
            }

            self.TargetAngle = skillInfo != null ? skillInfo.TargetAngle : 0;
            self.PosX = skillInfo != null ? skillInfo.PosX : 0f;
            self.PosY = skillInfo != null ? skillInfo.PosY : 0f;
            self.PosZ = skillInfo != null ? skillInfo.PosZ : 0f;
            self.Speed = LDSkillHelper.GetBulletSpeed(ldSkill);
            self.BeginTime = now;
            self.PassTime = 0;
            self.BuffState = BuffState.Running;
            self.EndTime = now + LDSkillHelper.GetBulletLifeMs(ldSkill);

            Unit unit = self.GetParent<Unit>();
            self.StartPosition = unit != null ? unit.Position : default;
            self.FlyDirection = unit != null ? (unit.Rotation * Vector3.forward) : Vector3.forward;
            self.FlyDirection.y = 0f;
            if (self.FlyDirection.sqrMagnitude > 1e-6f)
            {
                self.FlyDirection.Normalize();
            }
            else
            {
                self.FlyDirection = Vector3.forward;
            }

            NumericComponent numeric = unit?.GetComponent<NumericComponent>();
            int moveType = targetId > 0 ? SkillEntityMoveType.Track_2 : SkillEntityMoveType.Straight_1;
            numeric?.ApplyValue(NumericType.SkillEntity_MoveType, moveType, false);
            numeric?.ApplyValue(NumericType.SkillEntity_TrackTargetId, targetId, false);
            numeric?.ApplyValue(NumericType.SkillEntity_StartTime, now, false);

            self.Timer = TimerComponent.Instance.NewFrameTimer(TimerType.BulletTimer, self);

            if (unit != null)
            {
                unit.Position = FlyHeightHelper.WithFlyYFromFlyer(unit.Position, unit, self.StartPosition.y);
            }

            if (Log.IsDebugEnabled)
            {
                Log.Debug($"Bullet Init unit={unit?.Id} skill={self.SkillId} speed={self.Speed} track={targetId} lifeMs={self.EndTime - now}");
            }
        }

        public static void OnUpdate(this BulletComponent self)
        {
            if (self.BuffState == BuffState.Finished)
            {
                return;
            }

            Unit unit = self.GetParent<Unit>();
            if (unit == null || unit.IsDisposed)
            {
                self.BuffState = BuffState.Finished;
                return;
            }

            long now = TimeHelper.ServerNow();
            self.PassTime = now - self.BeginTime;
            UnitComponent uc = unit.GetParent<UnitComponent>();
            Unit master = uc?.Get(self.MasterId);
            Unit trackTarget = ResolveTrackTarget(self, uc);

            Fly(self, unit, trackTarget);

            if (TryCollideFire(self, unit, trackTarget, master, uc))
            {
                return;
            }

            if (self.TrackTargetId > 0)
            {
                if (trackTarget == null || trackTarget.IsDisposed)
                {
                    FinishAndRemove(self, unit);
                }

                return;
            }

            if (now >= self.EndTime)
            {
                FinishAndRemove(self, unit);
            }
        }

        private static void Fly(BulletComponent self, Unit unit, Unit trackTarget)
        {
            float traveled = self.Speed * (self.PassTime * 0.001f);
            Unit master = FlyHeightHelper.GetPerson(unit);
            float flyY = FlyHeightHelper.GetFlyY(master, self.StartPosition.y);

            if (self.TrackTargetId > 0)
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

                Vector3 dir = new Vector3(dx / total, 0f, dz / total);
                float maxTravel = Math.Max(0f, total - CollideReach);
                float move = Math.Min(traveled, maxTravel);
                Vector3 next = move >= maxTravel
                    ? trackTarget.Position - dir * CollideReach
                    : start + dir * move;
                next.y = flyY;
                unit.Position = next;
                unit.Rotation = Quaternion.LookRotation(dir, Vector3.up);
                return;
            }

            Vector3 forward = self.FlyDirection;
            if (forward.sqrMagnitude <= 1e-6f)
            {
                forward = unit.Rotation * Vector3.forward;
                forward.y = 0f;
            }

            if (forward.sqrMagnitude <= 1e-6f)
            {
                return;
            }

            forward.Normalize();
            Vector3 straight = self.StartPosition + forward * traveled;
            straight.y = flyY;
            unit.Position = straight;
            unit.Rotation = Quaternion.LookRotation(forward, Vector3.up);
        }

        /// <summary>对齐技能体 TryCollideFire：碰到再结算，不在发射时 CollectSkillTargets。</summary>
        private static bool TryCollideFire(BulletComponent self, Unit unit, Unit trackTarget, Unit master, UnitComponent uc)
        {
            float rangeSq = CollideReach * CollideReach + 0.01f;
            if (self.TrackTargetId > 0)
            {
                if (trackTarget == null || trackTarget.IsDisposed || trackTarget.Id == self.MasterId)
                {
                    return false;
                }

                if (XZSqr(unit.Position, trackTarget.Position) > rangeSq)
                {
                    return false;
                }

                FireOnHit(self, trackTarget);
                FinishAndRemove(self, unit);
                return true;
            }

            if (uc == null || master == null || master.IsDisposed)
            {
                return false;
            }

            List<Unit> all = uc.GetAll();
            for (int i = all.Count - 1; i >= 0; i--)
            {
                Unit other = all[i];
                if (other == null || other.IsDisposed || other.Id == unit.Id || other.Id == self.MasterId)
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

                FireOnHit(self, other);
                FinishAndRemove(self, unit);
                return true;
            }

            return false;
        }

        /// <summary>命中后放技能树：TheUnitFrom=子弹 → 树里 caster.parent=主人，对齐技能体 FireSkill1。</summary>
        private static void FireOnHit(BulletComponent self, Unit hitTarget)
        {
            Unit bullet = self.GetParent<Unit>();
            UnitComponent uc = bullet?.GetParent<UnitComponent>();
            Unit master = uc?.Get(self.MasterId);
            if (bullet == null || master == null || master.IsDisposed || hitTarget == null || hitTarget.IsDisposed)
            {
                return;
            }

            int skillId = self.WeaponSkillId > 0 ? self.WeaponSkillId : self.SkillId;
            if (skillId <= 0 || !LDSkill_BattleCategory.Instance.Contain(skillId))
            {
                return;
            }

            SkillManagerComponent skillManager = master.GetComponent<SkillManagerComponent>();
            if (skillManager == null)
            {
                return;
            }

            LDSkill_Battle actionSkill = LDSkill_BattleCategory.Instance.Get(skillId);
            Vector3 center = hitTarget.Position;
            float aoeRadius = actionSkill.Range_Type_Param1 > 0 ? (float)actionSkill.Range_Type_Param1 : 3f;
            SkillInfo skillInfo = new SkillInfo
            {
                SkillID = skillId,
                WeaponSkillID = skillId,
                TargetID = hitTarget.Id,
                PosX = center.x,
                PosY = center.y,
                PosZ = center.z,
                TargetAngle = AngleHelper.GetQuaternionAngle(bullet.Rotation),
            };

            // TheUnitFrom=子弹 → 树里 caster.parent = 主人
            Skill_TreeEditor handler = skillManager.SkillFactory(skillInfo, bullet);
            handler.TheUnitTarget = hitTarget;
            handler.ActionPosition = center;
            handler.ICheckShape = handler.CreateCheckShape(skillInfo.TargetAngle);
            handler.HurtIds.Clear();
            CollectAoeTargets(handler, master, uc, actionSkill, center, aoeRadius, hitTarget.Id);

            if (SkillEditorTreeRegistry.TryGetTree(skillId, out SkillEditorSkillLogic logic))
            {
                SkillEditorTreeExecutor.Execute(handler, logic);
            }

            int hurtCount = handler.HurtIds?.Count ?? 0;
            handler.SetSkillState(SkillState.Finished);
            handler.OnFinished();
            ObjectPool.Instance.Recycle(handler);

            if (Log.IsDebugEnabled)
            {
                Log.Debug($"Bullet hit entity={bullet.Id} skill={skillId} target={hitTarget.Id} hurtCount={hurtCount}");
            }
        }

        private static void CollectAoeTargets(
            Skill_TreeEditor handler,
            Unit master,
            UnitComponent uc,
            LDSkill_Battle actionSkill,
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

            if (primaryTargetId > 0 && !handler.HurtIds.Contains(primaryTargetId))
            {
                handler.OnAddHurtIds(primaryTargetId);
            }
        }

        private static Unit ResolveTrackTarget(BulletComponent self, UnitComponent uc)
        {
            if (self.TrackTargetId <= 0)
            {
                return null;
            }

            Unit t = uc?.Get(self.TrackTargetId);
            return t != null && !t.IsDisposed ? t : null;
        }

        private static float XZSqr(UnityEngine.Vector3 a, UnityEngine.Vector3 b)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            return dx * dx + dz * dz;
        }

        private static void FinishAndRemove(BulletComponent self, Unit unit)
        {
            if (self.BuffState == BuffState.Finished)
            {
                return;
            }

            self.BuffState = BuffState.Finished;
            TimerComponent.Instance?.Remove(ref self.Timer);
            unit.GetParent<UnitComponent>()?.Remove(unit.Id);
        }
    }
}
