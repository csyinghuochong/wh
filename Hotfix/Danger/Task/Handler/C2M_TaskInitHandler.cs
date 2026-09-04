using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_TaskInitHandler : AMActorLocationRpcHandler<Unit, C2M_TaskInitRequest, M2C_TaskInitResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_TaskInitRequest request, M2C_TaskInitResponse response, Action reply)
        {
            TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();
            response.RoleTaskList_2 = taskComponentServer.GetClientShowTaskList_2();
            response.RoleComoleteTaskList_2 = taskComponentServer.RoleComoleteTaskList_2;
            response.RoleTaskList_1 = taskComponentServer.RoleTaskList_1;
            response.RoleComoleteTaskList_1 = taskComponentServer.RoleComoleteTaskList_1;
            reply();
            await ETTask.CompletedTask;
        }
    }
}

