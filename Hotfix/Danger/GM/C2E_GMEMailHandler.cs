using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2E_GMEMailHandler : AMActorRpcHandler<Scene, C2E_GMEMailRequest, E2C_GMEMailResponse>
    {
        protected override async ETTask Run(Scene scene, C2E_GMEMailRequest request, E2C_GMEMailResponse response, Action reply)
        {
            if (string.IsNullOrWhiteSpace(request.MailInfo))
            {
                response.Error = ErrorCode.ERR_Parameter;
                reply();
                return;
            }

            response.Error = await ConsoleHelper.MailConsoleHandler(request.MailInfo);
            reply();
            await ETTask.CompletedTask;
        }

    }
}
