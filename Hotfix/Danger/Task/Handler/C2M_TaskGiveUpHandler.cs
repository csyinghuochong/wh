
using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_TaskGiveUpHandler : AMActorLocationRpcHandler<Unit, C2M_TaskGiveUpRequest, M2C_TaskGiveUpResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_TaskGiveUpRequest request, M2C_TaskGiveUpResponse response, Action reply)
        {
            int taskTable = ResolveTaskTable(request.TaskTable, request.TaskId);
            TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();
            taskComponentServer.OnRecvGiveUpTask(request.TaskId, taskTable);
           
            reply();
            await ETTask.CompletedTask;
        }

        private static int ResolveTaskTable(int taskTable, int taskId)
        {
            if (taskTable == TaskTableType.Task_1 || taskTable == TaskTableType.Task_2)
            {
                return taskTable;
            }

            if (LDTask_1Category.Instance != null && LDTask_1Category.Instance.Contain(taskId))
            {
                return TaskTableType.Task_1;
            }

            if (LDTask_2Category.Instance != null && LDTask_2Category.Instance.Contain(taskId))
            {
                return TaskTableType.Task_2;
            }

            return TaskTableType.None;
        }

    }
}
