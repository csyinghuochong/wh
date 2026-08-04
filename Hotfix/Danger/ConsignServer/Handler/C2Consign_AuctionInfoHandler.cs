using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2Consign_AuctionInfoHandler : AMActorRpcHandler<Scene, C2Consign_AuctionInfoRequest, Consign2C_AuctionInfoResponse>
    {
        protected override async ETTask Run(Scene scene, C2Consign_AuctionInfoRequest request, Consign2C_AuctionInfoResponse response, Action reply)
        {
            ConsignSceneComponent paiMaiSceneComponent = scene.GetComponent<ConsignSceneComponent>();
            response.AuctionStatus  = paiMaiSceneComponent.AuctionStatus;
            response.AuctionPrice   = paiMaiSceneComponent.AuctionPrice;
            response.AuctionItem    = paiMaiSceneComponent.AuctionItem;
            response.AuctionNumber = paiMaiSceneComponent.AuctionItemNum;
            response.AuctionPlayer = paiMaiSceneComponent.AuctionPlayer;
            response.AuctionStart = paiMaiSceneComponent.AuctionStart;
            response.AuctionJoin = paiMaiSceneComponent.AuctionJoinList.Contains(request.UnitId);

            reply();
            await ETTask.CompletedTask;
        }
    }
}
