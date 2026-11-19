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
            BagInfo useBagInfo = bag.GetItemByUId(bagInfoID);
            if (useBagInfo == null || !ItemNewHelper.IsValideBagLoc(useBagInfo.Loc))
            {
                reply();
                return;
            }
            bag.OnCostItemData(bagInfoID, 1, (ItemLocType)useBagInfo.Loc);

            reply();
            await ETTask.CompletedTask;
        }
    }
}
