using System;

namespace ET
{

    [ActorMessageHandler]
    public class U2M_UnionApplyHandler : AMActorRpcHandler<Unit, U2M_UnionApplyRequest, M2U_UnionApplyResponse>
    {
        protected override async ETTask Run(Unit unit, U2M_UnionApplyRequest request, M2U_UnionApplyResponse response, Action reply)
        {
            NumericComponent numeric = unit.GetComponent<NumericComponent>();
            // 创建公会和入会请求由 Unit 邮箱串行处理，以在线角色的实时状态作最终判断。
            if (numeric.GetAsLong(NumericType.UnionId_0) != 0)
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Union_Already_Joined);
                reply();
                return;
            }

            RoleInfoComponentServer roleInfo = unit.GetComponent<RoleInfoComponentServer>();
            numeric.ApplyValue(NumericType.UnionId_0, request.UnionId);
            numeric.ApplyValue(NumericType.UnionLeader, 0);
            roleInfo.SetUnionName(request.UnionName);
            await unit.GetComponent<DBSaveComponent>().UpdateCacheDBAsync();
            reply();
        }
    }
}
