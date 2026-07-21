using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_BuChangeRequestHandler : AMActorLocationRpcHandler<Unit, C2M_BuChangeRequest, M2C_BuChangeResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_BuChangeRequest request, M2C_BuChangeResponse response, Action reply)
        {
            Log.Error($"C2M_BuChangeRequest: {unit.Id}  {request.BuChangId}");
            long accountZone = DBHelper.GetRealmCenter();
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;
            NumericComponent numeric = unit.GetComponent<NumericComponent>();
            R2M_BuChangeResponse centerAccount = (R2M_BuChangeResponse)await ActorMessageSenderComponent.Instance.Call(accountZone, new M2R_BuChangeRequest()
            { 
                BuChangId = request.BuChangId,
                UserId = roleInfoComponentServer.Id,
                AccountId = roleInfo.AccInfoID
            });
 
            numeric.ApplyChange(null, NumericType.RechargeNumber, centerAccount.BuChangRecharge, 0,true);
            roleInfoComponentServer.UpdateRoleMoneyAdd( UserDataType.Diamond, centerAccount.BuChangDiamond.ToString(), true, ItemGetWay.BuChang);
            response.PlayerInfo = centerAccount.PlayerInfo;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
