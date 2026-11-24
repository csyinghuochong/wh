using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_UnionSendMailHandler : AMActorLocationRpcHandler<Unit, C2M_UnionSendMailRequest, M2C_UnionSendMailResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_UnionSendMailRequest request, M2C_UnionSendMailResponse response, Action reply)
        {
            long unionId = unit.GetComponent<NumericComponent>().GetAsLong(NumericType.UnionId_0);
            if (unionId == 0)
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                reply();
                return;
            }

            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Content))
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Invalid_Data);
                reply();
                return;
            }

            U2M_UnionSendMailResponse inner = (U2M_UnionSendMailResponse)await ActorMessageSenderComponent.Instance.Call(
                DBHelper.GetUnionServerId(unit),
                new M2U_UnionSendMailRequest
                {
                    UnionId = unionId,
                    UnitId = unit.Id,
                    Title = request.Title,
                    Content = request.Content,
                });
            response.Error = inner.Error;
            response.Message = inner.Message;
            reply();
        }
    }
}
