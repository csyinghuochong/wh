using System;

namespace ET
{
    [ActorMessageHandler]
    public class U2M_UnionSetNameHandler : AMActorLocationRpcHandler<Unit, U2M_UnionSetNameRequest, M2U_UnionSetNameResponse>
    {
        protected override async ETTask Run(Unit unit, U2M_UnionSetNameRequest request, M2U_UnionSetNameResponse response, Action reply)
        {
            unit.GetComponent<RoleInfoComponentServer>().SetUnionName(request.UnionName);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
