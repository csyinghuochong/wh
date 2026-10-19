using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_RoleHeadUseHandler : AMActorLocationRpcHandler<Unit, C2M_RoleHeadUseRequest, M2C_RoleHeadUseResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_RoleHeadUseRequest request, M2C_RoleHeadUseResponse response, Action reply)
        {
            RoleInfoComponentServer roleInfo = unit.GetComponent<RoleInfoComponentServer>();
            roleInfo.RoleInfo.RoleHeadId = request.RoleHeadId;
            roleInfo.SyncUnionMember();
            reply();
            await ETTask.CompletedTask;
        }
    }
}
