using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_MakeSelectHandler : AMActorLocationRpcHandler<Unit, C2M_MakeSelectRequest, M2C_MakeSelectResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_MakeSelectRequest request, M2C_MakeSelectResponse response, Action reply)
        {
          
            reply();
            await ETTask.CompletedTask;
        }
    }
}
