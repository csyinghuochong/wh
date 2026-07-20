using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_KillMonsterRewardHandler: AMActorLocationRpcHandler<Unit, C2M_KillMonsterRewardRequest, M2C_KillMonsterRewardResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_KillMonsterRewardRequest request, M2C_KillMonsterRewardResponse response, Action reply)
        {
            if (!CommonConfig.KillMonsterReward.ContainsKey(request.Key))
            {
                Log.Error($"C2M_KillMonsterRewardRequest 1");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            if (numericComponent.GetAsInt(NumericType.KillMonsterReward) >= request.Key)
            {
                response.Error = ErrorCode.ERR_Parameter;
                reply();
                return;
            }

            if (numericComponent.GetAsInt(NumericType.KillMonsterNumber) < request.Key)
            {
                Log.Error($"C2M_KillMonsterRewardRequest 3");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            string[] occItems = CommonConfig.KillMonsterReward[request.Key].Split('&');
            string[] items;
            if (occItems.Length == 3)
            {
                items = occItems[roleInfoComponentServer.RoleInfo.Occ - 1].Split('@');
            }
            else
            {
                items = occItems[0].Split('@');
            }

            if (items.Length < request.Index + 1 || request.Index < 0)
            {
                Log.Error($"C2M_KillMonsterRewardRequest 4");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            string item = items[request.Index];
            numericComponent.ApplyValue(NumericType.KillMonsterReward, request.Key);
            unit.GetComponent<BagComponentServer>().OnAddItemData(item, $"{ItemGetWay.KillMonsterReward}_{TimeHelper.ServerNow()}");
            reply();
            await ETTask.CompletedTask;
        }
    }
}