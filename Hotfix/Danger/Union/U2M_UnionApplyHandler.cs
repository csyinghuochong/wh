using System;

namespace ET
{

    [ActorMessageHandler]
    public class U2M_UnionApplyHandler : AMActorRpcHandler<Unit, U2M_UnionApplyRequest, M2U_UnionApplyResponse>
    {
        protected override async ETTask Run(Unit unit, U2M_UnionApplyRequest request, M2U_UnionApplyResponse response, Action reply)
        {
            NumericComponent numeric = unit.GetComponent<NumericComponent>();
            RoleInfoComponentServer roleInfo = unit.GetComponent<RoleInfoComponentServer>();
            roleInfo.SetUnionName(request.UnionName);
            numeric.ApplyValue(NumericType.UnionLeader, 0);
            numeric.ApplyValue(NumericType.UnionId_0, request.UnionId);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
