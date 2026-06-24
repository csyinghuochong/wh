using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_PetHeXinHeChengQuickHandler : AMActorLocationRpcHandler<Unit, C2M_PetHeXinHeChengQuickRequest, M2C_PetHeXinHeChengQuickResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_PetHeXinHeChengQuickRequest request, M2C_PetHeXinHeChengQuickResponse response, Action reply)
        {
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            List<BagInfo> allPetHeXin = bagComponentServer.BagItemPetHeXin;

            List<long> costList = new List<long>();
            List<RewardItem> rewardItems = new List<RewardItem>();  
            Dictionary<int, List<BagInfo>> keyValuePairs = new Dictionary<int, List<BagInfo>>();
            for (int i = 0; i < allPetHeXin.Count; i++)
            {
                BagInfo bagInfo = allPetHeXin[i];   
                if (!keyValuePairs.ContainsKey(bagInfo.ItemID))
                {
                    keyValuePairs.Add(bagInfo.ItemID, new List<BagInfo>());
                }
                keyValuePairs[bagInfo.ItemID].Add(bagInfo);
            }


            //去掉多余的
            foreach (var item in keyValuePairs)
            {
                LDItem ldItem = LDItemCategory.Instance.Get(item.Key);
                /*if (Item.PetHeXinHeChengID == 0)
                {
                    item.Value.Clear();
                    continue;
                }*/
                if (keyValuePairs.Count < 2)
                {
                    item.Value.Clear();
                }
                if (item.Value.Count % 2 > 0)
                {
                    item.Value.RemoveAt(item.Value.Count - 1);
                }
            }

            foreach (var item in keyValuePairs)
            {
                LDItem ldItem = LDItemCategory.Instance.Get(item.Key);
    
                int number1 = item.Value.Count / 2;
                //新增item
                for (int n = 0; n < number1; n++)
                {
                    //rewardItems.Add( new RewardItem() { ItemID = Item.PetHeXinHeChengID, ItemNum = 1 } );
                }

                //移除item
                for (int n = 0; n < item.Value.Count; n++)
                {
                    costList.Add(item.Value[n].BagInfoID);
                }
            }

            bagComponentServer.OnCostItemData(costList, ItemLocType.ItemPetHeXinBag);
            bagComponentServer.OnAddItemData(rewardItems, string.Empty, $"{ItemGetWay.PetHeXinHeCheng}_{TimeHelper.ServerNow()}");
            reply();
            await ETTask.CompletedTask;
        }
    }
}
