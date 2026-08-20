using System;


namespace ET
{

    [ActorMessageHandler]
    public class C2M_RechargeRewardHandler : AMActorLocationRpcHandler<Unit, C2M_RechargeRewardRequest, M2C_RechargeRewardResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_RechargeRewardRequest request, M2C_RechargeRewardResponse response, Action reply)
        {
            if (ServerHelper.IsGoogleServer(UnitZoneHelper.GetHomeZone(unit)))
            {
                if (!CommonConfig.RechargeReward_EN.ContainsKey(request.RechargeNumber))
                {
                    Log.Error($"C2M_RechargeRewardRequest 1");
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }
            }
            else
            {
                if (!CommonConfig.RechargeReward.ContainsKey(request.RechargeNumber))
                {
                    Log.Error($"C2M_RechargeRewardRequest 1");
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }
            }

            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            //if (roleInfoComponentServer.RoleInfo.RechargeReward.Contains(request.RechargeNumber))
            //{
            //    response.Error = ErrorCode.ERR_AlreadyReceived;
            //    reply();
            //    return;
            //}

            long rechargeTotal = unit.GetTotalRechargeNum();
            if (rechargeTotal < request.RechargeNumber)
            {
                response.Error = ErrorCode.Pre_Condition_Error;
                reply();
                return;
            }

            string rewarditem = "";
            if (ServerHelper.IsGoogleServer(UnitZoneHelper.GetHomeZone(unit)))
            {
                rewarditem = CommonConfig.RechargeReward_EN[request.RechargeNumber];
            }
            else
            {
                rewarditem = CommonConfig.RechargeReward[request.RechargeNumber];
            }
            
            string[] rewardList = rewarditem.Split('@');
            if (bagComponentServer.GetBagLeftCell() < rewardList.Length)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }

            bagComponentServer.OnAddItemData(rewarditem, $"{93}_{TimeHelper.ServerNow()}");
            reply();
            await ETTask.CompletedTask;
        }
    }
}
