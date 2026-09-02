using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_TaskNpcTalkCompleteHandler : AMActorLocationRpcHandler<Unit, C2M_TaskNpcTalkCompleteRequest, M2C_TaskNpcTalkCompleteResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_TaskNpcTalkCompleteRequest request, M2C_TaskNpcTalkCompleteResponse response, Action reply)
        {
            TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();
            
            for (int k = 0; k < taskComponentServer.RoleTaskList.Count; k++)
            {
                TaskPro taskPro = taskComponentServer.RoleTaskList[k];
                
                if (!LDTask_2Category.Instance.Contain(taskPro.taskID))
                {
                    Log.Debug($"无效的任务ID {taskPro.taskID}");
                    continue;
                }
                LDTask_2 ldTask = LDTask_2Category.Instance.Get(taskPro.taskID);
     
                if (taskPro.taskStatus < (int)TaskStatuEnum.Completed 
                    && ldTask.Condition_Type  == TastConditionType.TalkToNpc_200  && ldTask.Param2 == request.NpcId)
                {
                    taskPro.taskStatus = TaskStatuEnum.Completed;
                }
            }
  
            reply();
            await ETTask.CompletedTask;
        }
    }
}
