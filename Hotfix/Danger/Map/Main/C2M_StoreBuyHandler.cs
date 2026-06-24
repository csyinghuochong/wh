using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_StoreBuyHandler : AMActorLocationRpcHandler<Unit, C2M_StoreBuyRequest, M2C_StoreBuyResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_StoreBuyRequest request, M2C_StoreBuyResponse response, Action reply)
        {
            if (!LDShop_GoodsCategory.Instance.Contain(request.SellItemID))
            {
                reply();
                return;
            }

            LDShop_Goods storeSellConfig = LDShop_GoodsCategory.Instance.Get(request.SellItemID);
            if (storeSellConfig == null)
            {
                response.Error = ErrorCode.ERR_NetWorkError;
                reply();
                return;
            }

            int buynumber =  unit.GetComponent<RoleInfoComponentServer>().GetStoreBuy(storeSellConfig.Id);
            if (storeSellConfig.Buy_Limit_Num >0 && request.SellItemNum +  buynumber > storeSellConfig.Buy_Limit_Num)
            {
                response.Error = ErrorCode.ERR_BuyMaxLimit;
                reply();
                return;
            }

            int needCell = ItemHelper.GetNeedCell(storeSellConfig.Goods);
            if (unit.GetComponent<BagComponentServer>().GetBagLeftCell() < needCell)
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
            if (!unit.GetComponent<BagComponentServer>().CheckNeedItem(costItem))
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            RoleInfo roleInfo = unit.GetComponent<RoleInfoComponentServer>().RoleInfo;
            List<RewardItem> rewardItems = ItemHelper.GetRewardItems(storeSellConfig.Goods);

            unit.GetComponent<BagComponentServer>().OnCostItemData(costItem, ItemLocType.ItemLocBag, ItemGetWay.StoreBuy );
            unit.GetComponent<BagComponentServer>().OnAddItemData(rewardItems, string.Empty, $"{ItemGetWay.StoreBuy}_{TimeHelper.ServerNow()}");
            
            if (response.Error == ErrorCode.ERR_Success && storeSellConfig.Buy_Limit_Num > 0)
            {
                unit.GetComponent<RoleInfoComponentServer>().OnStoreBuy( storeSellConfig.Id );
            }
            reply();

            await ETTask.CompletedTask;
        }
    }
}
