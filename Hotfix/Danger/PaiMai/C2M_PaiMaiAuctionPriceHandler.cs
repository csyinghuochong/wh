using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_PaiMaiAuctionPriceHandler : AMActorLocationRpcHandler<Unit, C2M_PaiMaiAuctionPriceRequest, M2C_PaiMaiAuctionPriceResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_PaiMaiAuctionPriceRequest request, M2C_PaiMaiAuctionPriceResponse response, Action reply)
        {
            RoleInfo roleInfo = unit.GetComponent<RoleInfoComponentServer>().RoleInfo;
            if (roleInfo.Gold < request.Price)
            {
                response.Error = ErrorCode.ERR_GoldNotEnoughError;
                reply();
                return;
            }
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            M2P_PaiMaiAuctionPriceRequest message = new M2P_PaiMaiAuctionPriceRequest()
            {
                Price = request.Price,
                UnitID = unit.Id, 
                Occ = roleInfoComponentServer.RoleInfo.Occ,
                AuctionPlayer = roleInfoComponentServer.RoleInfo.Name,
            };
            long paimaiserverid = DBHelper.GetPaiMaiServerId(unit);
            P2M_PaiMaiAuctionPriceResponse r_GameStatusResponse = (P2M_PaiMaiAuctionPriceResponse)await ActorMessageSenderComponent.Instance.Call
                    (paimaiserverid, message);

            response.Error = r_GameStatusResponse.Error;
            reply();
            return;
        }
    }
}
