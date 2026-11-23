using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_TaskInitHandler : AMActorLocationRpcHandler<Unit, C2M_TaskInitRequest, M2C_TaskInitResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_TaskInitRequest request, M2C_TaskInitResponse response, Action reply)
        {
            TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();
            // 登录/重拉任务时补接已满足条件的无NPC任务，回包直接携带追踪状态。
            taskComponentServer.AutoAcceptTasks_1();
            // 已有的未完成等级任务也按当前等级校正，直接随本次初始化回包返回。
            foreach (TaskPro taskPro in taskComponentServer.RoleTaskList_2)
            {
                taskComponentServer.RefreshRoleLevelTaskProgress(taskPro);
            }
            response.RoleTaskList_2 = taskComponentServer.GetClientShowTaskList_2();
            response.RoleComoleteTaskList_2 = taskComponentServer.RoleComoleteTaskList_2;
            response.RoleTaskList_1 = taskComponentServer.RoleTaskList_1;
            response.RoleComoleteTaskList_1 = taskComponentServer.RoleComoleteTaskList_1;
            response.NextTask1_Id = taskComponentServer.NextTask1_Id;
            taskComponentServer.EnsureNextUnionWorkTaskId();
            response.NextUnionWorkTaskId = taskComponentServer.NextUnionWorkTaskId;
            reply();
            await ETTask.CompletedTask;
        }
    }
}

