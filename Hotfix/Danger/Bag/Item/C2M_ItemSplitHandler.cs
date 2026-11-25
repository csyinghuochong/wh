using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_ItemSplitHandler : AMActorLocationRpcHandler<Unit, C2M_ItemSplitRequest, M2C_ItemSplitResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ItemSplitRequest request, M2C_ItemSplitResponse response, Action reply)
        {
            long bagInfoID = request.OperateBagID;
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            BagInfo useBagInfo = bagComponentServer.GetItemByUId(bagInfoID);
            if (useBagInfo == null || !ItemNewHelper.IsValideBagLoc(useBagInfo.Loc)
                || !ItemTradeStatus.CanGrant(useBagInfo.ItemFlags))
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Use_Fail);
                reply();
                return;
            }

            long splitNumber = 0;
            try
            {
                splitNumber = long.Parse(request.OperatePar);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                Log.Error($"C2M_ItemSplitRequest 1");
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Invalid_Data);
                reply();
                return;
            }
            
            if (splitNumber <= 0 )
            {
                Log.Error($"C2M_ItemSplitRequest 2");
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Invalid_Data);
                reply();
                return;
            }

            if (splitNumber >= useBagInfo.ItemNum )
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                reply();
                return;
            }
            if (splitNumber >= 100000)
            {
                Log.Error($"C2M_ItemSplitRequest 4");
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Invalid_Data);
                reply();
                return;
            }
            if (bagComponentServer.GetBagLeftCell(useBagInfo.Loc) <= 0)
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Bag_Full);
                reply();
                return;
            }
            useBagInfo.BindOnOperate();
            useBagInfo.ItemNum -= (int)splitNumber;
            if (useBagInfo.ItemNum <= 0)
            {
                Log.Error($"C2M_ItemSplitRequest 5");
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Invalid_Data);
                reply();
                return;
            }
            bagComponentServer.OnAddItemDataNewCell(useBagInfo, (int)splitNumber);
            LogHelper.LogWarning($"道具拆分 {unit.DomainZone()} {unit.Id} {splitNumber}", true);

            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();
            m2c_bagUpdate.BagInfoUpdate.Add(useBagInfo);
            MessageHelper.SendToClient(unit, m2c_bagUpdate);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
