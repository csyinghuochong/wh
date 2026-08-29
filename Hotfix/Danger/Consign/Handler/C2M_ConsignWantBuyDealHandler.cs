using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ConsignWantBuyDealHandler : AMActorLocationRpcHandler<Unit, C2M_ConsignWantBuyDealRequest, M2C_ConsignWantBuyDealResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ConsignWantBuyDealRequest request, M2C_ConsignWantBuyDealResponse response, Action reply)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Sell, unit.Id))
            {
                if (request.WantBuyId <= 0 || request.SellNum <= 0 || request.BagInfoID <= 0)
                {
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }

                BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
                RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
                BagInfo bagInfo = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocBag, request.BagInfoID);
                if (bagInfo == null)
                {
                    response.Error = ErrorCode.ERR_ItemNotEnoughError;
                    reply();
                    return;
                }

                if (bagInfo.ItemID != request.ItemId)
                {
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }

                if (request.ItemType > 0 && bagInfo.ItemType != request.ItemType)
                {
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }

                if (bagInfo.ItemNum < request.SellNum)
                {
                    response.Error = ErrorCode.ERR_ItemNotEnoughError;
                    reply();
                    return;
                }

                if (bagInfo.IsBinding())
                {
                    response.Error = ErrorCode.ERR_ItemBing;
                    reply();
                    return;
                }

                BagInfo mailBag = CloneHelper.ShallowClone(bagInfo);
                mailBag.ItemNum = request.SellNum;
                mailBag.BagInfoID = IdGenerater.Instance.GenerateId();
                mailBag.Loc = (int)ItemLocType.ItemLocBag;

                bagComponentServer.OnCostItemData(request.BagInfoID, request.SellNum);

                long paimaiServerId = DBHelper.GetPaiMaiServerId(unit);
                Consign2M_WantBuyDealResponse dealResponse = (Consign2M_WantBuyDealResponse)await ActorMessageSenderComponent.Instance.Call(
                    paimaiServerId, new M2Consign_WantBuyDealRequest()
                    {
                        WantBuyId = request.WantBuyId,
                        ItemType = request.ItemType > 0 ? request.ItemType : bagInfo.ItemType,
                        ItemId = request.ItemId,
                        SellNum = request.SellNum,
                        SellerUserId = roleInfoComponentServer.RoleInfo.UserId,
                    });

                if (dealResponse.Error != ErrorCode.ERR_Success)
                {
                    bagComponentServer.OnAddItemData(mailBag, $"{ItemGetWay.XiaJia}_{TimeHelper.ServerNow()}");
                    response.Error = dealResponse.Error;
                    reply();
                    return;
                }

                long gold = ConsignHelper.GetConsignSellerGold((long)dealResponse.Price * dealResponse.DealNum);
                if (gold > 0)
                {
                    roleInfoComponentServer.UpdateRoleData(UserDataType.Gold, gold.ToString(), true, ItemGetWay.PaiMaiSell);
                }

                MailHelp.SendWantBuyItemMail(dealResponse.BuyerUserId, mailBag).Coroutine();
                reply();
                await ETTask.CompletedTask;
            }
        }
    }
}
