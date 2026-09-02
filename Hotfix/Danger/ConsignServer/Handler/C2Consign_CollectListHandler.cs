using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2Consign_CollectListHandler : AMActorRpcHandler<Scene, C2Consign_CollectListRequest, Consign2C_CollectListResponse>
    {
        protected override async ETTask Run(Scene scene, C2Consign_CollectListRequest request, Consign2C_CollectListResponse response, Action reply)
        {
            if (request.UserId <= 0)
            {
                response.Error = ErrorCode.ERR_Parameter;
                reply();
                return;
            }

            ConsignSceneComponent consignScene = scene.GetComponent<ConsignSceneComponent>();
            response.ConsignItemInfo = await consignScene.GetCollectList(request.UserId);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
