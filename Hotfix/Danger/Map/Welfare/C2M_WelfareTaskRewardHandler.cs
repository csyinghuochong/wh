using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_WelfareTaskRewardHandler : AMActorLocationRpcHandler<Unit, C2M_WelfareTaskRewardRequest, M2C_WelfareTaskRewardResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_WelfareTaskRewardRequest request, M2C_WelfareTaskRewardResponse response, Action reply)
        {
            TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();   
            bool canget = TaskHelper.IsDayTaskComplete(taskComponentServer.RoleComoleteTaskList, request.day);
            if (!canget)
            {
                response.Error = ErrorCode.Pre_Condition_Error;
                reply();
                return;
            }
            //if (unit.GetComponent<RoleInfoComponentServer>().RoleInfo.WelfareTaskRewards.Contains(request.day))
            //{
            //    response.Error = ErrorCode.ERR_AlreadyReceived;
            //    reply();
            //    return;
            //}

            string reward = CommonConfig.WelfareTaskReward[request.day];
            if (!unit.GetComponent<BagComponentServer>().OnAddItemData(reward, $"{ItemGetWay.Welfare}_{TimeHelper.ServerNow()}"))
            { 
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }
            //unit.GetComponent<RoleInfoComponentServer>().RoleInfo.WelfareTaskRewards.Add(request.day);

            reply();
            await ETTask.CompletedTask;
        }
    }
}
