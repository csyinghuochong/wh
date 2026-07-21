using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_ItemDestoryHandler : AMActorLocationRpcHandler<Unit, C2M_ItemDestoryRequest, M2C_ItemDestoryResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ItemDestoryRequest request, M2C_ItemDestoryResponse response, Action reply)
        {
            long bagInfoID = request.OperateBagID;
            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            BagInfo useBagInfo = bag.GetItemByLoc(ItemLocType.ItemLocBag, bagInfoID);
            if (useBagInfo == null)
            {
                reply();
                return;
            }
            bag.OnCostItemData(bagInfoID, 1);

            reply();
            await ETTask.CompletedTask;
        }
    }
}
