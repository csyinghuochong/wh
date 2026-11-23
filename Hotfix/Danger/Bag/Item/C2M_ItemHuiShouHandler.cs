using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_ItemHuiShouHandler : AMActorLocationRpcHandler<Unit, C2M_ItemHuiShouRequest, M2C_ItemHuiShouResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_ItemHuiShouRequest request, M2C_ItemHuiShouResponse response, Action reply)
        {
            List<long> huishouList = request.OperateBagID;
            if (huishouList.Count == 0)
            {
                response.Error =  LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Use_Fail);
                reply();
                return;
            }

            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            Dictionary<int, List<long>> idsByLoc = new Dictionary<int, List<long>>();
            for (int i = 0; i < huishouList.Count; i++)
            {
                BagInfo bagInfo = bag.GetItemByUId(huishouList[i]);
                if (bagInfo == null)
                {
                    continue;
                }

                // 请求包含已穿戴装备时整单直接返回，不扣除任何道具，也不设置错误码。
                if (bagInfo.Loc == (int)ItemLocType.ItemLocEquip)
                {
                    reply();
                    return;
                }

                if (!idsByLoc.TryGetValue(bagInfo.Loc, out List<long> ids))
                {
                    ids = new List<long>();
                    idsByLoc[bagInfo.Loc] = ids;
                }

                ids.Add(bagInfo.BagInfoID);
            }

            if (idsByLoc.Count == 0)
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Item_NotExist);
                reply();
                return;
            }

            foreach (KeyValuePair<int, List<long>> kv in idsByLoc)
            {
                bag.OnCostItemData(kv.Value, (ItemLocType)kv.Key);
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
