using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ActivityInfoHandler : AMActorLocationRpcHandler<Unit, C2M_ActivityInfoRequest, M2C_ActivityInfoResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ActivityInfoRequest request, M2C_ActivityInfoResponse response, Action reply)
        {
            response.ActivityInfo = unit.GetComponent<ActivityComponentServer>().ActivityInfo;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
