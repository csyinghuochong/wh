using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_TaskTrackHandler : AMActorLocationRpcHandler<Unit, C2M_TaskTrackRequest, M2C_TaskTrackResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_TaskTrackRequest request, M2C_TaskTrackResponse response, Action reply)
        {
            int taskTable = ResolveTaskTable(request.TaskTable, request.TaskId);
            response.Error = unit.GetComponent<TaskComponentServer>().OnTrackTask(request.TaskId, taskTable, request.TrackStatus);
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
