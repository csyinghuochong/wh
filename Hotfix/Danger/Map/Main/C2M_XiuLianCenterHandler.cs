using System;
using UnityEngine;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_XiuLianCenterHandler : AMActorLocationRpcHandler<Unit, C2M_XiuLianCenterRequest, M2C_XiuLianCenterResponse>
    {
        private static float xiuLianExpCoefficient;
        private static float xiuLianCoinCoefficient;
        private static bool xiuLianCacheInit;

        private static void EnsureXiuLianCache()
        {
            if (xiuLianCacheInit)
            {
                return;
            }

            xiuLianExpCoefficient = float.Parse(LDGlobalValueCategory.Instance.Get(29).Value);
            xiuLianCoinCoefficient = float.Parse(LDGlobalValueCategory.Instance.Get(30).Value);
            xiuLianCacheInit = true;
        }

        protected override async ETTask Run(Unit unit, C2M_XiuLianCenterRequest request, M2C_XiuLianCenterResponse response, Action reply)
        {
            EnsureXiuLianCache();
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;
            int level = roleInfo.Lv;
            //1 经验  2金币
            if (request.XiuLianType == 1)
            {
                int xiulianNumber = numericComponent.GetAsInt(NumericType.XiuLian_ExpNumber);
                if (xiulianNumber >= 3)
                {
                    reply();
                    return;
                }
            
                numericComponent.ApplyValue(NumericType.XiuLian_ExpNumber, xiulianNumber+1);
                numericComponent.ApplyValue(NumericType.XiuLian_ExpTime, TimeHelper.ServerNow());
                int addValue = Mathf.CeilToInt(xiuLianExpCoefficient * level);
                roleInfoComponentServer.UpdateRoleMoneyAdd( UserDataType.Exp, addValue.ToString(), true, ItemGetWay.XiuLian);
            }
            else if (request.XiuLianType == 2)
            {
                int xiulianNumber = numericComponent.GetAsInt(NumericType.XiuLian_CoinNumber);
                if (xiulianNumber >= 3)
                {
                    reply();
                    return;
                }
                numericComponent.ApplyValue(NumericType.XiuLian_CoinNumber, xiulianNumber + 1);
                numericComponent.ApplyValue(NumericType.XiuLian_CoinTime, TimeHelper.ServerNow());
                int addValue = Mathf.CeilToInt(xiuLianCoinCoefficient * level);
                roleInfoComponentServer.UpdateRoleMoneyAdd(UserDataType.Gold, addValue.ToString(), true, 37);// ItemGetWay.XiuLian);
            }
            reply();
            await ETTask.CompletedTask;
        }
    }
}
