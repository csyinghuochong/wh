using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_TaskHuoYueRewardHandler : AMActorLocationRpcHandler<Unit, C2M_TaskHuoYueRewardRequest, M2C_TaskHuoYueRewardResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_TaskHuoYueRewardRequest request, M2C_TaskHuoYueRewardResponse response, Action reply)
        {

            TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();
            if (taskComponentServer.ReceiveHuoYueIds.Contains(request.HuoYueId))
            {
                response.Error = ErrorCode.ERR_AlreadyReceived;
                reply();
                return;
            }
            if (!HuoYueRewardConfigCategory.Instance.Contain(request.HuoYueId))
            {
                Log.Error($"C2M_TaskHuoYueRewardRequest 1");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            HuoYueRewardConfig huoYueRewardConfig = HuoYueRewardConfigCategory.Instance.Get(request.HuoYueId);
            long haveHuoyue = unit.GetComponent<TaskComponentServer>().GetHuoYueDu();
            if (haveHuoyue < huoYueRewardConfig.NeedPoint)
            {
                response.Error = ErrorCode.ERR_HouBiNotEnough;
                reply();
                return;
            }

            taskComponentServer.ReceiveHuoYueIds.Add(request.HuoYueId);
            unit.GetComponent<BagComponentServer>().OnAddItemData(huoYueRewardConfig.RewardItems, $"{ItemGetWay.TaskCountry}_{TimeHelper.ServerNow()}");

            if (huoYueRewardConfig.NeedPoint >= 100)
            {
                unit.GetComponent<ChengJiuComponentServer>().TriggerEvent(ChengJiuTargetEnum.HuoYue100Reward_221, 0, 1);
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
