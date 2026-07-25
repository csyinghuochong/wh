using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ShopBuyHandler : AMActorLocationRpcHandler<Unit, C2M_ShopBuyRequest, M2C_ShopBuyResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ShopBuyRequest request, M2C_ShopBuyResponse response, Action reply)
        {
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            RoleDailyDataComponentServer daily = unit.GetComponent<RoleDailyDataComponentServer>();

            if (!LDShop_GoodsCategory.Instance.Contain(request.ShopGoodsID))
            {
                reply();
                return;
            }

            LDShop_Goods storeSellConfig = LDShop_GoodsCategory.Instance.Get(request.ShopGoodsID);
            if (storeSellConfig.Is_Close > 0)
            {
                reply();
                return;
            }

            int shopId = request.ShopId > 0 ? request.ShopId : storeSellConfig.ShopId;
            bool isGlobalShop = LDShopCategory.Instance.Contain(shopId)
                    && LDShopCategory.Instance.Get(shopId).Type == ShopType.GlobalRandom;

            int buyNumber = request.BuyNumber;
            if (buyNumber <= 0)
            {
                buyNumber = 1;
            }
            else if (buyNumber > 100)
            {
                buyNumber = 100;
            }

            int periodBought = daily?.GetBuyStorePeriod(storeSellConfig.Id) ?? roleInfoComponentServer.GetStoreBuy(storeSellConfig.Id);
            if (storeSellConfig.Limit_Num > 0 && buyNumber + periodBought > storeSellConfig.Limit_Num)
            {
                response.Error = ErrorCode.ERR_BuyMaxLimit;
                reply();
                return;
            }

            int foreverBought = daily?.GetBuyStoreForever(storeSellConfig.Id) ?? 0;
            if (storeSellConfig.Limit_Num_Forever > 0 && buyNumber + foreverBought > storeSellConfig.Limit_Num_Forever)
            {
                response.Error = ErrorCode.ERR_BuyMaxLimit;
                reply();
                return;
            }

            List<RewardItem> rewardItems = ItemNewHelper.GetRewardItems(storeSellConfig.Goods);
            ItemNewHelper.ScaleRewardItems(rewardItems, buyNumber);
            if (bag.GetBagLeftCell() < ItemNewHelper.GetNeedCell(rewardItems))
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }

            List<RewardItem> costItems = ItemNewHelper.GetShopConsumeItems(storeSellConfig, buyNumber);
            if (costItems.Count > 0 && !bag.CheckNeedItem(costItems))
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            // 全服商店：先向活动服扣全服货架库存
            if (isGlobalShop)
            {
                long activityServerId = DBHelper.GetActivityServerId(unit);
                A2M_GlobalShopBuyResponse a2MResponse =
                        (A2M_GlobalShopBuyResponse)await ActorMessageSenderComponent.Instance.Call(
                            activityServerId,
                            new M2A_GlobalShopBuyRequest()
                            {
                                ShopId = shopId,
                                MysteryItemInfo = new MysteryItemInfo()
                                {
                                    MysteryId = storeSellConfig.Id,
                                    ItemNumber = buyNumber,
                                },
                            });

                if (a2MResponse == null || a2MResponse.Error != ErrorCode.ERR_Success)
                {
                    response.Error = a2MResponse?.Error ?? ErrorCode.ERR_NetWorkError;
                    reply();
                    return;
                }
            }

            if (costItems.Count > 0
                && !bag.OnCostItemData(costItems, ItemLocType.ItemLocBag, ItemGetWay.StoreBuy))
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            long storeBuyTime = TimeHelper.ServerNow();
            if (rewardItems.Count > 0)
            {
                bag.OnAddItemData(rewardItems, string.Empty, $"{ItemGetWay.StoreBuy}_{storeBuyTime}");
            }

            bool needPeriod = storeSellConfig.Limit_Num > 0;
            bool needForever = storeSellConfig.Limit_Num_Forever > 0;
            if (needPeriod || needForever)
            {
                if (daily != null)
                {
                    daily.AddShopBuy(storeSellConfig.Id, buyNumber, needPeriod, needForever);
                }
                else if (needPeriod)
                {
                    roleInfoComponentServer.OnShopBuy(storeSellConfig.Id, buyNumber);
                }
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
