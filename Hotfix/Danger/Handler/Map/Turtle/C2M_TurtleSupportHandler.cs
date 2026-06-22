using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_TurtleSupportHandler : AMActorLocationRpcHandler<Unit, C2M_TurtleSupportRequest, M2C_TurtleSupportResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_TurtleSupportRequest request, M2C_TurtleSupportResponse response, Action reply)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Buy, unit.Id))
            {
                RoleInfoComponent roleInfoComponent = unit.GetComponent<RoleInfoComponent>();
               
                long costgold = LDGlobalValueCategory.Instance.TempValue;
                if (roleInfoComponent.RoleInfo.Gold < costgold)
                {
                    response.Error = ErrorCode.ERR_GoldNotEnoughError;
                    reply();
                    return;
                }

                long activtiyserverid = DBHelper.GetActivityServerId(unit.DomainZone());
                M2A_TurtleSupportRequest m2A_TurtleSupport = new M2A_TurtleSupportRequest()
                {
                    UnitId = unit.Id,   
                    AccountId = roleInfoComponent.RoleInfo.AccInfoID,
                    SupportId = request.SupportId,
                };
                A2M_TurtleSupportResponse a2M_TurtleSupport = (A2M_TurtleSupportResponse)await ActorMessageSenderComponent.Instance.Call
                        (activtiyserverid, m2A_TurtleSupport);

                if (a2M_TurtleSupport.Error != ErrorCode.ERR_Success)
                {
                    response.Error = a2M_TurtleSupport.Error;
                    reply();
                    return;
                }
                //扣除金币 
                roleInfoComponent.UpdateRoleMoneySub(UserDataType.Gold, (costgold * -1).ToString(), true, ItemGetWay.Turtle);

            }
            reply();
            await ETTask.CompletedTask;
        }
    }
}
