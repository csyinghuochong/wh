using System;
using System.Collections.Generic;


namespace ET
{

    [ActorMessageHandler]
    public class C2M_RolePointResetHandler : AMActorLocationRpcHandler<Unit, C2M_RolePointResetRequest, M2C_RolePointResetResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_RolePointResetRequest request, M2C_RolePointResetResponse response, Action reply)
        {
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;
            if (roleInfo.Lv < RoleAddPointHelper.GetAutoLevel())
            {
                response.Error = ErrorCode.ERR_LevelNoEnough;
                reply();
                return;
            }

            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            List<RewardItem> costs = ItemNewHelper.GetRewardItems(ReadGlobalCost(GlobalValueKey.Global_Role_Potential_Points_Reset_Consume));
            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            if (costs.Count > 0 && !bag.OnCostItemData(costs, ItemLocType.ItemLocBag, ItemGetWay.CostItem))
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            numericComponent.ApplyValue(NumericType.Point_Ti_1, 0);
            numericComponent.ApplyValue(NumericType.Point_Li_2, 0);
            numericComponent.ApplyValue(NumericType.Point_Zhi_3, 0);
            numericComponent.ApplyValue(NumericType.Point_Nian_4, 0);
            numericComponent.ApplyValue(NumericType.Point_Min_5, 0);
            numericComponent.ApplyValue(NumericType.Point_Xun_6, 0);

            int remainPoint = RoleAddPointHelper.GetTotalFreePointByLevel(roleInfo.Lv);

            numericComponent.ApplyValue(NumericType.PointRemain, remainPoint);

            response.Error = ErrorCode.ERR_Success;
            reply();
            await ETTask.CompletedTask;
        }

        static string ReadGlobalCost(string key)
        {
            string raw = LDGlobalValueCategory.Instance.GetByKey(key).Value;
            if (string.IsNullOrEmpty(raw))
            {
                return raw;
            }

            raw = raw.Trim();
            if (raw.Length >= 2 && raw[0] == '{' && raw[raw.Length - 1] == '}')
            {
                return raw.Substring(1, raw.Length - 2).Trim();
            }

            return raw;
        }
    }
}