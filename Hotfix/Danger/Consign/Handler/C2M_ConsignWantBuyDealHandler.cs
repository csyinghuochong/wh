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
                    response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Invalid_Data);
                    reply();
                    return;
                }

                BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
                RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
                BagInfo bagInfo = bagComponentServer.GetItemByUId(request.BagInfoID);
                if (bagInfo == null || !ItemNewHelper.IsValideBagLoc(bagInfo.Loc))
                {
                    response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_NoItem);
                    reply();
                    return;
                }

                if (bagInfo.ItemID != request.ItemId)
                {
                    response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Invalid_Data);
                    reply();
                    return;
                }

                if (request.ItemType > 0 && bagInfo.ItemType != request.ItemType)
                {
                    response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Invalid_Data);
                    reply();
                    return;
                }

                if (bagInfo.ItemNum < request.SellNum)
                {
                    response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_NoItem);
                    reply();
                    return;
                }

                if (!ItemFlagEnum.IsTradable(bagInfo.GetEffectiveItemFlags()))
                {
                    response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                    reply();
                    return;
                }

                BagInfo mailBag = CloneHelper.ShallowClone(bagInfo);
                mailBag.ItemNum = request.SellNum;
                mailBag.BagInfoID = IdGenerater.Instance.GenerateId();
                mailBag.Loc = bagInfo.Loc;

                bagComponentServer.OnCostItemData(request.BagInfoID, request.SellNum, (ItemLocType)bagInfo.Loc);

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
