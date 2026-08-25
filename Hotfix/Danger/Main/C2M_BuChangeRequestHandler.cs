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
            RechargeComponentServer rechargeComponentServer = unit.GetComponent<RechargeComponentServer>();
            R2M_BuChangeResponse centerAccount = (R2M_BuChangeResponse)await ActorMessageSenderComponent.Instance.Call(accountZone, new M2R_BuChangeRequest()
            { 
                BuChangId = request.BuChangId,
                UserId = roleInfoComponentServer.Id,
                AccountId = roleInfo.AccInfoID
            });

            if (centerAccount.BuChangRecharge != 0)
            {
                rechargeComponentServer.RechargePro.TotalRechargeNum += centerAccount.BuChangRecharge;
                rechargeComponentServer.NotifyClient();
            }
            roleInfoComponentServer.UpdateRoleData( UserDataType.Diamond, centerAccount.BuChangDiamond.ToString(), true, ItemGetWay.Activity);
            response.PlayerInfo = centerAccount.PlayerInfo;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
