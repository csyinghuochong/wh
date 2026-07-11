using System;
using System.Linq;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_PetExploreRewardHandler: AMActorLocationRpcHandler<Unit, C2M_PetExploreReward, M2C_PetExploreReward>
    {
        protected override async ETTask Run(Unit unit, C2M_PetExploreReward request, M2C_PetExploreReward response, Action reply)
        {
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            //if (roleInfoComponentServer.RoleInfo.PetExploreRewardIds.Contains(request.RewardId))
            //{
            //    response.Error = ErrorCode.ERR_AlreadyReceived;
            //    reply();
            //    return;
            //}

            if (!CommonConfig.PetExploreReward.Keys.Contains(request.RewardId))
            {
                Log.Error($"C2M_PetExploreReward 1");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            if (unit.GetComponent<NumericComponent>().GetAsInt(NumericType.PetExploreNumber) < request.RewardId)
            {
                response.Error = ErrorCode.Pre_Condition_Error;
                reply();
                return;
            }

            string[] reward = CommonConfig.PetExploreReward[request.RewardId].Split('$');
            string[] items = reward[0].Split('@');
            string[] diamond = reward[1].Split(';')[1].Split(',');
            if (unit.GetComponent<BagComponentServer>().GetBagLeftCell() < items.Length)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }
            int randomZuanshi = RandomHelper.RandomNumber(int.Parse(diamond[0]), int.Parse(diamond[1]));
            unit.GetComponent<BagComponentServer>().OnAddItemData(reward[0], $"{95}_{TimeHelper.ServerNow()}");
            unit.GetComponent<RoleInfoComponentServer>().UpdateRoleMoneyAdd(UserDataType.Diamond, randomZuanshi.ToString(), true, 95);

            reply();
            await ETTask.CompletedTask;
        }
    }
}