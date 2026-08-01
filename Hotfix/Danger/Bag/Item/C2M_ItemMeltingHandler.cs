using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ItemMeltingHandler : AMActorLocationRpcHandler<Unit, C2M_ItemMeltingRequest, M2C_ItemMeltingResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ItemMeltingRequest request, M2C_ItemMeltingResponse response, Action reply)
        {
            reply();
            await ETTask.CompletedTask;
        }
    }
}
