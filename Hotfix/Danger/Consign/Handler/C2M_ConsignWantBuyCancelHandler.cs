using System;

namespace ET
{
    /// <summary>
    /// 取消求购，退还剩余 Price*ItemNum
    /// </summary>
    [ActorMessageHandler]
    public class C2M_ConsignWantBuyCancelHandler : AMActorLocationRpcHandler<Unit, C2M_ConsignWantBuyCancelRequest, M2C_ConsignWantBuyCancelResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ConsignWantBuyCancelRequest request, M2C_ConsignWantBuyCancelResponse response, Action reply)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.WantBuy, unit.Id))
            {
                if (request.WantBuyId <= 0)
                {
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }

                RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
                long paimaiServerId = DBHelper.GetPaiMaiServerId(unit);
                Consign2M_WantBuyCancelResponse cancelResponse = (Consign2M_WantBuyCancelResponse)await ActorMessageSenderComponent.Instance.Call(
                    paimaiServerId, new M2Consign_WantBuyCancelRequest()
                    {
                        WantBuyId = request.WantBuyId,
                        UserId = roleInfoComponentServer.RoleInfo.UserId,
                    });

                if (cancelResponse.Error != ErrorCode.ERR_Success)
                {
                    response.Error = cancelResponse.Error;
                    reply();
                    return;
                }

                long refund = (long)cancelResponse.Price * cancelResponse.RefundNum;
                if (refund > 0)
                {
                    roleInfoComponentServer.UpdateRoleData(UserDataType.Gold, refund.ToString(), true, ItemGetWay.PaiMaiBuy);
                }

                reply();
                await ETTask.CompletedTask;
            }
        }
    }
}
