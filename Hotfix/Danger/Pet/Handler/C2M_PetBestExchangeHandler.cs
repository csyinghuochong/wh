using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 神兽兑换。消耗读 GlobalValue.Global_Pet_Best_Exchange。
    /// </summary>
    [ActorMessageHandler]
    public class C2M_PetBestExchangeHandler : AMActorLocationRpcHandler<Unit, C2M_PetBestExchangeRequest, M2C_PetBestExchangeResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_PetBestExchangeRequest request, M2C_PetBestExchangeResponse response, Action reply)
        {
            List<int> shenshouIds = PetHelper.GetAllShenShou();
            if (shenshouIds.Count == 0)
            {
                Console.WriteLine("shenshouIds.Count == 0");
            }
           
            List<RewardItem> costs = ItemNewHelper.GetRewardItems(
                    LDGlobalValueCategory.Instance.GetByKey(GlobalValueKey.Global_Pet_Best_Exchange).Value);
            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            Dictionary<int, List<RewardItem>> locCosts = GroupCostByLoc(costs);
            if (!CanCostByLoc(bag, locCosts))
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            foreach (KeyValuePair<int, List<RewardItem>> kv in locCosts)
            {
                if (!bag.OnCostItemData(kv.Value, (ItemLocType)kv.Key, ItemGetWay.CostItem))
                {
                    response.Error = ErrorCode.ERR_ItemNotEnoughError;
                    reply();
                    return;
                }
            }

            int petConfigId = shenshouIds[RandomHelper.RandomNumber(0, shenshouIds.Count)];
            response.PetInfo = unit.GetComponent<PetComponentServer>().OnAddPet(ItemGetWay.DuiHuan, petConfigId);
            reply();
            await ETTask.CompletedTask;
        }

        static Dictionary<int, List<RewardItem>> GroupCostByLoc(List<RewardItem> costs)
        {
            Dictionary<int, List<RewardItem>> locCosts = new Dictionary<int, List<RewardItem>>();
            for (int i = 0; i < costs.Count; i++)
            {
                RewardItem cost = costs[i];
                int itemType = cost.ItemType > 0 ? cost.ItemType : ItemBigType.Type_Item;
                int loc = (int)ItemNewHelper.GetToItemLocType(itemType, cost.ItemID);
                if (!locCosts.TryGetValue(loc, out List<RewardItem> list))
                {
                    list = new List<RewardItem>();
                    locCosts.Add(loc, list);
                }

                list.Add(cost);
            }

            return locCosts;
        }

        static bool CanCostByLoc(BagComponentServer bag, Dictionary<int, List<RewardItem>> locCosts)
        {
            foreach (KeyValuePair<int, List<RewardItem>> kv in locCosts)
            {
                ItemLocType loc = (ItemLocType)kv.Key;
                for (int i = 0; i < kv.Value.Count; i++)
                {
                    RewardItem cost = kv.Value[i];
                    int itemType = cost.ItemType > 0 ? cost.ItemType : ItemBigType.Type_Item;
                    long owned = bag.GetItemNumber(itemType, cost.ItemID, loc);
                    int fallbackId = ItemNewHelper.GetCostFallbackItemId(itemType, cost.ItemID);
                    if (fallbackId > 0)
                    {
                        owned += bag.GetItemNumber(itemType, fallbackId, loc);
                    }

                    if (owned < cost.ItemNum)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
