using System;

namespace ET
{
    [ActorMessageHandler]
    public class Friend2M_SendApplyHandler : AMActorLocationHandler<Unit, Friend2M_SendApply>
    {
        protected override async ETTask Run(Unit unit, Friend2M_SendApply message)
        {
            unit.GetComponent<TaskComponentServer>()?.OnSendFriendRequest();
            await ETTask.CompletedTask;
        }
    }
}
