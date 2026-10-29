using System;
using System.Collections.Generic;

namespace ET
{
    [MessageHandler]
	public class C2R_DeleteRoleHandler : AMRpcHandler<C2R_DeleteRoleData, R2C_DeleteRoleData>
	{
		protected override async ETTask Run(Session session, C2R_DeleteRoleData request, R2C_DeleteRoleData response, Action reply)
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
                List<DBCenterAccountInfo> newAccountList = await Game.Scene.GetComponent<DBComponent>().Query<DBCenterAccountInfo>(session.DomainZone(), d => d.Id == request.AccountId);
                if (newAccountList.Count == 0)
                {
                    response.Error = ErrorCode.ERR_NotFindAccount;
                    reply();
                    return;
                }

                DBCenterAccountInfo newAccount = newAccountList[0];
                CreateRoleInfo roleInfo = null;
                for (int i = 0; i < newAccount.RoleList.Count; i++)
                {
                    if (newAccount.RoleList[i].UserID != request.DeleUserID)
                    {
                        continue;
                    }

                    roleInfo = newAccount.RoleList[i];
                    break;
                }

                if (roleInfo == null || roleInfo.State == (int)RoleInfoState.Freeze)
                {
                    response.Error = ErrorCode.ERR_Error;
                    reply();
                    return;
                }

                int roleLv = roleInfo.PlayerLv;
                RoleInfoComponentServer roleInfoComponentServer = await DBHelper.GetComponent<RoleInfoComponentServer>(request.ServerId, request.DeleUserID);
                if (roleInfoComponentServer != null)
                {
                    roleLv = roleInfoComponentServer.RoleInfo.Lv;
                }

                int waitLevel = LDGlobalValueCategory.Instance.GetInt(GlobalValueKey.Global_Role_Delete_Wait_Level);
                if (roleLv >= waitLevel)
                {
                    if (roleInfo.Cooldown <= 0)
                    {
                        int waitDays = LDGlobalValueCategory.Instance.GetInt(GlobalValueKey.Global_Role_Delete_Wait_Time);
                        roleInfo.Cooldown = TimeHelper.ServerNow() + waitDays * TimeHelper.OneDay;
                        await Game.Scene.GetComponent<DBComponent>().Save<DBCenterAccountInfo>(session.DomainZone(), newAccount);
                    }

                    response.Cooldown = roleInfo.Cooldown;
                    reply();
                    return;
                }

                roleInfo.State = (int)RoleInfoState.Freeze;
                roleInfo.Cooldown = 0;
                await Game.Scene.GetComponent<DBComponent>().Save<DBCenterAccountInfo>(session.DomainZone(), newAccount);
                await RemoveRoleRuntime(request.ServerId, request.DeleUserID, request.AccountId);
                if (roleInfoComponentServer != null)
                {
                    List<string> allComponets = DBHelper.GetAllUnitComponent();
                    for (int i = 0; i < allComponets.Count; i++)
                    {
                        Game.Scene.GetComponent<DBComponent>().Remove<Entity>(request.ServerId, request.DeleUserID, allComponets[i]).Coroutine();
                    }
                }

                response.Cooldown = 0;
                reply();
            }
		}

        private static async ETTask RemoveRoleRuntime(int serverId, long userId, long accountId)
        {
            long mapInstanceId = DBHelper.GetRankServerId(serverId);
            await ActorMessageSenderComponent.Instance.Call(mapInstanceId, new R2Rank_DeleteRoleData()
            {
                DeleUserID = userId,
                AccountId = accountId
            });
            long paimaiInstanceid = DBHelper.GetPaiMaiServerId(serverId);
            await ActorMessageSenderComponent.Instance.Call(paimaiInstanceid, new R2Consign_DeleteRoleData()
            {
                DeleUserID = userId,
                AccountId = accountId,
                DeleteType = 0,
            });
            DBHelper.DeleteUnitCache(serverId, userId).Coroutine();
        }
	}
}