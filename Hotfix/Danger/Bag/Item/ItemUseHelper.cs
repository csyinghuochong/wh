using System;

namespace ET
{
    public static class ItemUseHelper
    {
        /// <param name="useBagInfo">背包使用传道具，获得后自动使用传空（不扣背包、不推 bagUpdate）。</param>
        public static int UseItem(Unit unit, int itemId, BagInfo useBagInfo, M2C_RoleBagUpdate bagUpdate, out string responsePar)
        {
            responsePar = null;
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            RoleDailyDataComponentServer daily = unit.GetComponent<RoleDailyDataComponentServer>();
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            if (useBagInfo != null)
            {
                itemId = useBagInfo.ItemID;
            }

            LDItem ldItem = LDItemCategory.Instance.Get(itemId);
            if (ldItem == null)
            {
                return ErrorCode.ERR_Success;
            }

            if (ldItem.DayUseNum > 0 && (daily?.GetDayItemUse(ldItem.Id) ?? 0) >= ldItem.DayUseNum)
            {
                return ErrorCode.ERR_ItemNoUseTime;
            }

            if (ldItem.SumUseNum > 0 && roleInfoComponentServer.GetTotalUseTimes(ldItem.Id) >= ldItem.SumUseNum)
            {
                return ErrorCode.ERR_ItemNoUseTime;
            }

            int recipeMakeId = 0;
            if (ldItem.ItemType == ItemSubTypeEnum.SubType_Recipe_98)
            {
                recipeMakeId = ldItem.ItemTypeParam1;
                if (recipeMakeId <= 0 || !LDSkill_MakeCategory.Instance.Contain(recipeMakeId))
                {
                    return ErrorCode.ERR_ModifyData;
                }
            }

            if (useBagInfo != null && !bagComponentServer.OnCostItemData(useBagInfo, ItemLocType.ItemLocBag, 1))
            {
                return ErrorCode.ERR_Success;
            }

            if (useBagInfo != null && bagUpdate != null)
            {
                if (useBagInfo.ItemNum <= 0)
                {
                    bagUpdate.BagInfoDelete.Add(useBagInfo);
                }
                else
                {
                    bagUpdate.BagInfoUpdate.Add(useBagInfo);
                }
            }

            if (ldItem.DayUseNum > 0)
            {
                daily?.OnDayItemUse(ldItem.Id);
            }

            if (ldItem.SumUseNum > 0)
            {
                roleInfoComponentServer.OnTotalUseTimes(ldItem.Id);
            }

            if (recipeMakeId > 0)
            {
                roleInfoComponentServer.LearnRecipe(recipeMakeId);
            }

            return ErrorCode.ERR_Success;
        }
    }
}
