
using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_TaskNoticeRequestHandler : AMActorLocationRpcHandler<Unit, C2M_TaskNoticeRequest, M2C_TaskNoticeResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_TaskNoticeRequest request, M2C_TaskNoticeResponse response, Action reply)
        {
            request.TaskTable = ResolveTaskTable(request.TaskTable, request.TaskId);
            unit.GetComponent<TaskComponentServer>().OnTaskNotice(request);
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
