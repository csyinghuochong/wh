using System;

namespace ET
{
    [ActorMessageHandler]
    public class M2Consign_AuctionPriceHandler : AMActorRpcHandler<Scene, M2Consign_AuctionPriceRequest, Consign2M_AuctionPriceResponse>
    {
        protected override async ETTask Run(Scene scene, M2Consign_AuctionPriceRequest message, Consign2M_AuctionPriceResponse response, Action reply)
        {
            ConsignSceneComponent paiMaiSceneComponent = scene.GetComponent<ConsignSceneComponent>();
            if (TimeHelper.ServerNow() >= paiMaiSceneComponent.AuctionStatus)
            {
                response.Error = ErrorCode.Err_Auction_Finish;
                reply();
                return;
            }
            if (paiMaiSceneComponent.AuctionPrice >= message.Price)
            {
                response.Error = ErrorCode.Err_Auction_Low;
                reply();
                return;
            }

            paiMaiSceneComponent.AuctionPrice = message.Price;
            paiMaiSceneComponent.AuctioUnitId = message.UnitID;
            paiMaiSceneComponent.AuctionPlayer = message.AuctionPlayer;

            AuctionRecord keyValuePair = new AuctionRecord();
            keyValuePair.UnionId = message.UnitID;
            keyValuePair.Price = message.Price;
            keyValuePair.Time = TimeHelper.ServerNow();
            keyValuePair.Occ = message.Occ;
            keyValuePair.PlayerName = message.AuctionPlayer;
            paiMaiSceneComponent.AuctionRecords.Add(keyValuePair);
            paiMaiSceneComponent.ExtendOverTime();
            ServerMessageHelper.SendServerMessage(DBHelper.GetChatServerId(scene.DomainZone()), NoticeType.PaiMaiAuction,
                $"{paiMaiSceneComponent.AuctionItem}_{paiMaiSceneComponent.AuctionItemNum}_{message.Price}_{paiMaiSceneComponent.AuctionPlayer}_1").Coroutine();
            reply();
            await ETTask.CompletedTask;
        }
    }
}
