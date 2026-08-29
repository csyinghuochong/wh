using System;

namespace ET
{
    [ActorMessageHandler]
    public class M2Consign_WantBuyAddHandler : AMActorRpcHandler<Scene, M2Consign_WantBuyAddRequest, Consign2M_WantBuyAddResponse>
    {
        protected override async ETTask Run(Scene scene, M2Consign_WantBuyAddRequest request, Consign2M_WantBuyAddResponse response, Action reply)
        {
            ConsignWantBuyInfo wantBuy = request.WantBuyInfo;
            if (wantBuy == null || wantBuy.ItemId <= 0 || wantBuy.ItemNum <= 0 || wantBuy.Price <= 0)
            {
                response.Error = ErrorCode.ERR_Parameter;
                reply();
                return;
            }

            BagInfo checkItem = new BagInfo() { ItemType = wantBuy.ItemType, ItemID = wantBuy.ItemId, ItemNum = wantBuy.ItemNum };
            if (!ItemNewHelper.CheckValiedItem(checkItem))
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }

            ConsignSceneComponent consignScene = scene.GetComponent<ConsignSceneComponent>();
            DBConsignWantBuy db = consignScene.GetOrCreateWantBuyDB(wantBuy.ItemType, wantBuy.ItemId);
            db.WantBuyInfos.Add(wantBuy);

            reply();
            await ETTask.CompletedTask;
        }
    }
}
