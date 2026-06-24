using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_SeasonLevelRewardHandler : AMActorLocationRpcHandler<Unit, C2M_SeasonLevelRewardRequest, M2C_SeasonLevelRewardResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_SeasonLevelRewardRequest request, M2C_SeasonLevelRewardResponse response, Action reply)
        {            response.Error = ErrorCode.ERR_ModifyData;
            reply();
            await ETTask.CompletedTask;
#if false // TODO: migrate to LD config

            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();   
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            if (numericComponent.GetAsInt( NumericType.SeasonReward ) >= roleInfoComponentServer.RoleInfo.SeasonLevel)
            {
                response.Error = ErrorCode.ERR_Parameter;
                reply();
                return;
            }

            int rewardLevel = request.SeasonLevel;
            if (numericComponent.GetAsInt(NumericType.SeasonReward) >= rewardLevel )
            {
                Log.Error($"C2M_SeasonLevelRewardRequest 2");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }
            if (roleInfoComponentServer.RoleInfo.SeasonLevel < rewardLevel)
            {
                Log.Error($"C2M_SeasonLevelRewardRequest 3");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }
            /*

            SeasonLevelConfig seasonLevelConfig = SeasonLevelConfigCategory.Instance.Get(rewardLevel);
            if (!unit.GetComponent<BagComponentServer>().OnAddItemData(seasonLevelConfig.Reward, $"{ItemGetWay.Season}_{seasonLevelConfig.Reward}"))
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }

            numericComponent.ApplyValue(NumericType.SeasonReward, rewardLevel);
            */
            reply();
            await ETTask.CompletedTask;
        #endif
}
    }
}
