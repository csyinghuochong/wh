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
                AIComponent aiComponent = theUnitBelongto.GetComponent<AIComponent>();
                StateComponent stateComponent = theUnitBelongto.GetComponent<StateComponent>();
                aiComponent.ChangeTarget(theUnitFrom.Id);
                stateComponent.StateTypeAdd(StateTypeEnum.ChaoFeng);
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
