using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class M2Consign_XiaJiaHandler : AMActorRpcHandler<Scene, M2Consign_XiaJiaRequest, Consign2M_XiaJiaResponse>
    {
        protected override async ETTask Run(Scene scene, M2Consign_XiaJiaRequest request, Consign2M_XiaJiaResponse response, Action reply)
        {
            DBConsignInfo dBPaiMainInfo = scene.GetComponent<ConsignSceneComponent>().GetPaiMaiDBByType(request.ItemType);
            if (dBPaiMainInfo == null)
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }

            List<ConsignItemInfo> paiMaiItemInfo = dBPaiMainInfo.PaiMaiItemInfos;
            for (int i = paiMaiItemInfo.Count - 1; i >= 0; i--)
            {
                if (paiMaiItemInfo[i].Id == request.ConsignItemInfoId)
                {
                    ConsignItemInfo paiMaiItemInfo1 = paiMaiItemInfo[i];
                    response.ConsignItemInfo = paiMaiItemInfo1;
                    paiMaiItemInfo.RemoveAt(i);
                    break;
                }
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
