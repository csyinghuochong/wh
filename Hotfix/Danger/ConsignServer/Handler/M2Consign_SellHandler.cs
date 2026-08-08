using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class M2Consign_SellHandler : AMActorRpcHandler<Scene, M2Consign_SellRequest, Consign2M_SellResponse>
    {

        protected override async ETTask Run(Scene scene, M2Consign_SellRequest request, Consign2M_SellResponse response, Action reply)
        {
            if (!ItemNewHelper.CheckValiedItem(request.ConsignItemInfo.BagInfo))
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
