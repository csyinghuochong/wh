using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class M2Consign_AuctionJoinHandler : AMActorRpcHandler<Scene, M2Consign_AuctionJoinRequest, Consign2M_AuctionJoinResponse>
    {
        protected override async ETTask Run(Scene scene, M2Consign_AuctionJoinRequest request, Consign2M_AuctionJoinResponse response, Action reply)
        {
            ConsignSceneComponent paiMaiSceneComponent = scene.GetComponent<ConsignSceneComponent>();
            long returngold = (int)(paiMaiSceneComponent.AuctionStart * 0.1f);
            if (returngold <= 0)
            {
                response.Error = ErrorCode.ERR_AlreadyFinish;
                reply();
                return;
            }
            if (request.Gold < returngold)
            {
                response.Error = ErrorCode.ERR_GoldNotEnoughError;
                reply();
                return;
            }

            //paiMaiSceneComponent.AuctionStatus == 0 || paiMaiSceneComponent.AuctionStatus == -1
            if ( TimeHelper.ServerNow() >= paiMaiSceneComponent.AuctionStatus)
            {
                response.Error = ErrorCode.ERR_AlreadyFinish;
                reply();
                return;
            }


            if (!paiMaiSceneComponent.AuctionJoinList.Contains(request.UnitID))
            {
                paiMaiSceneComponent.AuctionJoinList.Add(request.UnitID);
                response.CostGold = returngold;
            }
            else
            {
                response.CostGold = 0;
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
