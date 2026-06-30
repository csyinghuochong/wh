using System;
using UnityEngine;

namespace ET
{
    public class RoleBuffQuSan : BuffHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffData"></param>
        /// <param name="theUnitFrom">buff持有者</param>
        /// <param name="theUnitBelongto">施法者</param>
        /// <param name="skillHandler"></param>
        public override void OnInit(BuffData buffData, Unit theUnitFrom, Unit theUnitBelongto, Skill_TreeEditor skillHandler = null)
        {
            this.OnBaseBuffInit(buffData, theUnitFrom, theUnitBelongto);
            this.BeginTime = TimeHelper.ServerNow();

            BuffManagerComponent buffManager = theUnitFrom.GetComponent<BuffManagerComponent>();
            for (int i = buffManager.m_Buffs.Count - 1; i >= 0; i--)
            {
                LDSkillBuff ldSkillBuff = buffManager.m_Buffs[i].MBuff;
                if (ldSkillBuff.BuffBenefitType == 2)
                {
                    buffManager.m_Buffs[i].BuffState = BuffState.Finished;
                }
            }
        }

        public override void OnUpdate()
        {
            if (TimeHelper.ServerNow() > this.BuffEndTime)
            {
                this.BuffState = BuffState.Finished;
            }
        }

        public override void OnFinished()
        {
        }

    }
}
