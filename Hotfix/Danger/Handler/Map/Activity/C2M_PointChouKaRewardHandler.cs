using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_PointChouKaRewardHandler : AMActorLocationRpcHandler<Unit, C2M_PointChouKaRewardRequest, M2C_PointChouKaRewardResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_PointChouKaRewardRequest request, M2C_PointChouKaRewardResponse response, Action reply)
        {

            BagComponent bagComponent = unit.GetComponent<BagComponent>();
            if (bagComponent.GetBagLeftCell() < 1)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }
            
            reply();
            await ETTask.CompletedTask;
        }
    }
}
