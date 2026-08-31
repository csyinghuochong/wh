using System;
using UnityEngine;

namespace ET
{
    [Timer(TimerType.SkillSingTimer)]
    public class SkillSingTimer : ATimer<SkillManagerComponent>
    {
        public override void Run(SkillManagerComponent self)
        {
            try
            {
                self.OnMonsterSingComplete();
            }
            catch (Exception e)
            {
                Log.Error($"skill sing timer error: {self.Id}\n{e}");
            }
        }
    }

    /// <summary>
    /// 怪物吟唱：AI 点技能 → 读条 → 到点 OnUseSkill；受击/受控走 InterruptPendingSing。
    /// 玩家吟唱仍由客户端 C2M_SingingUpdate 发起。
    /// </summary>
    public static partial class SkillManagerComponentSystem
    {
        /// <summary>
        /// 吟唱技能则进入读条并返回 true（调用方不要 OnUseSkill）；
        /// 非吟唱返回 false，由调用方立刻 OnUseSkill。
        /// 已在读条中也返回 true，避免重复起条。
        /// </summary>
        public static bool TryBeginMonsterSing(this SkillManagerComponent self, C2M_SkillCmd skillcmd)
        {
            Unit unit = self.GetParent<Unit>();
            if (unit == null || unit.Type != UnitType.Monster || skillcmd == null)
            {
                return false;
            }

            if (self.IsMonsterSinging())
            {
                return true;
            }

            if (!LDSkill_BattleCategory.Instance.Contain(skillcmd.SkillID))
            {
                return false;
            }

            LDSkill_Battle ldSkill = LDSkill_BattleCategory.Instance.Get(skillcmd.SkillID);
            if (!LDSkillHelper.IsSingCast(ldSkill))
            {
                return false;
            }

            unit.Rotation = Quaternion.Euler(0, skillcmd.TargetAngle, 0);
            if (!unit.GetComponent<MoveComponent>().IsArrived())
            {
                unit.Stop(skillcmd.SkillID);
            }

            CopySkillCmd(skillcmd, self.SingSkillCmd);
            self.SingSkillCmd.SingValue = 1f;

            string stateValue = $"{skillcmd.SkillID}_{skillcmd.TargetAngle}";
            int stateTime = (int)(ldSkill.Skill_Time * 1000);

            TimerComponent.Instance?.Remove(ref self.SingTimer);
            self.SingTimer = TimerComponent.Instance.NewOnceTimer(
                    TimeHelper.ServerNow() + stateTime, TimerType.SkillSingTimer, self);
            BroadcastMonsterSinging(unit, stateValue, 1, stateTime);
            return true;
        }

        public static bool IsMonsterSinging(this SkillManagerComponent self)
        {
            return self.SingSkillCmd != null && self.SingSkillCmd.SkillID != 0;
        }

        /// <summary>受击/受控打断前摇。未在吟唱或 Interrupt_2≠1 时不处理。</summary>
        public static void InterruptPendingSing(this SkillManagerComponent self, int skillId = 0)
        {
            if (!self.IsMonsterSinging())
            {
                return;
            }

            int pendingId = self.SingSkillCmd.SkillID;
            if (skillId != 0 && pendingId != skillId)
            {
                return;
            }

            if (!LDSkill_BattleCategory.Instance.Contain(pendingId))
            {
                self.ClearMonsterSing();
                return;
            }

            if (!LDSkillHelper.CanBeInterrupted(LDSkill_BattleCategory.Instance.Get(pendingId)))
            {
                return;
            }

            self.ClearMonsterSing();
        }

        public static void OnMonsterSingComplete(this SkillManagerComponent self)
        {
            self.SingTimer = 0;
            if (self.IsDisposed || !self.IsMonsterSinging())
            {
                return;
            }

            C2M_SkillCmd cmd = new C2M_SkillCmd();
            CopySkillCmd(self.SingSkillCmd, cmd);
            self.ClearMonsterSing();
            self.OnUseSkill(cmd, true);
        }

        /// <summary>清前摇：停表、丢掉指令、广播结束。</summary>
        public static void ClearMonsterSing(this SkillManagerComponent self)
        {
            bool wasSinging = self.IsMonsterSinging();
            TimerComponent.Instance?.Remove(ref self.SingTimer);
            if (self.SingSkillCmd != null)
            {
                self.SingSkillCmd.SkillID = 0;
            }

            if (!wasSinging)
            {
                return;
            }

            Unit unit = self.GetParent<Unit>();
            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            BroadcastMonsterSinging(unit, "0", 2);
        }

        private static void BroadcastMonsterSinging(Unit unit, string stateValue, int operateType, int stateTime = 0)
        {
            MessageHelper.Broadcast(unit, new M2C_SingingUpdate()
            {
                UnitId = unit.Id,
                StateType = SingingUpdateKind.Singing,
                StateValue = stateValue,
                StateOperateType = operateType,
                StateTime = stateTime,
            });
        }

        private static void CopySkillCmd(C2M_SkillCmd from, C2M_SkillCmd to)
        {
            to.SkillID = from.SkillID;
            to.TargetID = from.TargetID;
            to.TargetAngle = from.TargetAngle;
            to.TargetDistance = from.TargetDistance;
            to.WeaponSkillID = from.WeaponSkillID;
            to.ItemId = from.ItemId;
            to.SingValue = from.SingValue;
        }
    }
}
