using System;

namespace ET
{

    [ActorMessageHandler]
    public class P2M_PaiMaiAuctionOverHandler : AMActorRpcHandler<Unit, P2M_PaiMaiAuctionOverRequest, M2P_PaiMaiAuctionOverResponse>
    {
        protected override async ETTask Run(Unit unit, P2M_PaiMaiAuctionOverRequest request, M2P_PaiMaiAuctionOverResponse response, Action reply)
        {
            Log.Warning($"PaiMaiAuctionOver:  {unit.DomainZone()} {unit.Id}");
            
            RoleInfoComponent roleInfoComponent = unit.GetComponent<RoleInfoComponent>();
            if (roleInfoComponent.RoleInfo.Gold < request.Price)
            {
                unit.GetComponent<RoleInfoComponent>().UpdateRoleData(UserDataType.Message, "金币不足，竞拍失败！");
                response.Error = ErrorCode.ERR_GoldNotEnoughError;
            }
            else
            {
                roleInfoComponent.UpdateRoleMoneySub( UserDataType.Gold, (request.Price * -1).ToString(), true, ItemGetWay.Auction );
                response.Error = ErrorCode.ERR_Success;
                Log.Warning($"扣除竞拍价：{unit.DomainZone()} {request.Price}");
            }
            reply();
            await ETTask.CompletedTask;
        }
    }
}
