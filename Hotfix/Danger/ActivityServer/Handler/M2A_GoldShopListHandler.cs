using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class M2A_GoldShopListHandler : AMActorRpcHandler<Scene, M2A_GoldShopListRequest, A2M_GoldShopListResponse>
    {
        protected override async ETTask Run(Scene scene, M2A_GoldShopListRequest request, A2M_GoldShopListResponse response, Action reply)
        {
            ActivitySceneComponent activitySceneComponent = scene.GetComponent<ActivitySceneComponent>();
            int shopId = request.ShopId;

            if (!LDShopCategory.Instance.Contain(shopId) ||
                LDShopCategory.Instance.Get(shopId).Type != ShopType.GlobalRandom)
            {
                response.Error = ErrorCode.ERR_Error;
                reply();
                await ETTask.CompletedTask;
                return;
            }

            List<MysteryItemInfo> shopList = activitySceneComponent.GetGlobalRandomShopList(shopId);
            if (shopList.Count == 0)
            {
                LogHelper.LogDebug($"全服随机商店为空: zone={scene.DomainZone()} shopId={shopId}");
                activitySceneComponent.InitGlobalRandomShop();
                activitySceneComponent.SaveDB();
                shopList = activitySceneComponent.GetGlobalRandomShopList(shopId);
            }

            response.MysteryItemInfos = shopList;
            response.Error = ErrorCode.ERR_Success;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
