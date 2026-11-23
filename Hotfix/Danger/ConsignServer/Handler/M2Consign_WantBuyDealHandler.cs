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
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                reply();
                return;
            }

            long wantBuyKey = ConsignHelper.GetWantBuyKey(request.ItemType, request.ItemId);
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.WantBuy, wantBuyKey))
            {
                ConsignSceneComponent consignScene = scene.GetComponent<ConsignSceneComponent>();
                DBConsignWantBuy db = consignScene.GetWantBuyDB(request.ItemType, request.ItemId);
                int index = db == null ? -1 : db.WantBuyInfos.FindIndex(item => item.Id == request.WantBuyId);
                if (index < 0)
                {
                    response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Item_NotExist);
                    reply();
                    return;
                }

                ConsignWantBuyInfo info = db.WantBuyInfos[index];
                if (info.UserId == request.SellerUserId)
                {
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }

                if (request.SellNum > info.ItemNum)
                {
                    response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                    reply();
                    return;
                }

                info.ItemNum -= request.SellNum;
                if (info.ItemNum == 0)
                {
                    db.WantBuyInfos.RemoveAt(index);
                }

                response.BuyerUserId = info.UserId;
                response.DealNum = request.SellNum;
                response.Price = info.Price;
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
