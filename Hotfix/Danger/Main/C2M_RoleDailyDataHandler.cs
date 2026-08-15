using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_RoleDailyDataHandler : AMActorLocationRpcHandler<Unit, C2M_RoleDailyDataRequest, M2C_RoleDailyDataInit>
    {
        protected override async ETTask Run(Unit unit, C2M_RoleDailyDataRequest request, M2C_RoleDailyDataInit response, Action reply)
        {
            RoleDailyDataComponentServer daily = unit.GetComponent<RoleDailyDataComponentServer>();
            if (daily == null)
            {
                daily = unit.AddComponent<RoleDailyDataComponentServer>();
            }

            daily.FillInitResponse(response);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
