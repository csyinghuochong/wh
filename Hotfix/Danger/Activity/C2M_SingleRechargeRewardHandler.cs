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
                response.RewardIds = roleInfo.SingleRechargeIds;
                reply();
                return;
            }

            if (ServerHelper.IsGoogleServer(unit.DomainZone()))
            {
                if (!CommonConfig.SingleRechargeReward_EN.ContainsKey(request.RewardId))
                {
                    Log.Error($"C2M_SingleRechargeRewardRequest 1");
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }
            }
            else
            {
                if (!CommonConfig.SingleRechargeReward.ContainsKey(request.RewardId))
                {
                    Log.Error($"C2M_SingleRechargeRewardRequest 1");
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }
            }

            if (!roleInfo.SingleRechargeIds.Contains(request.RewardId))
            {
                response.Error = ErrorCode.Pre_Condition_Error;
                reply();
                return;
            }

            if (roleInfo.SingleRewardIds.Contains(request.RewardId))
            {
                response.Error = ErrorCode.ERR_AlreadyReceived;
                reply();
                return;
            }

            string[] rewarditemlist = null;
            if (ServerHelper.IsGoogleServer(unit.DomainZone()))
            {
                rewarditemlist = CommonConfig.SingleRechargeReward_EN[request.RewardId].Split('@');
            }
            else
            {
                rewarditemlist = CommonConfig.SingleRechargeReward[request.RewardId].Split('@');
            }
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            if (bagComponentServer.GetBagLeftCell() < rewarditemlist.Length)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }


            bool ret = false;
            if (ServerHelper.IsGoogleServer(unit.DomainZone()))
            {
                ret = unit.GetComponent<BagComponentServer>().OnAddItemData(CommonConfig.SingleRechargeReward_EN[request.RewardId], $"{ItemGetWay.ActivityChouKa}_{TimeHelper.ServerNow()}");
            }
            else
            {
                ret = unit.GetComponent<BagComponentServer>().OnAddItemData(CommonConfig.SingleRechargeReward[request.RewardId], $"{ItemGetWay.ActivityChouKa}_{TimeHelper.ServerNow()}");
            }

            if (ret)
            {
                roleInfo.SingleRewardIds.Add(request.RewardId);
                response.RewardIds = roleInfo.SingleRewardIds;
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