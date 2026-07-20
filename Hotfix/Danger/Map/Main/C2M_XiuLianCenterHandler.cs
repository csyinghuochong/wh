using System;
using UnityEngine;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_XiuLianCenterHandler : AMActorLocationRpcHandler<Unit, C2M_XiuLianCenterRequest, M2C_XiuLianCenterResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_XiuLianCenterRequest request, M2C_XiuLianCenterResponse response, Action reply)
        {
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            int level = roleInfoComponentServer.RoleInfo.Lv;
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
                float coefficient = float.Parse(LDGlobalValueCategory.Instance.Get(29).Value);
                int addValue = Mathf.CeilToInt(coefficient * level);
                roleInfoComponentServer.UpdateRoleMoneyAdd( UserDataType.Exp, addValue.ToString(), true, ItemGetWay.XiuLian);
            }
            if (request.XiuLianType == 2)
            {
                int xiulianNumber = numericComponent.GetAsInt(NumericType.XiuLian_CoinNumber);
                if (xiulianNumber >= 3)
                {
                    reply();
                    return;
                }
                numericComponent.ApplyValue(NumericType.XiuLian_CoinNumber, xiulianNumber + 1);
                numericComponent.ApplyValue(NumericType.XiuLian_CoinTime, TimeHelper.ServerNow());
                float coefficient = float.Parse(LDGlobalValueCategory.Instance.Get(30).Value);
                int addValue = Mathf.CeilToInt(coefficient * level);
                roleInfoComponentServer.UpdateRoleMoneyAdd(UserDataType.Gold, addValue.ToString(), true, 37);// ItemGetWay.XiuLian);
            }
            reply();
            await ETTask.CompletedTask;
        }
    }
}
