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
            if (!LDComposeCategory.Instance.Contain(cfg.Compose_Id))
            {
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            BagComponentServer bag = unit.GetComponent<BagComponentServer>();

            BagInfo mainBagInfo = null;
            ItemLocType mainLoc = ItemLocType.ItemLocBag;
            bool mainIsCurrency = false;
            if (cfg.Consume_Id_1 > 0 && cfg.Consume_Num_1 > 0)
            {
                mainIsCurrency = ItemNewHelper.GetItemToUserDataType(cfg.Consume_Type_1, cfg.Consume_Id_1) != UserDataType.None;
                if (mainIsCurrency)
                {
                    if (bag.GetItemNumberForCost(cfg.Consume_Type_1, cfg.Consume_Id_1) < cfg.Consume_Num_1)
                    {
                        response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_NoItem);
                        reply();
                        return;
                    }
                }
                else
                {
                    mainLoc = ItemNewHelper.GetToItemLocType(cfg.Consume_Type_1, cfg.Consume_Id_1);
                    // 主材料只按选中的唯一 BagInfoID 定位，再校验类型、配置 ID 和该格数量。
                    mainBagInfo = bag.GetItemByLoc(mainLoc, request.BagInfoID);
                    if (mainBagInfo == null
                        || mainBagInfo.ItemType != cfg.Consume_Type_1
                        || mainBagInfo.ItemID != cfg.Consume_Id_1
                        || mainBagInfo.ItemNum < cfg.Consume_Num_1)
                    {
                        response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Item_NotExist);
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

            // 副材料按配置 ItemType + ItemID 汇总扣除，不接收副材料 BagInfoID，也不参与产物标签计算。
            AddCurrencyOrLocCost(currencyCosts, locCosts, cfg.Consume_Type_2, cfg.Consume_Id_2, cfg.Consume_Num_2);
            AddCurrencyOrLocCost(currencyCosts, locCosts, cfg.Consume_Type_3, cfg.Consume_Id_3, cfg.Consume_Num_3);
            AddCurrencyOrLocCost(currencyCosts, locCosts, cfg.Consume_Type_4, cfg.Consume_Id_4, cfg.Consume_Num_4);
            AddCurrencyOrLocCost(currencyCosts, locCosts, cfg.Consume_Type_5, cfg.Consume_Id_5, cfg.Consume_Num_5);

            // Consume1/2/4/5：灵玉 / 绑玉 / 金币 / 绑金
            AddCurrencyCost(currencyCosts, 1, cfg.Consume1);
            AddCurrencyCost(currencyCosts, 2, cfg.Consume2);
            AddCurrencyCost(currencyCosts, 4, cfg.Consume4);
            AddCurrencyCost(currencyCosts, 5, cfg.Consume5);

            if (!CheckCosts(bag, currencyCosts, locCosts, mainBagInfo, cfg.Consume_Num_1))
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_NoItem);
                reply();
                return;
            }

            int inheritSourceFlags = mainBagInfo != null ? ComposeHelper.GetComposeItemFlags(mainBagInfo) : 0;
            // 合成表 Goods 为单个产物，字段支持 | 或 ~ 分隔；缺少状态时按 0 解析。
            List<RewardItem> rewardItems = ItemNewHelper.GetRewardItems(cfg.Goods.Replace('|', '~'), inheritSourceFlags);
            if (rewardItems.Count == 0)
            {
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }
            for (int i = 0; i < rewardItems.Count; i++)
            {
                RewardItem reward = rewardItems[i];
                if (reward.ItemNum <= 0 || !ItemNewHelper.CheckValiedItem(reward))
                {
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }
                if (mainBagInfo != null) reward.ItemFlags = inheritSourceFlags;
            }
            long makeTime = TimeHelper.ServerNow();
            string makePlayer = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Name ?? string.Empty;
            if (!HasEnoughSpace(bag, rewardItems, mainBagInfo, cfg.Consume_Num_1, locCosts, makePlayer, makeTime))
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Bag_Full);
                reply();
                return;
            }

            const int getWay = ItemGetWay.SkillMake;
            if (mainBagInfo != null && !bag.OnCostItemData(mainBagInfo.BagInfoID, cfg.Consume_Num_1, mainLoc))
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_NoItem);
                reply();
                return;
            }

            if (mainBagInfo != null)
            {
                ItemAddHelper.OnCostItem(unit, mainBagInfo.ItemType, mainBagInfo.ItemID);
            }

            if (currencyCosts.Count > 0 && !bag.OnCostItemData(currencyCosts, ItemLocType.ItemLocBag, getWay))
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_NoItem);
                reply();
                return;
            }

            foreach (KeyValuePair<int, List<RewardItem>> kv in locCosts)
            {
                if (kv.Value.Count > 0 && !bag.OnCostItemData(kv.Value, (ItemLocType)kv.Key, getWay))
                {
                    response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_NoItem);
                    reply();
                    return;
                }
            }

            // 在实际入包处收集实例，保留生成的装备属性和唯一ID，避免按配置重建产物。
            if (!bag.OnAddItemData(rewardItems, new AddItemArg($"{getWay}_{makeTime}")
            {
                MakePlayer = makePlayer,
                MakeTime = makeTime,
                ResultItems = response.BagInfos,
            }))
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Bag_Full);
                reply();
                return;
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

        private static bool CheckCosts(BagComponentServer bag, List<RewardItem> currencyCosts,
                Dictionary<int, List<RewardItem>> locCosts, BagInfo mainItem, int mainCount)
        {
            // 主副材料、重复材料及货币必须合计校验，避免主材料已扣后才发现副材料不足。
            Dictionary<long, long> costs = new Dictionary<long, long>();
            // 顺序与 OnCostItemData 一致；绑玉、绑金不足时沿用 Same_Id 补扣规则。
            for (int i = currencyCosts.Count - 1; i >= 0; i--)
            {
                RewardItem cost = currencyCosts[i];
                int remaining = ReserveCurrency(cost.ItemType, cost.ItemID, cost.ItemNum);
                int fallbackId = ItemNewHelper.GetCostFallbackItemId(cost.ItemType, cost.ItemID);
                if (remaining > 0 && fallbackId > 0) remaining = ReserveCurrency(cost.ItemType, fallbackId, remaining);
                if (remaining > 0) return false;
            }
            foreach (KeyValuePair<int, List<RewardItem>> kv in locCosts)
            {
                foreach (RewardItem cost in kv.Value) AddCost(cost.ItemType, cost.ItemID, cost.ItemNum);
            }
            if (mainItem != null) AddCost(mainItem.ItemType, mainItem.ItemID, mainCount);
            foreach (KeyValuePair<long, long> cost in costs)
            {
                if (bag.GetItemNumber((int)(cost.Key >> 32), (int)cost.Key) < cost.Value) return false;
            }

            return true;

            void AddCost(int type, int id, int count)
            {
                long key = ((long)type << 32) | (uint)id;
                costs.TryGetValue(key, out long total);
                costs[key] = total + count;
            }

            int ReserveCurrency(int type, int id, int count)
            {
                long key = ((long)type << 32) | (uint)id;
                costs.TryGetValue(key, out long used);
                int take = (int)Math.Min(Math.Max(0, bag.GetItemNumber(type, id) - used), count);
                costs[key] = used + take;
                return count - take;
            }
        }

        private static bool HasEnoughSpace(BagComponentServer bag, List<RewardItem> rewards, BagInfo mainItem, int mainCount,
                Dictionary<int, List<RewardItem>> locCosts, string makePlayer, long makeTime)
        {
            Dictionary<int, List<BagInfo>> itemsByLoc = new Dictionary<int, List<BagInfo>>();
            for (int i = 0; i < rewards.Count; i++)
            {
                RewardItem reward = rewards[i];
                if (ItemNewHelper.GetItemToUserDataType(reward) != UserDataType.None)
                {
                    continue;
                }

                ItemLocType toLoc = ItemNewHelper.GetToItemLocType(reward);
                if (!ItemNewHelper.IsValideBagLoc((int)toLoc))
                {
                    return false;
                }
                int locKey = (int)toLoc;
                if (!itemsByLoc.TryGetValue(locKey, out List<BagInfo> locList))
                {
                    locList = new List<BagInfo>();
                    foreach (BagInfo item in bag.GetItemByLoc(toLoc))
                    {
                        BagInfo copy = new BagInfo
                        {
                            BagInfoID = item.BagInfoID, ItemType = item.ItemType, ItemID = item.ItemID,
                            ItemNum = item.ItemNum, ItemFlags = item.ItemFlags,
                            MakePlayer = item.MakePlayer, MakeTime = item.MakeTime,
                        };
                        if (mainItem != null && copy.BagInfoID == mainItem.BagInfoID)
                        {
                            copy.ItemNum -= mainCount;
                            copy.BindOnOperate();
                        }
                        if (copy.ItemNum > 0) locList.Add(copy);
                    }
                    if (locCosts.TryGetValue(locKey, out List<RewardItem> costs))
                    {
                        for (int c = costs.Count - 1; c >= 0; c--)
                        {
                            RewardItem cost = costs[c];
                            int remaining = cost.ItemNum;
                            for (int k = locList.Count - 1; k >= 0 && remaining > 0; k--)
                            {
                                BagInfo item = locList[k];
                                if (item.ItemType != cost.ItemType || item.ItemID != cost.ItemID) continue;
                                int take = Math.Min(item.ItemNum, remaining);
                                item.ItemNum -= take;
                                remaining -= take;
                                item.BindOnOperate();
                                if (item.ItemNum == 0) locList.RemoveAt(k);
                            }
                        }
                    }
                    itemsByLoc.Add(locKey, locList);
                }

                int pileSum = ItemNewHelper.GetNewItemPileSum(reward);
                int need = ItemNewHelper.CalcNeedNewCells(locList, reward.ItemType, reward.ItemID, reward.ItemNum,
                        pileSum, reward.ItemFlags, makePlayer, makeTime);
                if (need > bag.GeBagTotalCell(locKey) - locList.Count) return false;
                int left = reward.ItemNum;
                foreach (BagInfo item in locList)
                {
                    if (item.ItemType != reward.ItemType || item.ItemID != reward.ItemID
                            || !ItemFlagEnum.CanPileTogether(item.ItemFlags, reward.ItemFlags)
                            || !ItemNewHelper.SameMakeInfo(item.MakePlayer, item.MakeTime, makePlayer, makeTime)) continue;
                    int add = Math.Min(Math.Max(0, pileSum - item.ItemNum), left);
                    item.ItemNum += add;
                    left -= add;
                }
                while (left > 0)
                {
                    int add = Math.Min(Math.Max(1, pileSum), left);
                    locList.Add(new BagInfo
                    {
                        ItemType = reward.ItemType, ItemID = reward.ItemID, ItemNum = add,
                        ItemFlags = reward.ItemFlags, MakePlayer = makePlayer, MakeTime = makeTime,
                    });
                    left -= add;
                }
            }

            return true;
        }
    }
}
