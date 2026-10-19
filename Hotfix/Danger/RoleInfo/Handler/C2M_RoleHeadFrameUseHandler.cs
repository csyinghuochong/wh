using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_RoleHeadFrameUseHandler : AMActorLocationRpcHandler<Unit, C2M_RoleHeadFrameUseRequest, M2C_RoleHeadFrameUseResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_RoleHeadFrameUseRequest request, M2C_RoleHeadFrameUseResponse response, Action reply)
        {
            RoleInfoComponentServer roleInfo = unit.GetComponent<RoleInfoComponentServer>();
            roleInfo.RoleInfo.RoleHeadFrame = request.RoleHeadFrame;
            roleInfo.SyncUnionMember();
            reply();
            await ETTask.CompletedTask;
        }
    }
}
