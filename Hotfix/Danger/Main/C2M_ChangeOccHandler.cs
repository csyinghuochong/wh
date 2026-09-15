using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_ChangeOccHandler : AMActorLocationRpcHandler<Unit, C2M_ChangeOccRequest, M2C_ChangeOccResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ChangeOccRequest request, M2C_ChangeOccResponse response, Action reply)
        {
            response.Error = ErrorCode.ERR_ModifyData;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
