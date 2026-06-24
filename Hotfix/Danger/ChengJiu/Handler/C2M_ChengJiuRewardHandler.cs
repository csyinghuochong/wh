

using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ChengJiuRewardHandler : AMActorLocationRpcHandler<Unit, C2M_ChengJiuRewardRequest, M2C_ChengJiuRewardResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_ChengJiuRewardRequest request, M2C_ChengJiuRewardResponse response, Action reply)
        {
            ChengJiuComponentServer chengJiuComponentServer = unit.GetComponent<ChengJiuComponentServer>();
            if (!ChengJiuRewardConfigCategory.Instance.Contain(request.RewardId))
            {
                response.Error = ErrorCode.ERR_NetWorkError;
                reply();
                return;
            }

            ChengJiuRewardConfig chengJiuConfig = ChengJiuRewardConfigCategory.Instance.Get(request.RewardId);
            if (chengJiuComponentServer.TotalChengJiuPoint < chengJiuConfig.NeedPoint)
            {
                reply();
                return;
            }
            if (chengJiuComponentServer.AlreadReceivedId.Contains(request.RewardId))
            {
                reply();
                return;
            }

            response.Error = chengJiuComponentServer.ReceivedReward(request.RewardId);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
