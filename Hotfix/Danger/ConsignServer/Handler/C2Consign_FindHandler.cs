using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 查找装备所在拍卖行那一页(待实现)
    /// </summary>
    [ActorMessageHandler]
    public class C2Consign_FindHandler: AMActorRpcHandler<Scene, C2Consign_FindRequest, Consign2C_FindResponse>
    {
        protected override async ETTask Run(Scene scene, C2Consign_FindRequest request, Consign2C_FindResponse response, Action reply)
        {
            if (request.ItemType == 0)
            {
                response.Page = 0;
                reply();
                return;
            }
            ConsignSceneComponent paiMaiComponent = scene.GetComponent<ConsignSceneComponent>();
            DBConsignInfo dBPaiMainInfo = paiMaiComponent.GetPaiMaiDBByType(request.ItemType);
            if (dBPaiMainInfo == null)
            {
                response.Page = 0;
                reply();
                return;
            }

            List<ConsignItemInfo> PaiMaiItemInfo = dBPaiMainInfo.PaiMaiItemInfos;

            ConsignItemInfo paiMaiItemInfo = null;
            for (int i = 0; i < PaiMaiItemInfo.Count; i++)
            {
                if (PaiMaiItemInfo[i].Id == request.ConsignItemInfoId)
                {
                    paiMaiItemInfo = PaiMaiItemInfo[i];
                    break;
                }
            }

            if (paiMaiItemInfo == null)
            {
                response.Page = 0;
                reply();
                return;
            }

            int pagenum = int.Parse(LDGlobalValueCategory.Instance.Get(104).Value); //每页的数量
            LDItem ldItem = LDItemCategory.Instance.Get(paiMaiItemInfo.BagInfo.ItemID);
            for (int i = 0; i < PaiMaiItemInfo.Count; i++)
            {
                if (PaiMaiItemInfo[i].Id == paiMaiItemInfo.Id)
                {
                    response.Page = i / pagenum + 1;
                    reply();
                    return;
                }
            }
            response.Page = 0;
            reply();
            await ETTask.CompletedTask;
        }
    }
}