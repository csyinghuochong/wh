
using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_BagSortHandler : AMActorLocationRpcHandler<Unit, C2M_BagSortRequest, M2C_BagSortResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_BagSortRequest request, M2C_BagSortResponse response, Action reply)
        {
            await ETTask.CompletedTask;
        }
    }
}
