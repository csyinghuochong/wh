using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_MakeLearnHandler : AMActorLocationRpcHandler<Unit, C2M_MakeLearnRequest, M2C_MakeLearnResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_MakeLearnRequest request, M2C_MakeLearnResponse response, Action reply)
        {
            reply();
            await ETTask.CompletedTask;
        }
    }
}
