using System.Collections.Generic;
using UnityEngine;

namespace ET
{

    /// <summary>
    /// 属性类Buff
    /// </summary>
    public static class BuffSystem 
    {
      


        public static void OnInit(this Buff self,BuffData buffData, Unit theUnitFrom, Unit theUnitBelongto, Skill_TreeEditor skillHandler = null)
        {
            self.OnBaseBuffInit(buffData,  theUnitFrom, theUnitBelongto);

            self.OnUpdate();
        }

        public static void OnBaseBuffInit(this Buff self, BuffData buffData, Unit theUnitFrom, Unit theUnitBelongto)
        {
            self.PassTime = 0;
            self.IsTrigger = false;
            self.BuffData = buffData;
            self.TheUnitFrom = theUnitFrom;
            self.TheUnitBelongto = theUnitBelongto;
            self.BuffState = BuffState.Running;
            self.BeginTime = TimeHelper.ServerNow();
            self.MLdSkillConf = LDSkill_BattleCategory.Instance.Get(buffData.SkillId);
            self.MBuff = LDSkill_Battle_BuffCategory.Instance.Get(buffData.BuffId);
            //self.DelayTime = self.MBuff.BuffDelayTime;
            self.BuffEndTime = buffData.BuffEndTime > 0 ? buffData.BuffEndTime : self.BuffEndTime;
            //self.InterValTime = self.MBuff.BuffLoopTime * 1000;
            self.InterValTimeBegin = TimeHelper.ServerNow();
            self.NowBuffValue = 0f;
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
            NumericComponent heroCom = self.TheUnitBelongto.GetComponent<NumericComponent>();
            if (heroCom == null)
            {
                Log.Warning("RoleBuff_Attribute.heroCom == null");
                self.BuffState = BuffState.Finished;
                return;
            }

            long serverTime = TimeHelper.ServerNow();
            self.PassTime = serverTime - self.BeginTime;

            //buff是否为循环触发的
            if (self.InterValTime > 0)
            {
                long InterValTimePass = serverTime - self.InterValTimeBegin;
                if (InterValTimePass >= self.InterValTime)
                {
                    self.InterValTimeBegin = serverTime;
                    self.IsTrigger = false;
                }
            }

            //执行buff
            if (!self.IsTrigger && self.PassTime >= self.DelayTime)
            {
                ///移动才触发
              
            }

            //buff执行结束
            if (serverTime >= self.BuffEndTime)
            {
                self.BuffState = BuffState.Finished;
            }
        }

        public static  void OnFinished(this Buff self)
        {
            self.RemoveBuffControl();
            if (!self.IsTrigger)
            {
                return;
            }

            /*
            //移除相关属性
            switch (this.MBuff.BuffType)
            {
                case 1:
                    //Log.Debug("执行buff移除属性...");
                    int NowBuffParameterType = this.MBuff.buffParameterType;
                    if (NowBuffParameterType == 3001)
                    {
                        //血量不进行移除
                    }
                    else if (NowBuffParameterType == 3164)
                    {
                        this.TheUnitBelongto.GetComponent<NumericComponent>().ApplyValue(NowBuffParameterType, 0);
                    }
                    else if (NowBuffParameterType == 3134)
                    {
                        //怒气不进行移除
                    }
                    else
                    {
                        int ValueType = this.MBuff.buffParameterValueDef;      //0 表示整数  1表示浮点数

                        //整数
                        if (ValueType == 0)
                        {
                            // FightBuffNumericHelper removed; use NumericComponent.ChangeAttrFixed/Percent. Was: FightBuffNumericHelper.BuffPropertyUpdate_Long(this.TheUnitBelongto, NowBuffParameterType, (long)this.NowBuffValue * -1);
                        }

                        //浮点数
                        if (ValueType == 1)
                        {
                            // FightBuffNumericHelper removed; use NumericComponent.ChangeAttrFixed/Percent. Was: FightBuffNumericHelper.BuffPropertyUpdate_Float(this.TheUnitBelongto, NowBuffParameterType, (float)this.NowBuffValue * -1);
                        }
                    }
                    break;
                case 2:
                    NowBuffParameterType = this.MBuff.buffParameterType;
                    this.TheUnitBelongto.GetComponent<StateComponent>().StateTypeRemove(1<<NowBuffParameterType);
                    break;
                case 4:
                    this.TheUnitBelongto.GetComponent<SkillPassiveComponent>().RemovePassiveSkill(this.MBuff.buffParameterType);
                    break;
                case 7:
                    break;
                default:
                    break;
            }
            */
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
