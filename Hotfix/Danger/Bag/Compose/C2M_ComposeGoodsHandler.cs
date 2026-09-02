using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ComposeGoodsHandler : AMActorLocationRpcHandler<Unit, C2M_ComposeGoodsRequest, M2C_ComposeGoodsResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ComposeGoodsRequest request, M2C_ComposeGoodsResponse response, Action reply)
        {
            if (!LDCompose_GoodsCategory.Instance.Contain(request.Compose_Goods))
            {
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            LDCompose_Goods cfg = LDCompose_GoodsCategory.Instance.Get(request.Compose_Goods);
            BagComponentServer bag = unit.GetComponent<BagComponentServer>();

            BagInfo mainBagInfo = null;
            ItemLocType mainLoc = ItemLocType.ItemLocBag;
            bool mainIsCurrency = false;
            if (cfg.Consume_Id_1 > 0 && cfg.Consume_Num_1 > 0)
            {
                mainIsCurrency = ItemNewHelper.GetItemToUserDataType(cfg.Consume_Type_1, cfg.Consume_Id_1) != UserDataType.None;
                if (mainIsCurrency)
                {
                    if (bag.GetItemNumber(cfg.Consume_Type_1, cfg.Consume_Id_1) < cfg.Consume_Num_1)
                    {
                        response.Error = ErrorCode.ERR_ItemNotEnoughError;
                        reply();
                        return;
                    }
                }
                else
                {
                    mainLoc = ItemNewHelper.GetToItemLocType(cfg.Consume_Type_1, cfg.Consume_Id_1);
                    mainBagInfo = bag.GetItemByLoc(mainLoc, request.BagInfoID);
                    if (mainBagInfo == null
                        || mainBagInfo.ItemType != cfg.Consume_Type_1
                        || mainBagInfo.ItemID != cfg.Consume_Id_1
                        || mainBagInfo.ItemNum < cfg.Consume_Num_1)
                    {
                        response.Error = ErrorCode.ERR_ItemNotExist;
                        reply();
                        return;
                    }
                }
            }

            Dictionary<int, List<RewardItem>> locCosts = new Dictionary<int, List<RewardItem>>();
            List<RewardItem> currencyCosts = new List<RewardItem>();

            if (mainIsCurrency)
            {
                AddCurrencyOrLocCost(currencyCosts, locCosts, cfg.Consume_Type_1, cfg.Consume_Id_1, cfg.Consume_Num_1);
            }

            AddCurrencyOrLocCost(currencyCosts, locCosts, cfg.Consume_Type_2, cfg.Consume_Id_2, cfg.Consume_Num_2);
            AddCurrencyOrLocCost(currencyCosts, locCosts, cfg.Consume_Type_3, cfg.Consume_Id_3, cfg.Consume_Num_3);
            AddCurrencyOrLocCost(currencyCosts, locCosts, cfg.Consume_Type_4, cfg.Consume_Id_4, cfg.Consume_Num_4);

            // Consume1/2/4/5：灵玉 / 绑玉 / 金币 / 绑金
            AddCurrencyCost(currencyCosts, 1, cfg.Consume1);
            AddCurrencyCost(currencyCosts, 2, cfg.Consume2);
            AddCurrencyCost(currencyCosts, 4, cfg.Consume4);
            AddCurrencyCost(currencyCosts, 5, cfg.Consume5);

            List<RewardItem> specialCosts = ItemNewHelper.GetRewardItems(cfg.Consume_Special);
            for (int i = 0; i < specialCosts.Count; i++)
            {
                RewardItem special = specialCosts[i];
                AddCurrencyOrLocCost(currencyCosts, locCosts, special.ItemType, special.ItemID, special.ItemNum);
            }

            if (!CheckCosts(bag, currencyCosts, locCosts))
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            List<RewardItem> rewardItems = ItemNewHelper.GetRewardItems(cfg.Goods);
            bool freeMainCell = mainBagInfo != null && mainBagInfo.ItemNum <= cfg.Consume_Num_1;
            if (!HasEnoughSpace(bag, rewardItems, mainLoc, freeMainCell))
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }

            const int getWay = ItemGetWay.SkillMake;
            if (mainBagInfo != null)
            {
                bag.OnCostItemData(mainBagInfo.BagInfoID, cfg.Consume_Num_1, mainLoc);
                ItemAddHelper.OnCostItem(unit, mainBagInfo.ItemType, mainBagInfo.ItemID);
            }

            if (currencyCosts.Count > 0 && !bag.OnCostItemData(currencyCosts, ItemLocType.ItemLocBag, getWay))
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            foreach (KeyValuePair<int, List<RewardItem>> kv in locCosts)
            {
                if (kv.Value.Count > 0 && !bag.OnCostItemData(kv.Value, (ItemLocType)kv.Key, getWay))
                {
                    response.Error = ErrorCode.ERR_ItemNotEnoughError;
                    reply();
                    return;
                }
            }

            if (rewardItems.Count > 0
                && !bag.OnAddItemData(rewardItems, string.Empty, $"{getWay}_{TimeHelper.ServerNow()}"))
            {
                Log.Warning($"C2M_ComposeGoods add fail unit={unit.Id} goods={cfg.Id}");
            }

            reply();
            await ETTask.CompletedTask;
        }

        private static void AddCurrencyCost(List<RewardItem> currencyCosts, int itemId, int num)
        {
            if (num <= 0)
            {
                return;
            }

            currencyCosts.Add(new RewardItem { ItemType = ItemBigType.Type_Item, ItemID = itemId, ItemNum = num });
        }

        private static void AddCurrencyOrLocCost(List<RewardItem> currencyCosts, Dictionary<int, List<RewardItem>> locCosts, int type, int id, int num)
        {
            if (type <= 0 || id <= 0 || num <= 0)
            {
                return;
            }

            RewardItem cost = new RewardItem { ItemType = type, ItemID = id, ItemNum = num };
            if (ItemNewHelper.GetItemToUserDataType(type, id) != UserDataType.None)
            {
                currencyCosts.Add(cost);
                return;
            }

            int loc = (int)ItemNewHelper.GetToItemLocType(type, id);
            if (!locCosts.TryGetValue(loc, out List<RewardItem> list))
            {
                list = new List<RewardItem>();
                locCosts[loc] = list;
            }

            list.Add(cost);
        }

        private static bool CheckCosts(BagComponentServer bag, List<RewardItem> currencyCosts, Dictionary<int, List<RewardItem>> locCosts)
        {
            if (currencyCosts.Count > 0 && !bag.CheckNeedItem(currencyCosts))
            {
                return false;
            }

            foreach (KeyValuePair<int, List<RewardItem>> kv in locCosts)
            {
                List<RewardItem> costs = kv.Value;
                ItemLocType loc = (ItemLocType)kv.Key;
                for (int i = 0; i < costs.Count; i++)
                {
                    RewardItem cost = costs[i];
                    if (bag.GetItemNumber(cost.ItemType, cost.ItemID, loc) < cost.ItemNum)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool HasEnoughSpace(BagComponentServer bag, List<RewardItem> rewards, ItemLocType freedLoc, bool freeOneCell)
        {
            if (rewards == null || rewards.Count == 0)
            {
                return true;
            }

            Dictionary<int, int> needByLoc = new Dictionary<int, int>();
            for (int i = 0; i < rewards.Count; i++)
            {
                RewardItem reward = rewards[i];
                if (ItemNewHelper.GetItemToUserDataType(reward) != UserDataType.None)
                {
                    continue;
                }

                ItemLocType toLoc = ItemNewHelper.GetToItemLocType(reward);
                List<BagInfo> locList = bag.GetItemByLoc(toLoc);
                if (locList == null)
                {
                    return false;
                }

                int pileSum = ItemNewHelper.GetNewItemPileSum(reward);
                int need = ItemNewHelper.CalcNeedNewCells(locList, reward.ItemType, reward.ItemID, reward.ItemNum, pileSum, reward.ItemFlags);
                int locKey = (int)toLoc;
                if (needByLoc.TryGetValue(locKey, out int exist))
                {
                    needByLoc[locKey] = exist + need;
                }
                else
                {
                    needByLoc[locKey] = need;
                }
            }

            foreach (KeyValuePair<int, int> kv in needByLoc)
            {
                int left = bag.GetBagLeftCell(kv.Key);
                if (freeOneCell && kv.Key == (int)freedLoc)
                {
                    left += 1;
                }

                if (left < kv.Value)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
