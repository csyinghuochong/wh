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

            // Limit_Condition_1：购买所需角色等级。0 表示不限制。条件 2/3 暂未定义。
            int needLv = storeSellConfig.Limit_Condition_1;
            if (needLv > 0 && roleInfoComponentServer.RoleInfo.Lv < needLv)
            {
                response.Error = ErrorCode.ERR_LevelNoEnough;
                reply();
                return;
            }

            int shopId = request.ShopId > 0 ? request.ShopId : storeSellConfig.Shop_Id;
            bool isGlobalShop = LDShopCategory.Instance.Contain(shopId)
                    && LDShopCategory.Instance.Get(shopId).Type == ShopType.GlobalRandom;
            daily.TryRefreshShop(shopId);

            int buyNumber = request.BuyNumber;
            if (buyNumber <= 0)
            {
                buyNumber = 1;
            }

            // 本次购买数量：本刷新周期已购。上限 Limit_Num，商店刷新时清空。
            int periodBought = daily.GetBuyStorePeriod(storeSellConfig.Id);
            if (storeSellConfig.Limit_Num > 0 && buyNumber + periodBought > storeSellConfig.Limit_Num)
            {
                response.Error = ErrorCode.ERR_BuyMaxLimit;
                reply();
                return;
            }

            // 终身购买数量：累计已购，不随商店刷新清空。上限 Limit_Num_Forever。
            int foreverBought = daily.GetBuyStoreForever(storeSellConfig.Id);
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
                                ShopGoodsItem = new ShopGoodsItem()
                                {
                                    ShopGoodId = storeSellConfig.Id,
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

            // needPeriod：记入本次购买数量。needForever：记入终身购买数量。
            bool needPeriod = storeSellConfig.Limit_Num > 0;
            bool needForever = storeSellConfig.Limit_Num_Forever > 0;
            if (needPeriod || needForever)
            {
                daily.AddShopBuy(storeSellConfig.Id, buyNumber, needPeriod, needForever);
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
