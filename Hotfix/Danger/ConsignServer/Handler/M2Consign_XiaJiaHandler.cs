using System;

namespace ET
{

    [ActorMessageHandler]
    public class M2Consign_XiaJiaHandler : AMActorRpcHandler<Scene, M2Consign_XiaJiaRequest, Consign2M_XiaJiaResponse>
    {
        protected override async ETTask Run(Scene scene, M2Consign_XiaJiaRequest request, Consign2M_XiaJiaResponse response, Action reply)
        {
            ConsignItemInfo paiMaiItemInfo1 = scene.GetComponent<ConsignSceneComponent>()
                    .RemoveShangJiaItem(request.BelongId, request.ConsignItemInfoId);
            if (paiMaiItemInfo1 != null)
            {
                response.ConsignItemInfo = paiMaiItemInfo1;
            }
            else
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
