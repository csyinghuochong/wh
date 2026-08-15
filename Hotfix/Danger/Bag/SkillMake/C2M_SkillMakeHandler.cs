using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_SkillMakeHandler : AMActorLocationRpcHandler<Unit, C2M_SkillMakeRequest, M2C_SkillMakeResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_SkillMakeRequest request, M2C_SkillMakeResponse response, Action reply)
        {


            reply();
            await ETTask.CompletedTask;
        }
    }
}

