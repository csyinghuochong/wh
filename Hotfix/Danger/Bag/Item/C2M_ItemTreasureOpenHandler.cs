using System;
using System.Collections.Generic;

namespace ET
{

    //藏宝图
    [ActorMessageHandler]
    public class C2M_ItemTreasureOpenHandler : AMActorLocationRpcHandler<Unit, C2M_ItemTreasureOpenRequest, M2C_ItemTreasureOpenResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ItemTreasureOpenRequest request, M2C_ItemTreasureOpenResponse response, Action reply)
        {
            BagInfo useBagInfo = unit.GetComponent<BagComponentServer>().GetItemByLoc(ItemLocType.ItemLocBag, request.OperateBagID);
            if (useBagInfo == null)
            {
                response.Error = ErrorCode.ERR_ItemUseError;
                reply();
                return;
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
