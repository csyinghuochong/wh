using System;

namespace ET
{
    [MessageHandler]
    public class C2R_RegisterHandler : AMRpcHandler<C2R_Register, R2C_Register>
    {

        protected override async ETTask Run(Session session, C2R_Register request, R2C_Register response, Action reply)
        {
            if (session.DomainScene().SceneType != SceneType.Realm)
            {
                response.Error = ErrorCode.ERR_RequestSceneTypeError;
                reply();
                session.Disconnect().Coroutine();
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

            if (string.IsNullOrWhiteSpace(request.Account) || string.IsNullOrWhiteSpace(request.Password))
            {
                response.Error = ErrorCode.ERR_LoginInfoIsNull;
                reply();
                session.Disconnect().Coroutine();
                return;
            }

            if (!StringHelper.IsSafeSqlString(request.Account))
            {
                response.Error = ErrorCode.ERR_UnSafeSqlString;
                reply();
                session.Disconnect().Coroutine();
                return;
            }

            using (session.AddComponent<SessionLockingComponent>())
            {
                long accountId = await RegisterAccountHelper.RegisterAccount(session.DomainScene(), request.Account, request.Password,
                    LoginTypeEnum.RegisterLogin, string.Empty, true);
                if (accountId == 0)
                {
                    response.Error = ErrorCode.ERR_AccountAlreadyRegister;
                }
                    
                //发送创建回执
                reply();
            }
        }
    }
}
