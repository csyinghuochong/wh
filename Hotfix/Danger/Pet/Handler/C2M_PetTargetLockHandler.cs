using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_PetTargetLockHandler : AMActorLocationRpcHandler<Unit, C2M_PetTargetLockRequest, M2C_PetTargetLockResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_PetTargetLockRequest request, M2C_PetTargetLockResponse response, Action reply)
        {
            AttackRecordComponent attackRecordComponent = unit.GetComponent<AttackRecordComponent>();
            attackRecordComponent.PetLockId = request.TargetId;

            reply();
            await ETTask.CompletedTask;
        }
    }
}
