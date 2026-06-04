using System;
using System.Collections.Generic;

namespace ET
{
    [MessageHandler]
    public class C2R_RegisterHandler : AMRpcHandler<C2R_Register, R2C_Register>
    {

        protected override async ETTask Run(Session session, C2R_Register request, R2C_Register response, Action reply)
        {
            Log.Warning($"C2Center_Register:{request.Account}");
            if (session.DomainScene().SceneType != SceneType.Realm)
            {
                Log.Warning($"请求的Scene错误2，当前Scene为：{session.DomainScene().SceneType}");
                session.Dispose();
                return;
            }
            session.RemoveComponent<SessionAcceptTimeoutComponent>();

            if (session.GetComponent<SessionLockingComponent>() != null)
            {
                response.Error = ErrorCode.ERR_RequestRepeatedly;
                reply();
                session.Disconnect().Coroutine();
                return;
            }

            if (string.IsNullOrEmpty(request.Account) || !StringHelper.IsSafeSqlString(request.Account))
            {
                response.Error = ErrorCode.ERR_UnSafeSqlString;
                reply();
                session.Disconnect().Coroutine();
                return;
            }

            using (session.AddComponent<SessionLockingComponent>())
            {
                await RegisterAccountHelper.RegisterAccount(session.DomainScene(), request.Account, request.Password,
                    LoginTypeEnum.RegisterLogin, string.Empty);
                    
                //发送创建回执
                reply();
            }
        }
    }
}
