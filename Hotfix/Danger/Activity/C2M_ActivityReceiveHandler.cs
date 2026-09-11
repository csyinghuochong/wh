using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ActivityReceiveHandler : AMActorLocationRpcHandler<Unit, C2M_ActivityReceiveRequest, M2C_ActivityReceiveResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_ActivityReceiveRequest request, M2C_ActivityReceiveResponse response, Action reply)
        {
            response.Error = ErrorCode.ERR_ModifyData;
            reply();
            await ETTask.CompletedTask;

            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Received, unit.Id))
            {

                ActivityComponentServer activityComponentServer = unit.GetComponent<ActivityComponentServer>();
                if (!ActivityHelper.HaveReceiveTimes(activityComponentServer.ActivityReceiveIds, request.ActivityId))
                {
                    response.Error = ErrorCode.ERR_AlreadyReceived;
                    reply();
                    return;
                }

                reply();
                await ETTask.CompletedTask;
            }
        }
    }
}
