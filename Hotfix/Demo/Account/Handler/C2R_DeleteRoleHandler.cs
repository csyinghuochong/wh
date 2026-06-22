using System;
using System.Collections.Generic;

namespace ET
{
    [MessageHandler]
	public class C2R_DeleteRoleHandler : AMRpcHandler<C2R_DeleteRoleData, R2C_DeleteRoleData>
	{
		protected override async ETTask Run(Session session, C2R_DeleteRoleData request, R2C_DeleteRoleData response, Action reply)
		{
            try
            {
                if (session.GetComponent<SessionLockingComponent>() != null)
                {
                    response.Error = ErrorCode.ERR_RequestRepeatedly;
                    reply();
                    session.Disconnect().Coroutine();
                    return;
                }

                using (session.AddComponent<SessionLockingComponent>())
                {
                    //存储账号信息
                    List<DBCenterAccountInfo> newAccountList = await Game.Scene.GetComponent<DBComponent>().Query<DBCenterAccountInfo>(session.DomainZone(), d => d.Id == request.AccountId);
                    if (newAccountList.Count == 0)
                    {
                        response.Error = ErrorCode.ERR_NotFindAccount;
                        reply();
                        return;
                    }
                    
                    DBCenterAccountInfo newAccount = newAccountList[0];
                    //移除角色
                    if (newAccount.RoleList.Count > 0)
                    {
                        for (int i = newAccount.RoleList.Count - 1; i >= 0; i--)
                        {
                            if (newAccount.RoleList[i].UserID == request.DeleUserID)
                            {
                                newAccount.RoleList[i].State = (int)RoleInfoState.Freeze;
                            }
                        }
                    }
                    
                    await Game.Scene.GetComponent<DBComponent>().Save<DBCenterAccountInfo>(session.DomainZone(), newAccount);
                    long mapInstanceId = DBHelper.GetRankServerId(request.ServerId);
                    Rank2R_DeleteRoleData deleteResponse = (Rank2R_DeleteRoleData)await ActorMessageSenderComponent.Instance.Call
                    (mapInstanceId, new R2Rank_DeleteRoleData()
                    {
                        DeleUserID = request.DeleUserID,
                        AccountId = request.AccountId
                    });
                    long paimaiInstanceid = DBHelper.GetPaiMaiServerId(request.ServerId);
                    Paimai2R_DeleteRoleData deleteResponse2 = (Paimai2R_DeleteRoleData)await ActorMessageSenderComponent.Instance.Call
                   (paimaiInstanceid, new R2Paimai_DeleteRoleData()
                   {
                       DeleUserID = request.DeleUserID,
                       AccountId = request.AccountId,
                       DeleteType = 0,
                   });

                    DBHelper.DeleteUnitCache(request.ServerId, request.DeleUserID).Coroutine();
                    RoleInfoComponent roleInfoComponent = await DBHelper.GetComponent<RoleInfoComponent>(request.ServerId, request.DeleUserID);
                    NumericComponent numericComponent = await DBHelper.GetComponent<NumericComponent>(request.ServerId, request.DeleUserID);
                    if (roleInfoComponent != null && roleInfoComponent.RoleInfo.Lv <= 10 &&
                        (numericComponent.GetAsInt(NumericType.RechargeNumber) <= 0 ))
                    {
                        List<string> allComponets = DBHelper.GetAllUnitComponent();
                        for (int i = 0; i < allComponets.Count; i++)
                        {
                            Game.Scene.GetComponent<DBComponent>().Remove<Entity>(request.ServerId, request.DeleUserID, allComponets[i]).Coroutine();
                        }
                    }
                    reply();
                }
            }
            catch(Exception ex) 
            {
                Log.Error(ex.ToString());
            }
		}
	}
}