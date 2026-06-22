using System;


namespace ET
{

    [ActorMessageHandler]
    public class C2M_RechargeRewardHandler : AMActorLocationRpcHandler<Unit, C2M_RechargeRewardRequest, M2C_RechargeRewardResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_RechargeRewardRequest request, M2C_RechargeRewardResponse response, Action reply)
        {
            if (ServerHelper.IsGoogleServer(unit.DomainZone()))
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

            RoleInfoComponent roleInfoComponent = unit.GetComponent<RoleInfoComponent>();
            if (roleInfoComponent.RoleInfo.RechargeReward.Contains(request.RechargeNumber))
            {
                response.Error = ErrorCode.ERR_AlreadyReceived;
                reply();
                return;
            }

            long rechargeTotal = unit.GetComponent<NumericComponent>().GetAsLong(NumericType.RechargeNumber);
            if (rechargeTotal < request.RechargeNumber)
            {
                response.Error = ErrorCode.Pre_Condition_Error;
                reply();
                return;
            }

            string rewarditem = "";
            if (ServerHelper.IsGoogleServer(unit.DomainZone()))
            {
                rewarditem = CommonConfig.RechargeReward_EN[request.RechargeNumber];
            }
            else
            {
                rewarditem = CommonConfig.RechargeReward[request.RechargeNumber];
            }
            
            string[] rewardList = rewarditem.Split('@');
            if (unit.GetComponent<BagComponent>().GetBagLeftCell() < rewardList.Length)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }

            unit.GetComponent<BagComponent>().OnAddItemData(rewarditem, $"{93}_{TimeHelper.ServerNow()}");
            roleInfoComponent.RoleInfo.RechargeReward.Add(request.RechargeNumber);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
