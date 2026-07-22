
using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_BagSortHandler : AMActorLocationRpcHandler<Unit, C2M_BagSortRequest, M2C_BagSortResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_BagSortRequest request, M2C_BagSortResponse response, Action reply)
        {
            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            ItemLocType loc = (ItemLocType)request.ItemLocType;
            if (loc <= ItemLocType.ItemLocEquip || loc >= ItemLocType.ItemLocMax)
            {
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                await ETTask.CompletedTask;
                return;
            }

            bag.OnRecvItemSort(loc);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
