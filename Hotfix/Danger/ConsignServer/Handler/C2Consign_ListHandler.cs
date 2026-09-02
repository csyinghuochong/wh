using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2Consign_ListHandler: AMActorRpcHandler<Scene, C2Consign_ListRequest, Consign2C_ListResponse>
    {
        protected override async ETTask Run(Scene scene, C2Consign_ListRequest request, Consign2C_ListResponse response, Action reply)
        {
            ConsignSceneComponent paiMaiComponent = scene.GetComponent<ConsignSceneComponent>();
            if (request.ListType == ConsignHelper.ListTypeTarget)
            {
                if (request.UserId <= 0)
                {
                    response.Error = ErrorCode.ERR_Parameter;
                    reply();
                    return;
                }

                await paiMaiComponent.CheckAllOverTime();
                List<ConsignItemInfo> targetList = paiMaiComponent.GetTargetShangJiaItems(request.UserId);
                paiMaiComponent.FillListPage(targetList, request.Page, response);
                reply();
                return;
            }

            int belongId = request.BelongId2;
            if (belongId <= 0)
            {
                response.Error = ErrorCode.ERR_Parameter;
                reply();
                return;
            }

            DBConsignInfo dBPaiMainInfo = paiMaiComponent.GetPaiMaiDBByBelongId(belongId);
            if (dBPaiMainInfo == null)
            {
                reply();
                return;
            }

            await paiMaiComponent.CheckOverTime(dBPaiMainInfo);
            List<ConsignItemInfo> publicList = paiMaiComponent.GetPublicShangJiaItems(dBPaiMainInfo.PaiMaiItemInfos);
            paiMaiComponent.FillListPage(publicList, request.Page, response);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
