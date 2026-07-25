using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ShopListHandler : AMActorLocationRpcHandler<Unit, C2M_ShopListRequest, M2C_ShopListResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ShopListRequest request, M2C_ShopListResponse response, Action reply)
        {
            int shopId = request.ShopId;
            if (!LDShopCategory.Instance.Contain(shopId))
            {
                response.Error = ErrorCode.ERR_Error;
                reply();
                await ETTask.CompletedTask;
                return;
            }

            LDShop shop = LDShopCategory.Instance.Get(shopId);
            if (shop.Type == ShopType.Fixed)
            {
                // Type1 固定：客户端读表
                response.Error = ErrorCode.ERR_Success;
                reply();
                await ETTask.CompletedTask;
                return;
            }

            // 全服随机：Map → Activity
            if (shop.Type == ShopType.GlobalRandom)
            {
                long activityServerId = DBHelper.GetActivityServerId(unit);
                A2M_GoldShopListResponse a2MResponse =
                        (A2M_GoldShopListResponse)await ActorMessageSenderComponent.Instance.Call(
                            activityServerId,
                            new M2A_GoldShopListRequest() { ShopId = shopId });

                if (a2MResponse == null || a2MResponse.Error != ErrorCode.ERR_Success)
                {
                    response.Error = a2MResponse?.Error ?? ErrorCode.ERR_NetWorkError;
                    reply();
                    await ETTask.CompletedTask;
                    return;
                }

                response.MysteryItemInfos = a2MResponse.MysteryItemInfos ?? new List<MysteryItemInfo>();
                response.Error = ErrorCode.ERR_Success;
                reply();
                await ETTask.CompletedTask;
                return;
            }

            // 个人随机 Type 2/3：日清货架
            if (shop.Type != ShopType.RandomRepeat && shop.Type != ShopType.RandomUnique)
            {
                response.Error = ErrorCode.ERR_Error;
                reply();
                await ETTask.CompletedTask;
                return;
            }

            RoleDailyDataComponentServer daily = unit.GetComponent<RoleDailyDataComponentServer>();
            if (daily == null)
            {
                daily = unit.AddComponent<RoleDailyDataComponentServer>();
            }

            response.MysteryItemInfos = daily.GetOrInitPersonalRandomShop(shopId);
            response.Error = ErrorCode.ERR_Success;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
