using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_FubenGetRewardHandler : AMActorLocationRpcHandler<Unit, C2M_GetFubenRewardRequest, M2C_GetFubenRewardReponse>
    {
        protected override async ETTask Run(Unit unit, C2M_GetFubenRewardRequest request, M2C_GetFubenRewardReponse response, Action reply)
        {
            //需要验证, 奖励的数据放在fubencompoentsystem

            List<RewardItem> rewardItems = new List<RewardItem>();
            rewardItems.Add(request.RewardItem);
            unit.GetComponent<BagComponentServer>().OnAddItemData(rewardItems, string.Empty, $"{ItemGetWay.FubenGetReward}_{TimeHelper.ServerNow()}");

            response.Error = ErrorCode.ERR_Success;
            reply();

            await ETTask.CompletedTask;
        }
    }
}
