using System;
using System.Linq;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ItemXiLianNumRewardHandler: AMActorLocationRpcHandler<Unit, C2M_ItemXiLianNumReward, M2C_ItemXiLianNumReward>
    {
        protected override async ETTask Run(Unit unit, C2M_ItemXiLianNumReward request, M2C_ItemXiLianNumReward response, Action reply)
        {
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            //if (roleInfoComponentServer.RoleInfo.ItemXiLianNumRewardIds.Contains(request.RewardId))
            //{
            //    response.Error = ErrorCode.ERR_AlreadyReceived;
            //    reply();
            //    return;
            //}

            if (!CommonConfig.ItemXiLianNumReward.TryGetValue(request.RewardId, out _))
            {
                Log.Error($"C2M_ItemXiLianNumReward 1");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

           /* if (unit.GetComponent<NumericComponent>().GetAsInt(NumericType.ItemXiLianNumber) < request.RewardId)
            {
                response.Error = ErrorCode.Pre_Condition_Error;
                reply();
                return;
            }

            string[] reward = CommonConfig.ItemXiLianNumReward[request.RewardId].Split('$');
            string[] items = reward[0].Split('@');
            string[] diamond = reward[1].Split(';')[1].Split(',');
            if (unit.GetComponent<BagComponentServer>().GetBagLeftCell() < items.Length)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }

            roleInfoComponent.RoleInfo.ItemXiLianNumRewardIds.Add(request.RewardId);
            int randomZuanshi = RandomHelper.RandomNumber(int.Parse(diamond[0]), int.Parse(diamond[1]));
            unit.GetComponent<BagComponentServer>().OnAddItemData(reward[0], $"{ItemGetWay.ItemXiLian}_{TimeHelper.ServerNow()}");
            unit.GetComponent<RoleInfoComponentServer>().UpdateRoleMoneyAdd(UserDataType.Diamond, randomZuanshi.ToString(), true, ItemGetWay.ItemXiLian);
*/
            reply();
            await ETTask.CompletedTask;
        }
    }
}