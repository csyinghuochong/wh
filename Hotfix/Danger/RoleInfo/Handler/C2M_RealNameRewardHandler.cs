using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_RealNameRewardHandler : AMActorLocationRpcHandler<Unit, C2M_RealNameRewardRequest, M2C_RealNameRewardResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_RealNameRewardRequest request, M2C_RealNameRewardResponse response, Action reply)
        {
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();

            string[] itemCost = LDGlobalValueCategory.Instance.Get(6).Value.Split('@');
            List<RewardItem> rewardItems = new List<RewardItem>(itemCost.Length);
            for (int i = 0; i < itemCost.Length; i++)
            {
                string[] itemInfo = itemCost[i].Split(';');
                int itemId = int.Parse(itemInfo[0]);
                int itemNum = int.Parse(itemInfo[1]);
                rewardItems.Add(new RewardItem() { ItemID = itemId, ItemNum = itemNum });
            }

            bool sucess = bagComponentServer.OnAddItemData(rewardItems, string.Empty, string.Empty);
            response.Error = sucess ? ErrorCode.ERR_Success : ErrorCode.ERR_BagIsFull;

            reply();
            await ETTask.CompletedTask;
        }
    }
}
