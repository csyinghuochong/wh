using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ChouKaRewardHandler : AMActorLocationRpcHandler<Unit, C2M_ChouKaRewardRequest, M2C_ChouKaRewardResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ChouKaRewardRequest request, M2C_ChouKaRewardResponse response, Action reply)
        {
            RoleInfoComponentServer roleInfoComponentServer= unit.GetComponent<RoleInfoComponentServer>();
            if (roleInfoComponentServer.RoleInfo.ChouKaRewardIds.Contains(request.RewardId))
            {
                response.Error = ErrorCode.ERR_AlreadyReceived;
                reply();
                return;
            }
            if (!TakeCardRewardConfigCategory.Instance.Contain(request.RewardId))
            {
                Log.Error($"C2M_ChouKaRewardError {unit.Id} {request.RewardId}");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }
            TakeCardRewardConfig rewardConfig = TakeCardRewardConfigCategory.Instance.Get(request.RewardId);
            if (unit.GetComponent<NumericComponent>().GetAsInt(NumericType.ChouKa) < rewardConfig.RoseLvLimit)
            {
                response.Error = ErrorCode.Pre_Condition_Error;
                reply();
                return;
            }
            string[] rewarditemlist = rewardConfig.RewardItems.Split('@');
            if (unit.GetComponent<BagComponentServer>().GetBagLeftCell() < rewarditemlist.Length)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }

            roleInfoComponentServer.RoleInfo.ChouKaRewardIds.Add(request.RewardId);
            int randomZuanshi = RandomHelper.RandomNumber(rewardConfig.RewardDiamond[0], rewardConfig.RewardDiamond[1]);
            unit.GetComponent<BagComponentServer>().OnAddItemData(rewardConfig.RewardItems, $"{ItemGetWay.ChouKa}_{TimeHelper.ServerNow()}");
            unit.GetComponent<RoleInfoComponentServer>().UpdateRoleMoneyAdd(  UserDataType.Diamond, randomZuanshi.ToString(),true, ItemGetWay.ChouKa);

            reply();
            await ETTask.CompletedTask;
        }
    }
}
