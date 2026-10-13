using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_RoleHeadFrameUseHandler : AMActorLocationRpcHandler<Unit, C2M_RoleHeadFrameUseRequest, M2C_RoleHeadFrameUseResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_RoleHeadFrameUseRequest request, M2C_RoleHeadFrameUseResponse response, Action reply)
        {
            unit.GetComponent<RoleInfoComponentServer>().RoleInfo.RoleHeadFrame = request.RoleHeadFrame;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
