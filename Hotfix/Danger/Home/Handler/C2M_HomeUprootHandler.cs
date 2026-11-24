using System;


namespace ET
{
    [ActorMessageHandler]
    public class C2M_HomeUprootHandler : AMActorLocationRpcHandler<Unit, C2M_HomeUprootRequest, M2C_HomeUprootResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_HomeUprootRequest request, M2C_HomeUprootResponse response, Action reply)
        {
            response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Invalid_Data);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
