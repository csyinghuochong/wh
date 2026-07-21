using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_SingleRechargeRewardHandler: AMActorLocationRpcHandler<Unit, C2M_SingleRechargeRewardRequest, M2C_SingleRechargeRewardResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_SingleRechargeRewardRequest request, M2C_SingleRechargeRewardResponse response,
        Action reply)
        {
            RoleInfo roleInfo = unit.GetComponent<RoleInfoComponentServer>().RoleInfo;
            if (request.RewardId == 0)
            {
                //response.RewardIds = roleInfo.SingleRechargeIds;
                reply();
                return;
            }

            bool isGoogleServer = ServerHelper.IsGoogleServer(UnitZoneHelper.GetHomeZone(unit));
            string rewardData = null;
            if (isGoogleServer)
            {
                if (!CommonConfig.SingleRechargeReward_EN.TryGetValue(request.RewardId, out rewardData))
                {
                    Log.Error($"C2M_SingleRechargeRewardRequest 1");
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }
            }
            else
            {
                if (!CommonConfig.SingleRechargeReward.TryGetValue(request.RewardId, out rewardData))
                {
                    Log.Error($"C2M_SingleRechargeRewardRequest 1");
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }
            }

            string[] rewarditemlist = rewardData.Split('@');
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            if (bagComponentServer.GetBagLeftCell() < rewarditemlist.Length)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }

            bool ret = bagComponentServer.OnAddItemData(rewardData, $"{ItemGetWay.ActivityChouKa}_{TimeHelper.ServerNow()}");

            if (ret)
            {
               // roleInfo.SingleRewardIds.Add(request.RewardId);
                //response.RewardIds = roleInfo.SingleRewardIds;
            }
            else
            {
                Log.Error($"领取失败: {bagComponentServer.GetBagLeftCell()} {request.RewardId}");
            }
           
            reply();
            await ETTask.CompletedTask;
        }
    }
}