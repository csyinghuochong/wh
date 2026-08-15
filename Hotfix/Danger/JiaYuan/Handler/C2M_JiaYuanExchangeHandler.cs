using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_JiaYuanExchangeHandler : AMActorLocationRpcHandler<Unit, C2M_JiaYuanExchangeRequest, M2C_JiaYuanExchangeResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_JiaYuanExchangeRequest request, M2C_JiaYuanExchangeResponse response, Action reply)
        {
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            RoleInfoComponentServer roleInfoComponentServer=unit.GetComponent<RoleInfoComponentServer>();
            RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;
            LDHome ldHome = LDHomeCategory.Instance.Get(roleInfo.JiaYuanLv);
            switch (request.ExchangeType)
            {
                case 1: //金币兑换资金
                   
                    /*if (roleInfo.Gold < ldHome.ExchangeZiJinCostGold)
                    {
                        response.Error = ErrorCode.ERR_ItemNotEnoughError;
                        reply();
                        return;
                    }

                    roleInfoComponent.UpdateRoleData(UserDataType.Gold, (ldHome.ExchangeZiJinCostGold * -1).ToString(), true, ItemGetWay.JiaYuanCost);
                    roleInfoComponent.UpdateRoleData(UserDataType.JiaYuanFund, (ldHome.ExchangeZiJin).ToString(), true, ItemGetWay.JiaYuanExchange);
                    numericComponent.ApplyChange(null, NumericType.JiaYuanExchangeZiJin, 1, 0);*/
                    break;
                case 2: //资金兑换经验
                    if (!LDHomeCategory.Instance.Contain(ldHome.Id + 1))
                    {
                        response.Error = ErrorCode.ERR_JiaYuanLevelMax;
                        reply();
                        return;
                    }
                    /*if (roleInfo.JiaYuanFund < ldHome.ExchangeExpCostZiJin)
                    {
                        response.Error = ErrorCode.ERR_ItemNotEnoughError;
                        reply();
                        return;
                    }

                    roleInfoComponent.UpdateRoleData(UserDataType.JiaYuanFund, (ldHome.ExchangeExpCostZiJin * -1).ToString(), true, ItemGetWay.JiaYuanCost);
                    roleInfoComponent.UpdateRoleData(UserDataType.JiaYuanExp, (ldHome.ExchangeExp).ToString(), true, ItemGetWay.JiaYuanExchange);
                    numericComponent.ApplyChange(null, NumericType.JiaYuanExchangeExp, 1, 0);*/
                    break;
                default:
                    break;
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
