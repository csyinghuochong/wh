using System;

namespace ET
{
    public class C2R_GMCommonHandler : AMRpcHandler< C2R_GMCommonRequest, R2C_GMCommonResponse>
    {
        protected override async ETTask Run(Session session, C2R_GMCommonRequest request, R2C_GMCommonResponse response, Action reply)
        {
            if (string.IsNullOrEmpty(request.Context) || !AdminHelper.AdminAccount.Contains(request.Account))
            {
                reply();
                return;
            }

            //Game.EventSystem.Publish(new EventType.GMCommonRequest() { Context = request.Context });

            reply();
            await ETTask.CompletedTask;
        }
    }
}
