using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_TaskNpcTalkCompleteHandler : AMActorLocationRpcHandler<Unit, C2M_TaskNpcTalkCompleteRequest, M2C_TaskNpcTalkCompleteResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_TaskNpcTalkCompleteRequest request, M2C_TaskNpcTalkCompleteResponse response, Action reply)
        {
            unit.GetComponent<TaskComponentServer>().OnNpcTalkComplete(request.NpcId);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
