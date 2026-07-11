using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_WelfareInvestHandler : AMActorLocationRpcHandler<Unit, C2M_WelfareInvestRequest, M2C_WelfareInvestResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_WelfareInvestRequest request, M2C_WelfareInvestResponse response, Action reply)
        {
            if (unit.GetComponent<BagComponentServer>().GetBagLeftCell() < 1)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }
            if (request.Index < 0 || request.Index >= CommonConfig.WelfareInvestList.Count)
            {
                Log.Error($"C2M_WelfareInvestRequest 1");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }
            //if (unit.GetComponent<RoleInfoComponentServer>().RoleInfo.WelfareInvestList.Contains(request.Index))
            //{
            //    response.Error = ErrorCode.ERR_AlreadyReceived;
            //    reply();
            //    return;
            //}

            int ment = CommonConfig.WelfareInvestList[request.Index].KeyId;
            if (unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Gold <= ment)
            {
                response.Error = ErrorCode.ERR_GoldNotEnoughError;
                reply();
                return;
            }
            string reward = CommonConfig.WelfareInvestList[request.Index].Value;
            unit.GetComponent<BagComponentServer>().OnAddItemData(reward, $"{ItemGetWay.Welfare}_{TimeHelper.ServerNow()}");
            unit.GetComponent<RoleInfoComponentServer>().UpdateRoleMoneySub( UserDataType.Gold,(ment * -1).ToString(), true, ItemGetWay.Welfare );
            unit.GetComponent<NumericComponent>().ApplyChange(null, NumericType.InvestMent, ment, 0);
            unit.GetComponent<NumericComponent>().ApplyChange(null, NumericType.InvestTotal, ment, 0);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
