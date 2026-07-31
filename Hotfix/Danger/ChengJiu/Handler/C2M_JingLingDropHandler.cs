using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_JingLingDropHandler : AMActorLocationRpcHandler<Unit, C2M_JingLingDropRequest, M2C_JingLingDropResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_JingLingDropRequest request, M2C_JingLingDropResponse response, Action reply)
        {
            ChengJiuComponentServer chengJiuComponentServer = unit.GetComponent<ChengJiuComponentServer>();
            int jinglingid = chengJiuComponentServer.JingLingId;
            if (jinglingid == 0 || chengJiuComponentServer.RandomDrop == 1)
            {
                reply();
                return;
            }
            LDElf ldElf = LDElfCategory.Instance.Get(jinglingid);
          
            reply();
            await ETTask.CompletedTask;
        }
    }
}
