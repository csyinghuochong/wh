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
            //if (userInfoComponentServer.RoleInfo.PetHeXinExploreRewardIds.Contains(request.RewardId))
            //{
            //    response.Error = ErrorCode.ERR_AlreadyReceived;
            //    reply();
            //    return;
            //}

            if (!CommonConfig.PetHeXinExploreReward.ContainsKey(request.RewardId))
            {
                Log.Error($"C2M_PetHeXinExploreReward 1");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            if (numericComponent.GetAsInt(NumericType.PetHeXinExploreNumber) < request.RewardId)
            {
                response.Error = ErrorCode.Pre_Condition_Error;
                reply();
                return;
            }

            string[] reward = CommonConfig.PetHeXinExploreReward[request.RewardId].Split('$');
            string[] items = reward[0].Split('@');
            string[] diamond = reward[1].Split(';')[1].Split(',');
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            if (bagComponentServer.GetBagLeftCell() < items.Length)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }

           // userInfoComponentServer.RoleInfo.PetHeXinExploreRewardIds.Add(request.RewardId);
            int randomZuanshi = RandomHelper.RandomNumber(int.Parse(diamond[0]), int.Parse(diamond[1]));
            bagComponentServer.OnAddItemData(reward[0], $"{96}_{TimeHelper.ServerNow()}");
            userInfoComponentServer.UpdateRoleMoneyAdd(UserDataType.Diamond, randomZuanshi.ToString(), true, 96);

            reply();
            await ETTask.CompletedTask;
        }
    }
}