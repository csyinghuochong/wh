using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_AuctionJoinHandler : AMActorLocationRpcHandler<Unit, C2M_AuctionJoinRequest, M2C_AuctionJoinResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_AuctionJoinRequest request, M2C_AuctionJoinResponse response, Action reply)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Buy, unit.Id))
            {
                long paimaiserverid = DBHelper.GetPaiMaiServerId(unit);
                Consign2M_AuctionJoinResponse r_GameStatusResponse = (Consign2M_AuctionJoinResponse)await ActorMessageSenderComponent.Instance.Call

                        (paimaiserverid, new M2Consign_AuctionJoinRequest()
                        {
                            UnitID = unit.Id,
                            Gold = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Gold
                        });

                if (r_GameStatusResponse.Error == ErrorCode.ERR_Success)
                {
                    unit.GetComponent<RoleInfoComponentServer>().UpdateRoleMoneySub(UserDataType.Gold, (-1 * r_GameStatusResponse.CostGold).ToString(), true, ItemGetWay.AuctionJoin);
                }
                response.Error = r_GameStatusResponse.Error;
                reply();
            }
            await ETTask.CompletedTask;
        }
    }
}
