using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_PetExploreRewardHandler: AMActorLocationRpcHandler<Unit, C2M_PetExploreReward, M2C_PetExploreReward>
    {
        protected override async ETTask Run(Unit unit, C2M_PetExploreReward request, M2C_PetExploreReward response, Action reply)
        {
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            NumericComponent numeric = unit.GetComponent<NumericComponent>();
            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            //if (roleInfoComponentServer.RoleInfo.PetExploreRewardIds.Contains(request.RewardId))
            //{
            //    response.Error = ErrorCode.ERR_AlreadyReceived;
            //    reply();
            //    return;
            //}

            if (!CommonConfig.PetExploreReward.TryGetValue(request.RewardId, out string rewardConfig))
            {
                Log.Error($"C2M_PetExploreReward 1");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            if (numeric.GetAsInt(NumericType.PetExploreNumber) < request.RewardId)
            {
                response.Error = ErrorCode.Pre_Condition_Error;
                reply();
                return;
            }

            string[] reward = rewardConfig.Split('$');
            string[] items = reward[0].Split('@');
            string[] diamond = reward[1].Split(';')[1].Split(',');
            if (bag.GetBagLeftCell() < items.Length)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }
            int randomZuanshi = RandomHelper.RandomNumber(int.Parse(diamond[0]), int.Parse(diamond[1]));
            bag.OnAddItemData(reward[0], $"{95}_{TimeHelper.ServerNow()}");
            roleInfoComponentServer.UpdateRoleMoneyAdd(UserDataType.Diamond, randomZuanshi.ToString(), true, 95);

            reply();
            await ETTask.CompletedTask;
        }
    }
}
