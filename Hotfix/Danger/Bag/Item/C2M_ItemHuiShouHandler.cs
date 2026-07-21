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
                TaskComponentServer task = unit.GetComponent<TaskComponentServer>();
                ChengJiuComponentServer chengJiu = unit.GetComponent<ChengJiuComponentServer>();

                Dictionary<int, RewardItem> huishouGet = new Dictionary<int, RewardItem>();
                List<long> bagsList = new List<long>();
                List<long> petHexin = new List<long>();

                for (int i = 0; i < huishouList.Count; i++)
                {
                    bool fromPetHexin = false;
                    BagInfo bagInfo = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocBag, huishouList[i]);
                    if (bagInfo == null)
                    {
                        bagInfo = bagComponentServer.GetItemByLoc(ItemLocType.ItemPetHeXinBag, huishouList[i]);
                        fromPetHexin = bagInfo != null;
                    }
                    if (bagInfo == null)
                    {
                        continue;
                    }

                    LDItem ldItem = LDItemCategory.Instance.Get(bagInfo.ItemID);
                    // LD：Sell_ID / Sell_Num 为回收所得；无配置则跳过且不扣该道具
                    if (ldItem.Sell_ID <= 0 || ldItem.Sell_Num <= 0)
                    {
                        continue;
                    }

                    if (fromPetHexin)
                    {
                        petHexin.Add(huishouList[i]);
                    }
                    else
                    {
                        bagsList.Add(huishouList[i]);
                    }

                    int itemId = ldItem.Sell_ID;
                    int itemNum = ldItem.Sell_Num * bagInfo.ItemNum;
                    if (huishouGet.TryGetValue(itemId, out RewardItem rewardItem))
                    {
                        rewardItem.ItemNum += itemNum;
                    }
                    else
                    {
                        huishouGet.Add(itemId, new RewardItem()
                        {
                            ItemType = ItemBigType.Type_Item,
                            ItemID = itemId,
                            ItemNum = itemNum
                        });
                    }
                }

                if (huishouGet.Count == 0)
                {
                    response.Error = ErrorCode.ERR_ItemUseError;
                    reply();
                    return;
                }

                bagComponentServer.OnCostItemData(petHexin, ItemLocType.ItemPetHeXinBag);
                bagComponentServer.OnCostItemData(bagsList, ItemLocType.ItemLocBag);
                List<RewardItem> huishouRewards = new List<RewardItem>(huishouGet.Count);
                foreach (RewardItem rewardItem in huishouGet.Values)
                {
                    huishouRewards.Add(rewardItem);
                }
                bagComponentServer.OnAddItemData(huishouRewards, string.Empty, $"{ItemGetWay.HuiShou}_{TimeHelper.ServerNow()}");
                task.OnItemHuiShow(bagsList.Count + petHexin.Count);
                chengJiu.OnItemHuiShow(bagsList.Count + petHexin.Count);

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
