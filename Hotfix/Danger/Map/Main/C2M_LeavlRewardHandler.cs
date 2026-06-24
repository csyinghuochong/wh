using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_LeavlRewardHandler: AMActorLocationRpcHandler<Unit, C2M_LeavlRewardRequest, M2C_LeavlRewardResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_LeavlRewardRequest request, M2C_LeavlRewardResponse response, Action reply)
        {
            if (!CommonConfig.LevelRewardItem.Keys.Contains(request.LvKey))
            {
                Log.Error($"C2M_LeavlRewardRequest 1");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            if (unit.GetComponent<NumericComponent>().GetAsInt(NumericType.LeavlReward) >= request.LvKey)
            {
                response.Error = ErrorCode.ERR_Parameter;
                reply();
                return;
            }

            if (unit.GetComponent<RoleInfoComponent>().RoleInfo.Lv < request.LvKey)
            {
                Log.Error($"C2M_LeavlRewardRequest 3");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            RoleInfoComponent roleInfoComponent = unit.GetComponent<RoleInfoComponent>();
            string[] occItems = CommonConfig.LevelRewardItem[request.LvKey].Split('&');
            string[] items;
            if (occItems.Length > 1)
            {
                items = occItems[roleInfoComponent.RoleInfo.Occ - 1].Split('@');
            }
            else
            {
                items = occItems[0].Split('@');
            }

            if (items.Length < request.Index + 1 || request.Index < 0)
            {
                Log.Error($"C2M_LeavlRewardRequest 4");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            string item = items[request.Index];
            unit.GetComponent<NumericComponent>().ApplyValue(NumericType.LeavlReward, request.LvKey);
            unit.GetComponent<BagComponentServer>().OnAddItemData(item, $"{ItemGetWay.LeavlReward}_{TimeHelper.ServerNow()}");
            reply();
            await ETTask.CompletedTask;
        }
    }
}