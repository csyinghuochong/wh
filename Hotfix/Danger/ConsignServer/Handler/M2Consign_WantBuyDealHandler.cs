using System;

namespace ET
{
    [ActorMessageHandler]
    public class M2Consign_WantBuyDealHandler : AMActorRpcHandler<Scene, M2Consign_WantBuyDealRequest, Consign2M_WantBuyDealResponse>
    {
        protected override async ETTask Run(Scene scene, M2Consign_WantBuyDealRequest request, Consign2M_WantBuyDealResponse response, Action reply)
        {
            if (request.WantBuyId <= 0 || request.SellNum <= 0 || request.ItemId <= 0)
            {
                response.Error = ErrorCode.ERR_Parameter;
                reply();
                return;
            }

            long wantBuyKey = ConsignHelper.GetWantBuyKey(request.ItemType, request.ItemId);
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.WantBuy, wantBuyKey))
            {
                ConsignSceneComponent consignScene = scene.GetComponent<ConsignSceneComponent>();
                ConsignWantBuyInfo info = consignScene.FindWantBuy(request.ItemType, request.ItemId, request.WantBuyId);
                if (info == null)
                {
                    response.Error = ErrorCode.ERR_ItemNotExist;
                    reply();
                    return;
                }

                if (info.UserId == request.SellerUserId)
                {
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }

                if (request.SellNum > info.ItemNum)
                {
                    response.Error = ErrorCode.ERR_Parameter;
                    reply();
                    return;
                }

                int price = info.Price;
                long buyerUserId = info.UserId;
                ConsignWantBuyInfo dealt = consignScene.DealWantBuy(request.ItemType, request.ItemId, request.WantBuyId, request.SellNum, request.SellerUserId);
                if (dealt == null)
                {
                    response.Error = ErrorCode.ERR_ItemNotExist;
                    reply();
                    return;
                }

                response.BuyerUserId = buyerUserId;
                response.DealNum = request.SellNum;
                response.Price = price;
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
