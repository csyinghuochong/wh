using System;
using System.Collections.Generic;

namespace ET
{

    [MessageHandler]
    public class C2R_QueryAccountHandler : AMRpcHandler<C2R_QueryAccountRequest, R2C_QueryAccountResponse>
    {
        protected override async ETTask Run(Session session, C2R_QueryAccountRequest request, R2C_QueryAccountResponse response, Action reply)
        {

            await ETTask.CompletedTask;
        }
    }
}
