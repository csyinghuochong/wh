using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_KillMonsterRewardHandler: AMActorLocationRpcHandler<Unit, C2M_KillMonsterRewardRequest, M2C_KillMonsterRewardResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_KillMonsterRewardRequest request, M2C_KillMonsterRewardResponse response, Action reply)
        {
            reply();
            await ETTask.CompletedTask;
        }
    }
}