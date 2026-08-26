using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_TaskInitHandler : AMActorLocationRpcHandler<Unit, C2M_TaskInitRequest, M2C_TaskInitResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_TaskInitRequest request, M2C_TaskInitResponse response, Action reply)
        {
            TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();
            response.RoleTaskList = taskComponentServer.GetClientShowTaskList();
            response.RoleComoleteTaskList = taskComponentServer.RoleComoleteTaskList;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
