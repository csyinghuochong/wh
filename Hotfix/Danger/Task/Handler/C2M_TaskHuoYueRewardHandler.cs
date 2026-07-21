using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_TaskHuoYueRewardHandler : AMActorLocationRpcHandler<Unit, C2M_TaskHuoYueRewardRequest, M2C_TaskHuoYueRewardResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_TaskHuoYueRewardRequest request, M2C_TaskHuoYueRewardResponse response, Action reply)
        {

            TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();
            HashSet<int> receivedHuoYueIds = new HashSet<int>(taskComponentServer.ReceiveHuoYueIds);
            if (receivedHuoYueIds.Contains(request.HuoYueId))
            {
                response.Error = ErrorCode.ERR_AlreadyReceived;
                reply();
                return;
            }
          
            taskComponentServer.ReceiveHuoYueIds.Add(request.HuoYueId);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
