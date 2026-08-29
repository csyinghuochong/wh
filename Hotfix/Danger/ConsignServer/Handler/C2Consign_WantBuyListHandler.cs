using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2Consign_WantBuyListHandler : AMActorRpcHandler<Scene, C2Consign_WantBuyListRequest, Consign2C_WantBuyListResponse>
    {
        protected override async ETTask Run(Scene scene, C2Consign_WantBuyListRequest request, Consign2C_WantBuyListResponse response, Action reply)
        {
            ConsignSceneComponent consignScene = scene.GetComponent<ConsignSceneComponent>();
            response.WantBuyInfo = consignScene.GetWantBuyList(request.ItemType, request.ItemId);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
