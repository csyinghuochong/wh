using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_TaskGetRequestHandler : AMActorLocationRpcHandler<Unit, C2M_TaskGetRequest, M2C_TaskGetResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_TaskGetRequest request, M2C_TaskGetResponse response, Action reply)
        {
            int taskTable = ResolveTaskTable(request.TaskTable, request.TaskId);
            response.TaskTable = taskTable;
            TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();

            if (taskTable == TaskTableType.Task_1)
            {
                (TaskPro taskPro, int error) = taskComponentServer.OnAcceptedTask_1(request.TaskId, request.NpcId);
                response.Error = error;
                response.TaskPro = taskPro;
                reply();
                await ETTask.CompletedTask;
                return;
            }

            if (taskTable != TaskTableType.Task_2)
            {
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                await ETTask.CompletedTask;
                return;
            }

            if (!LDTask_2Category.Instance.Contain(request.TaskId))
            {
                Log.Error($"C2M_TaskGetRequest 1");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            if (UnionHelper.IsUnionWorkTask(request.TaskId))
            {
                response.Error = ErrorCode.ERR_TaskCanNotGet;
                reply();
                return;
            }

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
