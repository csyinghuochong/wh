using System;
using System.Collections.Generic;

namespace ET
{

    /// <summary>按 ItemType.Is_Sell 校验出售资格；先校验整笔货币奖励，再扣除道具并推送背包及货币变化。</summary>
    [ActorMessageHandler]
    public class C2M_ItemSellHandler: AMActorLocationRpcHandler<Unit, C2M_ItemSellRequest, M2C_ItemSellResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ItemSellRequest request, M2C_ItemSellResponse response, Action reply)
        {
            long bagInfoID = request.OperateBagID;
            ItemLocType locType = (ItemLocType)request.LocType;
            if (!ItemNewHelper.IsValideBagLoc((int)locType))
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Invalid_Data);
                reply();
                return;
            }

            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            BagInfo useBagInfo = bag.GetItemByLoc(locType, bagInfoID);
            if (useBagInfo == null )
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Item_NotExist);
                reply();
                return;
            }

            int sellNum = request.SellNum;
            if (sellNum <= 0 || sellNum > useBagInfo.ItemNum || useBagInfo.ItemType != ItemBigType.Type_Item
                || useBagInfo.IsLocked || !ItemTradeStatus.CanGrant(useBagInfo.ItemFlags)
                || !LDItemCategory.Instance.Contain(useBagInfo.ItemID))
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Invalid_Data);
                reply();
                return;
            }

            LDItem item = LDItemCategory.Instance.Get(useBagInfo.ItemID);
            if (!LDItemTypeCategory.Instance.Contain(item.ItemType)
                || LDItemTypeCategory.Instance.Get(item.ItemType).Is_Sell != 1
                || !TryGetSellCurrencies(item.Reward, sellNum, out Dictionary<int, long> currencies))
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Invalid_Data);
                reply();
                return;
            }

            RoleInfoComponentServer role = unit.GetComponent<RoleInfoComponentServer>();
            foreach (KeyValuePair<int, long> currency in currencies)
            {
                long current = RoleCurrencyHelper.Get(role.RoleInfo, currency.Key);
                long maximum = long.MaxValue;
                if (RoleCurrencyHelper.IsDailyCurrency(currency.Key))
                {
                    RoleDailyDataComponentServer daily = unit.GetComponent<RoleDailyDataComponentServer>();
                    current = currency.Key == UserDataType.DailyActive ? daily.GetDailyActivePoint() : daily.GetWeeklyActivePoint();
                    maximum = int.MaxValue;
                }
                if (currency.Value > maximum - current)
                {
                    response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Exceeds_Limit);
                    reply();
                    return;
                }
            }

            if (!bag.OnCostItemData(useBagInfo, locType, sellNum))
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Item_NotExist);
                reply();
                return;
            }

            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();
            if (useBagInfo.ItemNum <= 0)
            {
                m2c_bagUpdate.BagInfoDelete.Add(useBagInfo);
            }
            else
            {
                m2c_bagUpdate.BagInfoUpdate.Add(useBagInfo);
            }

            MessageHelper.SendToClient(unit, m2c_bagUpdate);
            foreach (KeyValuePair<int, long> currency in currencies)
            {
                role.UpdateRoleData(currency.Key, currency.Value.ToString(), true, ItemGetWay.Sell);
            }
            reply();
            await ETTask.CompletedTask;
        }

        /// <summary>Reward 每项为类型~ID~单件数量~状态（也兼容 ^）；缺少状态按 0 解析，货币不入包，不使用物品交易状态。</summary>
        private static bool TryGetSellCurrencies(string reward, int sellNum, out Dictionary<int, long> currencies)
        {
            currencies = new Dictionary<int, long>();
            if (string.IsNullOrWhiteSpace(reward))
            {
                return false;
            }

            foreach (string entry in reward.Split('|'))
            {
                string[] fields = entry.Replace('^', '~').Split('~');
                if ((fields.Length != 3 && fields.Length != 4)
                    || !int.TryParse(fields[0], out int type) || type != ItemBigType.Type_Item
                    || !int.TryParse(fields[1], out int id) || id <= 0
                    || !int.TryParse(fields[2], out int count) || count <= 0
                    || ItemNewHelper.GetItemToUserDataType(type, id) == UserDataType.None)
                {
                    return false;
                }
                int status = 0;
                if (fields.Length == 4 && !string.IsNullOrEmpty(fields[3]) && !int.TryParse(fields[3], out status))
                {
                    return false;
                }

                long amount = (long)count * sellNum;
                currencies.TryGetValue(id, out long total);
                if (amount > long.MaxValue - total)
                {
                    return false;
                }
                currencies[id] = total + amount;
            }
            return true;
        }
    }
}
