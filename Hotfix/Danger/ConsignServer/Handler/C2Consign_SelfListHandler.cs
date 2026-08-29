using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2Consign_SelfListHandler: AMActorRpcHandler<Scene, C2Consign_SelfListRequest, Consign2C_SelfListResponse>
    {
        protected override async ETTask Run(Scene scene, C2Consign_SelfListRequest request, Consign2C_SelfListResponse response, Action reply)
        {
            ConsignSceneComponent paiMaiComponent = scene.GetComponent<ConsignSceneComponent>();
            await paiMaiComponent.CheckAllOverTime();
            response.ConsignItemInfo = paiMaiComponent.GetUserShangJiaItems(request.UserId);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
