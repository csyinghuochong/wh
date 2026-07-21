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
            numeric.ApplyValue(NumericType.UnionLeader, 0);
            numeric.ApplyValue(NumericType.UnionId_0, request.UnionId);
            roleInfo.UpdateRoleData(UserDataType.UnionName, request.UnionName);
            roleInfo.UpdateRoleDataBroadcast(UserDataType.UnionName, request.UnionName);
            unit.GetComponent<TaskComponentServer>().TriggerTaskEvent(TastConditionType.JoinUnion_9, 0, 1);
         
            unit.UpdateUnionToChat().Coroutine();
            reply();
            await ETTask.CompletedTask;
        }
    }
}
