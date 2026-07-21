using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_PetHeXinExploreRewardHandler: AMActorLocationRpcHandler<Unit, C2M_PetHeXinExploreReward, M2C_PetHeXinExploreReward>
    {
        protected override async ETTask Run(Unit unit, C2M_PetHeXinExploreReward request, M2C_PetHeXinExploreReward response, Action reply)
        {
            RoleInfoComponentServer userInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            //if (userInfoComponentServer.RoleInfo.PetHeXinExploreRewardIds.Contains(request.RewardId))
            //{
            //    response.Error = ErrorCode.ERR_AlreadyReceived;
            //    reply();
            //    return;
            //}

            if (!CommonConfig.PetHeXinExploreReward.TryGetValue(request.RewardId, out string rewardConfig))
            {
                Log.Error($"C2M_PetHeXinExploreReward 1");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            if (numericComponent.GetAsInt(NumericType.PetHeXinExploreNumber) < request.RewardId)
            {
                response.Error = ErrorCode.Pre_Condition_Error;
                reply();
                return;
            }

            string[] reward = rewardConfig.Split('$');
            string[] items = reward[0].Split('@');
            if (bagComponentServer.GetBagLeftCell() < items.Length)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }

           // userInfoComponentServer.RoleInfo.PetHeXinExploreRewardIds.Add(request.RewardId);
            string[] diamond = reward[1].Split(';')[1].Split(',');
            int diamondMin = int.Parse(diamond[0]);
            int diamondMax = int.Parse(diamond[1]);
            int randomZuanshi = RandomHelper.RandomNumber(diamondMin, diamondMax);
            bagComponentServer.OnAddItemData(reward[0], $"{96}_{TimeHelper.ServerNow()}");
            userInfoComponentServer.UpdateRoleMoneyAdd(UserDataType.Diamond, randomZuanshi.ToString(), true, 96);

            reply();
            await ETTask.CompletedTask;
        }
    }
}