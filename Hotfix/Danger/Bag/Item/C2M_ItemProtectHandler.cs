using System;
using System.Collections.Generic;


namespace ET
{

    [ActorMessageHandler]
    public class C2M_ItemProtectHandler : AMActorLocationRpcHandler<Unit, C2M_ItemProtectRequest, M2C_ItemProtectResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ItemProtectRequest request, M2C_ItemProtectResponse response, Action reply)
        {
            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            BagInfo bagInfo = bag.GetItemByLoc(ItemLocType.ItemLocBag, request.OperateBagID);
            if (bagInfo == null)
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }
            //bagInfo.IsProtect = request.IsProtect;
            ItemAddHelper.OnItemUpdate(unit, bagInfo);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
