using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2Consign_ItemListHandler : AMActorRpcHandler<Scene, C2Consign_ItemListRequest, Consign2C_ItemListResponse>
    {
        protected override async ETTask Run(Scene scene, C2Consign_ItemListRequest request, Consign2C_ItemListResponse response, Action reply)
        {
            ConsignSceneComponent consignScene = scene.GetComponent<ConsignSceneComponent>();
            await consignScene.CheckAllOverTime();
            response.ConsignList = consignScene.GetShangJiaItemsByItem(request.ItemType, request.ItemId);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
