using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2Consign_SearchHandler: AMActorRpcHandler<Scene, C2Consign_SearchRequest, Consign2C_SearchResponse>
    {
        protected override async ETTask Run(Scene scene, C2Consign_SearchRequest request, Consign2C_SearchResponse response, Action reply)
        {
            if (request.ItemId <= 0 && request.ItemType <= 0)
            {
                response.Error = ErrorCode.ERR_Parameter;
                reply();
                return;
            }

            ConsignSceneComponent paiMaiComponent = scene.GetComponent<ConsignSceneComponent>();
            if (request.BelongId > 0)
            {
                SearchInDb(paiMaiComponent.GetPaiMaiDBByBelongId(request.BelongId), request, response);
            }
            else
            {
                foreach (DBConsignInfo db in paiMaiComponent.ShangJiaByBelongId.Values)
                {
                    SearchInDb(db, request, response);
                    if (response.ConsignItemInfos.Count > 200)
                    {
                        break;
                    }
                }
            }

            reply();
            await ETTask.CompletedTask;
        }

        private static void SearchInDb(DBConsignInfo dBPaiMainInfo, C2Consign_SearchRequest request, Consign2C_SearchResponse response)
        {
            if (dBPaiMainInfo?.PaiMaiItemInfos == null)
            {
                return;
            }

            for (int i = 0; i < dBPaiMainInfo.PaiMaiItemInfos.Count; i++)
            {
                ConsignItemInfo paiMaiItemInfo = dBPaiMainInfo.PaiMaiItemInfos[i];
                if (paiMaiItemInfo?.BagInfo == null || ConsignHelper.IsDesignatedShangJia(paiMaiItemInfo))
                {
                    continue;
                }

                if (request.ItemId > 0 && paiMaiItemInfo.BagInfo.ItemID != request.ItemId)
                {
                    continue;
                }

                if (request.ItemType > 0 && paiMaiItemInfo.BagInfo.ItemType != request.ItemType)
                {
                    continue;
                }

                response.ConsignItemInfos.Add(paiMaiItemInfo);
                if (response.ConsignItemInfos.Count > 200)
                {
                    return;
                }
            }
        }
    }
}
