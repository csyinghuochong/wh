using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_StoreBuyHandler : AMActorLocationRpcHandler<Unit, C2M_StoreBuyRequest, M2C_StoreBuyResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_StoreBuyRequest request, M2C_StoreBuyResponse response, Action reply)
        {
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            BagComponentServer bag = unit.GetComponent<BagComponentServer>();

            LDShop_Goods storeSellConfig = LDShop_GoodsCategory.Instance.Get(request.SellItemID);
            if (storeSellConfig == null)
            {
                reply();
                return;
            }

            int buynumber = roleInfoComponentServer.GetStoreBuy(storeSellConfig.Id);
            if (storeSellConfig.Buy_Limit_Num >0 && request.SellItemNum +  buynumber > storeSellConfig.Buy_Limit_Num)
            {
                response.Error = ErrorCode.ERR_BuyMaxLimit;
                reply();
                return;
            }

            int needCell = ItemNewHelper.GetNeedCell(storeSellConfig.Goods);
            if (bag.GetBagLeftCell() < needCell)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }

            //购买限制
            if (request.SellItemNum <= 0) {
                request.SellItemNum = 1;
            }

            if (request.SellItemNum >= 100)
            {
                request.SellItemNum = 100;
            }

            string costItem = $"{storeSellConfig.Consume_Type}_{storeSellConfig.Consume_Id}_{storeSellConfig.Consume_Value}";
            if (!bag.CheckNeedItem(costItem))
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;
            List<RewardItem> rewardItems = ItemNewHelper.GetRewardItems(storeSellConfig.Goods);

            long storeBuyTime = TimeHelper.ServerNow();
            bag.OnCostItemData(costItem, ItemLocType.ItemLocBag, ItemGetWay.StoreBuy );
            bag.OnAddItemData(rewardItems, string.Empty, $"{ItemGetWay.StoreBuy}_{storeBuyTime}");
            
            if (response.Error == ErrorCode.ERR_Success && storeSellConfig.Buy_Limit_Num > 0)
            {
                roleInfoComponentServer.OnStoreBuy( storeSellConfig.Id );
            }
            reply();

            await ETTask.CompletedTask;
        }
    }
}
