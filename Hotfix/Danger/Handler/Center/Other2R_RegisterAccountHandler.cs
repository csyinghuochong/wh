using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class Other2R_RegisterAccountHandler : AMActorRpcHandler<Scene, Other2R_RegisterAccount, R2Other_RegisterAccount>
    {
        protected override async ETTask Run(Scene scene, Other2R_RegisterAccount request, R2Other_RegisterAccount response, Action reply)
        {
            response.AccountId =  await RegisterAccountHelper.RegisterAccount(scene, request.AccountName, request.Password, request.LoginType, string.Empty);
            
            reply();
        }
    }
}
