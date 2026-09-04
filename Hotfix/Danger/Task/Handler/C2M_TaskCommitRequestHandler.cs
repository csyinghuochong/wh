using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_TaskCommitRequestHandler : AMActorLocationRpcHandler<Unit, C2M_TaskCommitRequest, M2C_TaskCommitResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_TaskCommitRequest request, M2C_TaskCommitResponse response, Action reply)
        {
            TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();
            if (LDTask_1Category.Instance != null && LDTask_1Category.Instance.Contain(request.TaskId))
            {
                response.Error = taskComponentServer.OnCommitTask_1(request);
                response.RoleComoleteTaskList_2 = taskComponentServer.RoleComoleteTaskList_2;
                response.RoleComoleteTaskList_1 = taskComponentServer.RoleComoleteTaskList_1;
                reply();
                await ETTask.CompletedTask;
                return;
            }

            if (!LDTask_2Category.Instance.Contain(request.TaskId))
            {
                reply();
                return;
            }
            
            response.Error = taskComponentServer.OnCommitTask(request);
            response.RoleComoleteTaskList_2 = taskComponentServer.RoleComoleteTaskList_2;
            response.RoleComoleteTaskList_1 = taskComponentServer.RoleComoleteTaskList_1;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
