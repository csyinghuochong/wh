using System;
using System.Collections.Generic;


namespace ET
{

    [ActorMessageHandler]
    public class C2M_ItemProtectHandler : AMActorLocationRpcHandler<Unit, C2M_ItemProtectRequest, M2C_ItemProtectResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ItemProtectRequest request, M2C_ItemProtectResponse response, Action reply)
        {
            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            // 加锁、解锁只校验角色持有实例，覆盖背包、仓库和穿戴栏；不使用仅校验背包页的 IsValideBagLoc。
            BagInfo bagInfo = bag.GetItemByUId(request.OperateBagID);
            if (bagInfo == null)
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Item_NotExist);
                reply();
                return;
            }
            bagInfo.BindOnOperate();
            bagInfo.IsLocked = request.IsLocked;
            M2C_RoleBagUpdate update = new M2C_RoleBagUpdate();
            update.BagInfoUpdate.Add(bagInfo);
            MessageHelper.SendToClient(unit, update);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
