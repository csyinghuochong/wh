using System;

namespace ET
{
    [ActorMessageHandler]
    public class M2Consign_WantBuyCancelHandler : AMActorRpcHandler<Scene, M2Consign_WantBuyCancelRequest, Consign2M_WantBuyCancelResponse>
    {
        protected override async ETTask Run(Scene scene, M2Consign_WantBuyCancelRequest request, Consign2M_WantBuyCancelResponse response, Action reply)
        {
            if (request.WantBuyId <= 0 || request.UserId <= 0)
            {
                response.Error = ErrorCode.ERR_Parameter;
                reply();
                return;
            }

            ConsignSceneComponent consignScene = scene.GetComponent<ConsignSceneComponent>();
            ConsignWantBuyInfo info = consignScene.FindWantBuy(request.WantBuyId);
            if (info == null)
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }

            if (info.UserId != request.UserId)
            {
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            long wantBuyKey = ConsignHelper.GetWantBuyKey(info.ItemType, info.ItemId);
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.WantBuy, wantBuyKey))
            {
                ConsignWantBuyInfo cancelled = consignScene.CancelWantBuy(info.ItemType, info.ItemId, request.WantBuyId, request.UserId);
                if (cancelled == null)
                {
                    response.Error = ErrorCode.ERR_ItemNotExist;
                    reply();
                    return;
                }

                response.RefundNum = cancelled.ItemNum;
                response.Price = cancelled.Price;
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
