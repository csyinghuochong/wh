using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_TaskCommitRequestHandler : AMActorLocationRpcHandler<Unit, C2M_TaskCommitRequest, M2C_TaskCommitResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_TaskCommitRequest request, M2C_TaskCommitResponse response, Action reply)
        {
            if (!LDTask_2Category.Instance.Contain(request.TaskId))
            {
                reply();
                return;
            }
            
            TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();
            response.Error = taskComponentServer.OnCommitTask(request);
            response.RoleComoleteTaskList = taskComponentServer.RoleComoleteTaskList;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
