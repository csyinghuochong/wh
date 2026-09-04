using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_UnionInviteRefuseHandler : AMActorLocationRpcHandler<Unit, C2M_UnionInviteRefuseRequest, M2C_UnionInviteRefuseResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_UnionInviteRefuseRequest request, M2C_UnionInviteRefuseResponse response, Action reply)
        {
            int refuse = request.RefuseUnionInvite != 0 ? 1 : 0;
            unit.GetComponent<RoleContextComponent>().SetRefuseUnionInvite(refuse);
            response.RefuseUnionInvite = refuse;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
