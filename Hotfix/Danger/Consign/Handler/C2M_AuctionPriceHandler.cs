using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_AuctionPriceHandler : AMActorLocationRpcHandler<Unit, C2M_AuctionPriceRequest, M2C_AuctionPriceResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_AuctionPriceRequest request, M2C_AuctionPriceResponse response, Action reply)
        {
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;
            if (roleInfo.Gold < request.Price)
            {
                response.Error = ErrorCode.ERR_GoldNotEnoughError;
                reply();
                return;
            }
            M2Consign_AuctionPriceRequest message = new M2Consign_AuctionPriceRequest()
            {
                Price = request.Price,
                UnitID = unit.Id, 
                Occ = roleInfo.Occ,
                AuctionPlayer = roleInfo.Name,
            };
            long paimaiserverid = DBHelper.GetPaiMaiServerId(unit);
            Consign2M_AuctionPriceResponse r_GameStatusResponse = (Consign2M_AuctionPriceResponse)await ActorMessageSenderComponent.Instance.Call
                    (paimaiserverid, message);

            response.Error = r_GameStatusResponse.Error;
            reply();
            return;
        }
    }
}
