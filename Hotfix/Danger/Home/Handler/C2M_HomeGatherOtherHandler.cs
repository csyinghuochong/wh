using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 家园偷取
    /// </summary>
    [ActorMessageHandler]
    public class C2M_HomeGatherOtherHandler : AMActorLocationRpcHandler<Unit, C2M_HomeGatherOtherRequest, M2C_HomeGatherOtherResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_HomeGatherOtherRequest request, M2C_HomeGatherOtherResponse response, Action reply)
        {
            response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Invalid_Data);
            reply();
            await ETTask.CompletedTask;
        }
    }
}