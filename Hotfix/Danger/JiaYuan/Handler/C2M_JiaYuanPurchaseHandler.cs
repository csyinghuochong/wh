using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_JiaYuanPurchaseHandler : AMActorLocationRpcHandler<Unit, C2M_JiaYuanPurchaseRequest, M2C_JiaYuanPurchaseResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_JiaYuanPurchaseRequest request, M2C_JiaYuanPurchaseResponse response, Action reply)
        {
            JiaYuanComponentServer jiaYuanComponentServer = unit.GetComponent<JiaYuanComponentServer>();
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            List<JiaYuanPurchaseItem> purchaselist = jiaYuanComponentServer.PurchaseItemList_7;
            JiaYuanPurchaseItem jiaYuanPurchaseItem = null;
            long serverTime = TimeHelper.ServerNow();
            for (int i = purchaselist.Count - 1; i >= 0; i--)
            {
                if (purchaselist[i].PurchaseId == request.PurchaseId)
                {
                    jiaYuanPurchaseItem = purchaselist[i];
                    purchaselist.RemoveAt(i);
                    break;
                }
                if (purchaselist[i].EndTime < serverTime)
                {
                    purchaselist.RemoveAt(i);
                }
            }
            if (jiaYuanPurchaseItem == null)
            {
                response.Error = ErrorCode.ERR_NetWorkError;
                reply();
                return;
            }
            if (bagComponentServer.GetItemNumber(ItemBigType.Type_Item, request.ItemId) < 1)
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            jiaYuanComponentServer.AddJiaYuanFund(jiaYuanPurchaseItem.BuyZiJin);
            bagComponentServer.OnCostItemData($"{request.ItemId};1", ItemLocType.ItemLocBag, ItemGetWay.JiaYuanCost  );
            response.PurchaseItemList = jiaYuanComponentServer.PurchaseItemList_7;
            DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(unit), unit.Id, jiaYuanComponentServer).Coroutine();
            reply();
            await ETTask.CompletedTask;
        }
    }
}
