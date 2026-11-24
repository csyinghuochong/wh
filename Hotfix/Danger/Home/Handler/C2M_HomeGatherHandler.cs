using System;


namespace ET
{

    /// <summary>
    /// 家园收获
    /// </summary>
    [ActorMessageHandler]
    public class C2M_HomeGatherHandler : AMActorLocationRpcHandler<Unit, C2M_HomeGatherRequest, M2C_HomeGatherResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_HomeGatherRequest request, M2C_HomeGatherResponse response, Action reply)
        {
            

            response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Invalid_Data);
            reply();
            await ETTask.CompletedTask;
         }
    }
}
