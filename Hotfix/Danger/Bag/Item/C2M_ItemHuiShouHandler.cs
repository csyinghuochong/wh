using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_ItemHuiShouHandler : AMActorLocationRpcHandler<Unit, C2M_ItemHuiShouRequest, M2C_ItemHuiShouResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_ItemHuiShouRequest request, M2C_ItemHuiShouResponse response, Action reply)
        {
            try
            {
                List<long> huishouList = request.OperateBagID;
                BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();

                //回收所得
                Dictionary<int, RewardItem> huishouGet = new Dictionary<int, RewardItem>();

                List<long> bagsList = new List<long>();
                List<long> petHexin = new List<long>();    
                for (int i = 0; i < huishouList.Count; i++)
                {
                    BagInfo bagInfo = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocBag, huishouList[i]);
                    if (bagInfo != null)
                    {
                        bagsList.Add(huishouList[i]);
                    }
                    else
                    {
                        bagInfo = bagComponentServer.GetItemByLoc(ItemLocType.ItemPetHeXinBag, huishouList[i]);
                        if (bagInfo != null)
                        {
                            petHexin.Add(huishouList[i]);
                        }
                    }

                    if (bagInfo == null)
                    {
                        continue;  
                    }
                    LDItem ldItem = LDItemCategory.Instance.Get(bagInfo.ItemID);
                    string huishouItem = string.Empty;
                    if (huishouItem.Length == 0 || string.IsNullOrEmpty(huishouItem))
                    {
                        continue;
                    }
                    string[] itemList = huishouItem.Split(';');
                    for (int k = 0; k < itemList.Length; k++)
                    {
                        string[] itemInfo = itemList[k].Split(',');
                        int itemId = int.Parse(itemInfo[0]);

                        if (huishouGet.TryGetValue(itemId, out RewardItem rewardItem))
                        {
                            rewardItem.ItemNum += int.Parse(itemInfo[1]) * bagInfo.ItemNum;
                        }
                        else
                        {
                            huishouGet.Add(itemId, new RewardItem() { ItemID = itemId, ItemNum = int.Parse(itemInfo[1]) * bagInfo.ItemNum });
                        }
                    }
                }

                //扣除装备
                bagComponentServer.OnCostItemData(petHexin, ItemLocType.ItemPetHeXinBag);
                bagComponentServer.OnCostItemData(bagsList, ItemLocType.ItemLocBag);
                List<RewardItem> huishouRewards = new List<RewardItem>(huishouGet.Count);
                foreach (RewardItem rewardItem in huishouGet.Values)
                {
                    huishouRewards.Add(rewardItem);
                }
                bagComponentServer.OnAddItemData(huishouRewards, string.Empty, $"{ItemGetWay.HuiShou}_{TimeHelper.ServerNow()}");
                unit.GetComponent<TaskComponentServer>().OnItemHuiShow(bagsList.Count);
                unit.GetComponent<ChengJiuComponentServer>().OnItemHuiShow(bagsList.Count);

                reply();
                await ETTask.CompletedTask;
            }
            catch (Exception ex)
            {
                Log.Debug(ex.ToString());
            }
        }
    }
}
