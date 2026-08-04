using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_JingHeZhuruRequestHandler : AMActorLocationRpcHandler<Unit, C2M_JingHeZhuruRequest, M2C_JingHeZhuruResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_JingHeZhuruRequest request, M2C_JingHeZhuruResponse response, Action reply)
        {
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            reply();
            await ETTask.CompletedTask;
        }
    }
}
