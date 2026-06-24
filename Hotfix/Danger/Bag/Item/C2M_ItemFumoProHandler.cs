using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_ItemFumoProHandler : AMActorLocationRpcHandler<Unit, C2M_ItemFumoProRequest, M2C_ItemFumoProResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ItemFumoProRequest request, M2C_ItemFumoProResponse response, Action reply)
        {
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            if (bagComponentServer.FuMoItemId == 0)
            {
                reply();
                return;
            }
            bagComponentServer.OnEquipFuMo(bagComponentServer.FuMoItemId ,  bagComponentServer.FuMoProList, request.Index);

            reply();
            await ETTask.CompletedTask;
        }
    }
}
