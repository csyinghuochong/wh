using System;

namespace ET
{
    /// <summary>
    /// 公会打工接取。只处理 Task_2，不加 TaskTable。
    /// </summary>
    [ActorMessageHandler]
    public class C2M_UnionWorkStartHandler : AMActorLocationRpcHandler<Unit, C2M_UnionWorkStartRequest, M2C_UnionWorkStartResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_UnionWorkStartRequest request, M2C_UnionWorkStartResponse response, Action reply)
        {
            (TaskPro taskPro, int error) = unit.GetComponent<TaskComponentServer>().OnAcceptUnionWork(request.TaskId);
            response.Error = error;
            response.TaskPro = taskPro;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
