using System;

namespace ET
{

    /// <summary>按 ItemType.Is_Sell 校验出售资格，使用 Sell_ID / Sell_Num 计算售价；校验货币上限后扣道具并同步变化。</summary>
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
                || item.Sell_ID <= 0 || item.Sell_Num <= 0
                || ItemNewHelper.GetItemToUserDataType(ItemBigType.Type_Item, item.Sell_ID) == UserDataType.None)
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Invalid_Data);
                reply();
                return;
            }

            RoleInfoComponentServer role = unit.GetComponent<RoleInfoComponentServer>();
            long sellAmount = (long)item.Sell_Num * sellNum;
            long current = RoleCurrencyHelper.Get(role.RoleInfo, item.Sell_ID);
            long maximum = long.MaxValue;
            if (RoleCurrencyHelper.IsDailyCurrency(item.Sell_ID))
            {
                RoleDailyDataComponentServer daily = unit.GetComponent<RoleDailyDataComponentServer>();
                current = item.Sell_ID == UserDataType.DailyActive ? daily.GetDailyActivePoint() : daily.GetWeeklyActivePoint();
                maximum = int.MaxValue;
            }
            if (current > maximum - sellAmount)
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Exceeds_Limit);
                reply();
                return;
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
            role.UpdateRoleData(item.Sell_ID, sellAmount.ToString(), true, ItemGetWay.Sell);
            reply();
            await ETTask.CompletedTask;
        }

    }
}
