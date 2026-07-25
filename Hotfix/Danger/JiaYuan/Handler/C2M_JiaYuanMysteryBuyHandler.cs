using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_JiaYuanMysteryBuyHandler : AMActorLocationRpcHandler<Unit, C2M_JiaYuanMysteryBuyRequest, M2C_JiaYuanMysteryBuyResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_JiaYuanMysteryBuyRequest request, M2C_JiaYuanMysteryBuyResponse response, Action reply)
        {
            int mysteryId = request.MysteryId;

            if (request.ProductId != -1)
            {
                List<ShopGoodsItem> jiayuanList = new List<ShopGoodsItem>();
                if (unit.GetComponent<JiaYuanComponentServer>().NowOpenNpcId == 30000001)
                {
                    jiayuanList = unit.GetComponent<JiaYuanComponentServer>().PlantGoods_7;
                }

                if (unit.GetComponent<JiaYuanComponentServer>().NowOpenNpcId == 30000013)
                {
                    jiayuanList = unit.GetComponent<JiaYuanComponentServer>().JiaYuanStore;
                }

                int errorCode = unit.GetComponent<JiaYuanComponentServer>().OnMysteryBuyRequest(request.ProductId, jiayuanList);
                if (errorCode != ErrorCode.ERR_Success)
                {
                    response.Error = errorCode;
                    reply();
                    return;
                }
                response.ShopGoodsItems = jiayuanList;
            }
            //unit.GetComponent<RoleInfoComponentServer>().OnMysteryBuy(mysteryId);
            //扣除货币添加对应道具
            /*
            unit.GetComponent<BagComponentServer>().OnCostItemData($"{mysteryConfig.SellType};{mysteryConfig.SellValue}", ItemLocType.ItemLocBag, ItemGetWay.JiaYuanCost );
            unit.GetComponent<BagComponentServer>().OnAddItemData($"{mysteryConfig.SellItemID};1",
                $"{ItemGetWay.MysteryBuy}_{TimeHelper.ServerNow()}");
                */
            
            reply();
            await ETTask.CompletedTask;
        }
    }
}
