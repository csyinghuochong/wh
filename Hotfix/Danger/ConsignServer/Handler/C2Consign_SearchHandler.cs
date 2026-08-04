using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2Consign_SearchHandler: AMActorRpcHandler<Scene, C2Consign_SearchRequest, Consign2C_SearchResponse>
    {
        protected override async ETTask Run(Scene scene, C2Consign_SearchRequest request, Consign2C_SearchResponse response, Action reply)
        {
            if (request.FindTypeList.Count <= 0)
            {
                reply();
                return;
            }

            if (request.FindItemIdList.Count <= 0)
            {
                reply();
                return;
            }

            ConsignSceneComponent paiMaiComponent = scene.GetComponent<ConsignSceneComponent>();
            foreach (int type in request.FindTypeList)
            {
                DBConsignInfo dBPaiMainInfo = paiMaiComponent.GetPaiMaiDBByType(type);
                if (dBPaiMainInfo == null)
                {
                    reply();
                    return;
                }

                foreach (ConsignItemInfo paiMaiItemInfo in dBPaiMainInfo.PaiMaiItemInfos)
                {
                    if (request.FindItemIdList.Contains(paiMaiItemInfo.BagInfo.ItemID))
                    {
                        response.ConsignItemInfos.Add(paiMaiItemInfo);

                        if (response.ConsignItemInfos.Count > 200)
                        {
                            break;
                        }
                    }
                }
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}