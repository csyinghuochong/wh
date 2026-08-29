using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2Consign_SearchHandler: AMActorRpcHandler<Scene, C2Consign_SearchRequest, Consign2C_SearchResponse>
    {
        protected override async ETTask Run(Scene scene, C2Consign_SearchRequest request, Consign2C_SearchResponse response, Action reply)
        {
            if (request.FindItemIdList.Count <= 0)
            {
                reply();
                return;
            }

            ConsignSceneComponent paiMaiComponent = scene.GetComponent<ConsignSceneComponent>();
            if (request.FindBelongIdList.Count <= 0)
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
            else
            {
                foreach (int belongId in request.FindBelongIdList)
                {
                    DBConsignInfo dBPaiMainInfo = paiMaiComponent.GetPaiMaiDBByBelongId(belongId);
                    SearchInDb(dBPaiMainInfo, request, response);
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

            foreach (ConsignItemInfo paiMaiItemInfo in dBPaiMainInfo.PaiMaiItemInfos)
            {
                if (request.FindItemIdList.Contains(paiMaiItemInfo.BagInfo.ItemID))
                {
                    response.ConsignItemInfos.Add(paiMaiItemInfo);
                    if (response.ConsignItemInfos.Count > 200)
                    {
                        return;
                    }
                }
            }
        }
    }
}
