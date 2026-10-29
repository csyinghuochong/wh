using System;
using System.Collections.Generic;


namespace ET
{
    [MessageHandler]
    public class C2R_RestoreRoleHandler : AMRpcHandler<C2R_RestoreRoleData, R2C_RestoreRoleData>
    {
        protected override async ETTask Run(Session session, C2R_RestoreRoleData request, R2C_RestoreRoleData response, Action reply)
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
                List<DBCenterAccountInfo> accountList = await Game.Scene.GetComponent<DBComponent>().Query<DBCenterAccountInfo>(session.DomainZone(), d => d.Id == request.AccountId);
                if (accountList.Count == 0)
                {
                    response.Error = ErrorCode.ERR_NotFindAccount;
                    reply();
                    return;
                }

                DBCenterAccountInfo account = accountList[0];
                CreateRoleInfo roleInfo = null;
                for (int i = 0; i < account.RoleList.Count; i++)
                {
                    if (account.RoleList[i].UserID != request.DeleUserID)
                    {
                        continue;
                    }

                    roleInfo = account.RoleList[i];
                    break;
                }

                if (roleInfo == null || roleInfo.State == (int)RoleInfoState.Freeze)
                {
                    response.Error = ErrorCode.ERR_Error;
                    reply();
                    return;
                }

                roleInfo.Cooldown = 0;
                await Game.Scene.GetComponent<DBComponent>().Save<DBCenterAccountInfo>(session.DomainZone(), account);
                response.Cooldown = 0;
                reply();
            }
        }

    }
}