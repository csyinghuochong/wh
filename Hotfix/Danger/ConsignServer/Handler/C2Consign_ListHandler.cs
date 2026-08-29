using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2Consign_ListHandler: AMActorRpcHandler<Scene, C2Consign_ListRequest, Consign2C_ListResponse>
    {
        protected override async ETTask Run(Scene scene, C2Consign_ListRequest request, Consign2C_ListResponse response, Action reply)
        {
            ConsignSceneComponent paiMaiComponent = scene.GetComponent<ConsignSceneComponent>();

            int belongId = request.BelongId2 ;
            DBConsignInfo dBPaiMainInfo = paiMaiComponent.GetPaiMaiDBByBelongId(belongId);
            if (dBPaiMainInfo == null)
            {
                reply();
                return;
            }

            await paiMaiComponent.CheckOverTime(dBPaiMainInfo);
            paiMaiComponent.FillListPage(dBPaiMainInfo.PaiMaiItemInfos, request.Page, response);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
