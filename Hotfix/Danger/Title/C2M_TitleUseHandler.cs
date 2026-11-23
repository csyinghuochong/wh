using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_TitleUseHandler : AMActorLocationRpcHandler<Unit, C2M_TitleUseRequest, M2C_TitleUseResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_TitleUseRequest request, M2C_TitleUseResponse response, Action reply)
        {
            TitleComponentServer titleComponent = unit.GetComponent<TitleComponentServer>();
            if (request.TitleId   >  0  && !titleComponent.IsHaveTitle(request.TitleId))
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                reply();
                return;
            }

            titleComponent.TitleId = request.TitleId;
            UnitHelper.SyncUnitAppear(unit);
            ChatShowHelper.Sync(unit);

            reply();
            await ETTask.CompletedTask;
        }
    }
}
