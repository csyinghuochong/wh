
using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_TaskGiveUpHandler : AMActorLocationRpcHandler<Unit, C2M_TaskGiveUpRequest, M2C_TaskGiveUpResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_TaskGiveUpRequest request, M2C_TaskGiveUpResponse response, Action reply)
        {
            LDTask_2 ldTask = LDTask_2Category.Instance.Get(request.TaskId);
           
            TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();
            taskComponentServer.OnRecvGiveUpTask(request.TaskId);
           
            reply();
            await ETTask.CompletedTask;
        }

    }
}
