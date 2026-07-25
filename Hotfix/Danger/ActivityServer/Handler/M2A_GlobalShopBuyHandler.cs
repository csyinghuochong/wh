using System;

namespace ET
{

    [ActorMessageHandler]
    public class M2A_GlobalShopBuyHandler : AMActorRpcHandler<Scene, M2A_GlobalShopBuyRequest, A2M_GlobalShopBuyResponse>
    {
        protected override async ETTask Run(Scene scene, M2A_GlobalShopBuyRequest request, A2M_GlobalShopBuyResponse response, Action reply)
        {
            response.Error = scene.GetComponent<ActivitySceneComponent>()
                    .OnGlobalShopBuyRequest(request.ShopId, request.MysteryItemInfo);

            reply();
            await ETTask.CompletedTask;
        }
    }
}
