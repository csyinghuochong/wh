using System;

namespace ET
{

    /// <summary>
    /// 置空buff
    /// </summary>
    public class RoleBuff_Bounce : BuffHandler
    {
        public override void OnInit(BuffData buffData, Unit theUnitFrom, Unit theUnitBelongto, Skill_TreeEditor skillHandler = null)
        {
            this.OnBaseBuffInit(buffData, theUnitFrom, theUnitBelongto);
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
            StateComponent stateComponent = this.TheUnitBelongto.GetComponent<StateComponent>();
        }
    }
}
