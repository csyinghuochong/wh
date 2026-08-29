using System;

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

            ConsignItemInfo consignItem = request.ConsignItemInfo;
            if (consignItem.BelongId <= 0 && consignItem.BagInfo != null)
            {
                consignItem.BelongId = ItemNewHelper.GetConsignBelongId(consignItem.BagInfo);
            }

            DBConsignInfo dBPaiMainInfo = scene.GetComponent<ConsignSceneComponent>().GetOrCreatePaiMaiDBByBelongId(consignItem.BelongId);

            dBPaiMainInfo.PaiMaiItemInfos.Add(consignItem);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
