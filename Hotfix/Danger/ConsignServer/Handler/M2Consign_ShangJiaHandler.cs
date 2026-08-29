using System;

namespace ET
{

    [ActorMessageHandler]
    public class M2Consign_ShangJiaHandler : AMActorRpcHandler<Scene, M2Consign_ShangJiaRequest, Consign2M_ShangJiaResponse>
    {

        protected override async ETTask Run(Scene scene, M2Consign_ShangJiaRequest request, Consign2M_ShangJiaResponse response, Action reply)
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
