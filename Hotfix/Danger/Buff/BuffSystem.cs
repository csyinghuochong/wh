using System.Collections.Generic;
using UnityEngine;

namespace ET
{

    /// <summary>
    /// 属性类Buff
    /// </summary>
    public static class BuffSystem 
    {
      


        public static void OnInit(this Buff self,BuffData buffData, Unit theUnitFrom, Skill_TreeEditor skillHandler = null, long intervalMs = 0)
        {
            self.OnBaseBuffInit(buffData,  theUnitFrom, intervalMs);

            self.OnUpdate();
        }

        public static void OnBaseBuffInit(this Buff self, BuffData buffData, Unit theUnitFrom, long intervalMs = 0)
        {
            self.PassTime = 0;
            self.IsTrigger = false;
            self.IsTimeEnd = false;
            self.IsInterrupt = false;
            self.BuffData = buffData;
            self.TheUnitFrom = theUnitFrom;
            self.BuffState = BuffState.Running;
            self.BeginTime = TimeHelper.ServerNow();
            self.MBuff = LDSkill_Battle_BuffCategory.Instance.Get(buffData.BuffId);
            //self.DelayTime = self.MBuff.BuffDelayTime;
            self.BuffEndTime = buffData.BuffEndTime > 0 ? buffData.BuffEndTime : self.BuffEndTime;
            self.InterValTime = intervalMs;
            // 首次作用在 Begin+间隔：0=Init，1000/2000/...=Trigger
            self.InterValTimeBegin = self.BeginTime + (self.InterValTime > 0 ? self.InterValTime : 0);
            self.ApplyBuffControl();
        }

        /// <summary>
        /// 返回毫秒
        /// </summary>
        /// <param name="theUnitBelongto"></param>
        /// <param name="skillBuffConfig"></param>
        /// <returns></returns>
        public static int CheckBuffTime(this Buff self, Unit theUnitBelongto, LDSkill_Battle_Buff ldSkillBuff)
        {

            return 0;
        }


        public static void OnUpdate(this Buff self)
        {
            if (self.BuffState == BuffState.Finished)
            {
                return;
            }

            NumericComponent heroCom = self.TheUnitBelongto.GetComponent<NumericComponent>();
            if (heroCom == null)
            {
                Log.Warning("RoleBuff_Attribute.heroCom == null");
                self.BuffState = BuffState.Finished;
                return;
            }

            long serverTime = TimeHelper.ServerNow();
            self.PassTime = serverTime - self.BeginTime;

            // 到点打一发 Trigger；定时器 100ms，一次只跳一格
            if (self.InterValTime > 0 && self.MBuff != null && self.MBuff.Skill_Trigger > 0)
            {
                long endTime = self.BuffEndTime > 0 ? self.BuffEndTime : long.MaxValue;
                if (self.InterValTimeBegin <= serverTime && self.InterValTimeBegin <= endTime)
                {
                    long fireAt = self.InterValTimeBegin;
                    SkillManagerComponentSystem.ExecuteLinkedSkill(self.MBuff.Skill_Trigger, self.TheUnitFrom, self.TheUnitBelongto);
                    if (self.TheUnitBelongto == null || self.TheUnitBelongto.IsDisposed || self.BuffState == BuffState.Finished)
                    {
                        return;
                    }

                    if (self.InterValTime > 0 && self.InterValTimeBegin == fireAt)
                    {
                        self.InterValTimeBegin += self.InterValTime;
                    }
                }
            }

            if (self.BuffEndTime > 0 && serverTime >= self.BuffEndTime)
            {
                self.IsTimeEnd = true;
                self.BuffState = BuffState.Finished;
            }
        }

        /// <param name="interrupt">被驱散、顶掉、技能移除为 true。切场景 / 死亡传 false。到期看 IsTimeEnd。</param>
        public static void OnFinished(this Buff self, bool interrupt = false)
        {
            self.RemoveBuffControl();
            if (self.MBuff == null)
            {
                return;
            }

            if (self.IsTimeEnd && self.MBuff.Skill_TimeEnd > 0)
            {
                SkillManagerComponentSystem.ExecuteLinkedSkill(self.MBuff.Skill_TimeEnd, self.TheUnitFrom, self.TheUnitBelongto);
            }

            // 到期和被提前清掉都放消失技能（延时结算、属性还原）。切场景、死亡不放。
            if ((self.IsTimeEnd || interrupt) && self.MBuff.Skill_Remove > 0)
            {
                SkillManagerComponentSystem.ExecuteLinkedSkill(self.MBuff.Skill_Remove, self.TheUnitFrom, self.TheUnitBelongto);
            }

            if (!self.IsTrigger)
            {
                return;
            }
        }

        public static void ApplyBuffControl(this Buff self)
        {
            long mask = StateTypeEnum.FromControl(self.MBuff?.Control);
            if (mask == 0)
            {
                return;
            }

            BuffManagerComponent buffManager = self.TheUnitBelongto?.GetComponent<BuffManagerComponent>();
            long immune = buffManager?.GetActiveImmuneMask() ?? 0;
            mask &= ~immune;
            if (mask == 0)
            {
                return;
            }

            if (Log.IsDebugEnabled)
            {
                long remain = self.BuffEndTime - TimeHelper.ServerNow();
                Log.Debug($"ApplyBuffControl unit={self.TheUnitBelongto.Id} buff={self.BuffData.BuffId} mask={mask} remainMs={remain}");
            }

            self.TheUnitBelongto?.GetComponent<StateComponent>()?.StateTypeAdd(mask);
        }

        public static void RemoveBuffControl(this Buff self)
        {
            long mask = StateTypeEnum.FromControl(self.MBuff?.Control);
            if (mask == 0 || self.TheUnitBelongto == null)
            {
                return;
            }

            long still = 0;
            BuffManagerComponent buffManager = self.TheUnitBelongto.GetComponent<BuffManagerComponent>();
            if (buffManager != null)
            {
                for (int i = 0; i < buffManager.m_Buffs.Count; i++)
                {
                    Buff other = buffManager.m_Buffs[i];
                    if (other == null || other.Id == self.Id)
                    {
                        continue;
                    }

                    still |= StateTypeEnum.FromControl(other.MBuff?.Control);
                }
            }

            long remove = mask & ~still;
            if (remove != 0)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug($"RemoveBuffControl unit={self.TheUnitBelongto.Id} buff={self.BuffData.BuffId} remove={remove} still={still}");
                }
                self.TheUnitBelongto.GetComponent<StateComponent>()?.StateTypeRemove(remove);
            }
        }
    }
}
