using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class M2Consign_SellHandler : AMActorRpcHandler<Scene, M2Consign_SellRequest, Consign2M_SellResponse>
    {

        protected override async ETTask Run(Scene scene, M2Consign_SellRequest request, Consign2M_SellResponse response, Action reply)
        {
            if (!ItemNewHelper.IsValidItem(request.ConsignItemInfo.BagInfo))
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }

            ConsignSceneComponent paiMaiComponent = scene.GetComponent<ConsignSceneComponent>();
            List<ConsignItemInfo> paiMaiItemsTo = new List<ConsignItemInfo>();
            paiMaiItemsTo.AddRange(paiMaiComponent.GetItemListByUser(request.UnitID, paiMaiComponent.dBPaiMainInfo_Consume.PaiMaiItemInfos));
            paiMaiItemsTo.AddRange(paiMaiComponent.GetItemListByUser(request.UnitID, paiMaiComponent.dBPaiMainInfo_Material.PaiMaiItemInfos));
            paiMaiItemsTo.AddRange(paiMaiComponent.GetItemListByUser(request.UnitID, paiMaiComponent.dBPaiMainInfo_Equipment.PaiMaiItemInfos));
            paiMaiItemsTo.AddRange(paiMaiComponent.GetItemListByUser(request.UnitID, paiMaiComponent.dBPaiMainInfo_Gemstone.PaiMaiItemInfos));

            long paimaiingGold = 0;
            for (int i = 0; i < paiMaiItemsTo.Count; i++)
            {
                paimaiingGold += (paiMaiItemsTo[i].Price * paiMaiItemsTo[i].BagInfo.ItemNum);
            }

            int openday = ServerHelper.GetOpenServerDay(false, scene.DomainZone());
            long todayGold = CommonConfig.GetPaiMaiTodayGold(openday);
            long sellGold = request.ConsignItemInfo.BagInfo.ItemNum * request.ConsignItemInfo.Price;
            if (paimaiingGold + request.PaiMaiTodayGold + sellGold >= todayGold)
            {
                response.Error = ErrorCode.ERR_PaiMaiSellLimit;
                reply();
                return;
            }

            //判定出售价格最低不能低于快捷拍卖列表的50%
            ConsignShopItemInfo shopinfo = scene.GetComponent<ConsignSceneComponent>().GetPaiMaiShopInfo(request.ConsignItemInfo.BagInfo.ItemID);
            if (shopinfo != null)
            {
                int nowPrice = (int)((float)request.ConsignItemInfo.Price);
                if (nowPrice < shopinfo.Price * 0.5f)
                {
                    response.Error = ErrorCode.Err_PaiMaiPriceLow;
                    reply();
                    return;
                }
            }
            // 上架紫色道具刷新该类型的道具
            LDItem ldItem = LDItemCategory.Instance.Get(request.ConsignItemInfo.BagInfo.ItemID);
            DBConsignInfo dBPaiMainInfo = scene.GetComponent<ConsignSceneComponent>().GetPaiMaiDBByType(ldItem.ItemType);
            if (dBPaiMainInfo == null)
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }

            dBPaiMainInfo.PaiMaiItemInfos.Add(request.ConsignItemInfo);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
