using System;

namespace ET
{

    [ActorMessageHandler]
    public class Consign2M_AuctionOverHandler : AMActorRpcHandler<Unit, Consign2M_AuctionOverRequest, M2Consign_AuctionOverResponse>
    {
        protected override async ETTask Run(Unit unit, Consign2M_AuctionOverRequest request, M2Consign_AuctionOverResponse response, Action reply)
        {
            Log.Warning($"PaiMaiAuctionOver:  {unit.DomainZone()} {unit.Id}");
            
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            if (roleInfoComponentServer.RoleInfo.Gold < request.Price)
            {
                response.Error = ErrorCode.ERR_GoldNotEnoughError;
            }
            else
            {
                roleInfoComponentServer.UpdateRoleMoneySub( UserDataType.Gold, (request.Price * -1).ToString(), true, ItemGetWay.Auction );
                response.Error = ErrorCode.ERR_Success;
                Log.Warning($"扣除竞拍价：{unit.DomainZone()} {request.Price}");
            }
            reply();
            await ETTask.CompletedTask;
        }
    }
}
