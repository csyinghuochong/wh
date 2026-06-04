using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;


namespace ET
{

    [MessageHandler]
    public class C2R_TapTapAutherHandler : AMRpcHandler<C2R_TapTapAuther, R2C_TapTapAuther>
    {

        protected override async ETTask Run(Session session, C2R_TapTapAuther request, R2C_TapTapAuther response, Action reply)
        {

            if (string.IsNullOrEmpty(request.Account))
            {
                response.Error = ErrorCode.ERR_LoginInfoIsNull;
                reply();
                return;
            }
            long accountZone = DBHelper.GetRealmCenter();
            R2Other_CheckAccount centerAccount = (R2Other_CheckAccount)await ActorMessageSenderComponent.Instance.Call(accountZone, new Other2R_CheckAccount()
            {
                AccountName = request.Account,
                Password = LoginTypeEnum.TikTok.ToString(),
                LoginType = LoginTypeEnum.TikTok,
            });

            //没有则注册
            if (centerAccount.PlayerInfo == null)
            {
                R2Other_RegisterAccount saveAccount = (R2Other_RegisterAccount)await ActorMessageSenderComponent.Instance.Call(accountZone, new Other2R_RegisterAccount()
                {
                    AccountName = request.Account,
                    Password = request.Password,
                    LoginType = request.LoginType,
                    age_type = request.age_type,
                });
            }
            reply();    
        }
    }
}
