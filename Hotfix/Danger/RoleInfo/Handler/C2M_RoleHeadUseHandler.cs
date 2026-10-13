using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_RoleHeadUseHandler : AMActorLocationRpcHandler<Unit, C2M_RoleHeadUseRequest, M2C_RoleHeadUseResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_RoleHeadUseRequest request, M2C_RoleHeadUseResponse response, Action reply)
        {
            unit.GetComponent<RoleInfoComponentServer>().RoleInfo.RoleHeadId = request.RoleHeadId;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
