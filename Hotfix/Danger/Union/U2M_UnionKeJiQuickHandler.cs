using System;

namespace ET
{
    [ActorMessageHandler]
    public class U2M_UnionKeJiQuickHandler : AMActorRpcHandler<Unit, U2M_UnionKeJiQuickRequest, M2U_UnionKeJiQuickResponse>
    {
        protected override async ETTask Run(Unit unit, U2M_UnionKeJiQuickRequest request, M2U_UnionKeJiQuickResponse response, Action reply)
        {
            RoleInfoComponentServer roleInfo = unit.GetComponent<RoleInfoComponentServer>();
            if (unit.GetComponent<BagComponentServer>().GetItemNumber(ItemBigType.Type_Item, UserDataType.Diamond) <= request.Cost)
            {
                int code = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_NoItem);
                response.Error = code;
                reply();
                return;
            }

            roleInfo.UpdateRoleData(UserDataType.Diamond, $"-{request.Cost}", true, ItemGetWay.UnionXiuLian);

            reply();
            await ETTask.CompletedTask;
        }
    }
}
