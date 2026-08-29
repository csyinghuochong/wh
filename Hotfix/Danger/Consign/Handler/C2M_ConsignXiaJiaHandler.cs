using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_ConsignXiaJiaHandler : AMActorLocationRpcHandler<Unit, C2M_ConsignXiaJiaRequest, M2C_ConsignXiaJiaResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ConsignXiaJiaRequest request, M2C_ConsignXiaJiaResponse response, Action reply)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.XiaJia, unit.Id))
            {
                if (unit.GetComponent<BagComponentServer>().GetBagLeftCell() < 1)
                {
                    reply();
                    return;
                }

                long chargeServerId = DBHelper.GetPaiMaiServerId(unit);
                Consign2M_XiaJiaResponse r_GameStatusResponse = (Consign2M_XiaJiaResponse)await ActorMessageSenderComponent.Instance.Call
                    (chargeServerId, new M2Consign_XiaJiaRequest()
                    {
                        BelongId = request.BelongId,    
                        ConsignItemInfoId = request.ConsignItemInfoId
                    });

                if (r_GameStatusResponse.Error == ErrorCode.ERR_Success && r_GameStatusResponse.ConsignItemInfo != null)
                {
                    unit.GetComponent<BagComponentServer>().OnAddItemData(r_GameStatusResponse.ConsignItemInfo.BagInfo, $"{ItemGetWay.XiaJia}_{TimeHelper.ServerNow()}");
                }
                else
                {
                    LogHelper.LogWarning($"C2M_PaiMaiXiaJiaHandler==null  {unit.Id} {request.ConsignItemInfoId}");
                }

                reply();
                await ETTask.CompletedTask;
            }
        }
    }
}
