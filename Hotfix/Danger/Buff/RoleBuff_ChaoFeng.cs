using System;
using UnityEngine;


namespace ET
{
    public class RoleBuff_ChaoFeng : BuffHandler
    {

        public override void OnInit(BuffData buffData, Unit theUnitFrom, Unit theUnitBelongto, Skill_TreeEditor skillHandler = null)
        {
            this.OnBaseBuffInit(buffData, theUnitFrom, theUnitBelongto);

            if (theUnitBelongto.Type == UnitType.Monster || theUnitBelongto.Type == UnitType.Pet)
            {
                theUnitBelongto.GetComponent<AIComponent>().ChangeTarget( theUnitFrom.Id);
                theUnitBelongto.GetComponent<StateComponent>().StateTypeAdd(StateTypeEnum.ChaoFeng);
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
            if (this.TheUnitBelongto.Type == UnitType.Monster || this.TheUnitBelongto.Type == UnitType.Pet)
            {
                this.TheUnitBelongto.GetComponent<StateComponent>().StateTypeRemove(StateTypeEnum.ChaoFeng);
            }
        }
    }
}
