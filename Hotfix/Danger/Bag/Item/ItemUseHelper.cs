using System;

namespace ET
{
    public static class ItemUseHelper
    {
        /// <param name="useBagInfo">背包使用传道具，获得后自动使用传空（不扣背包、不推 bagUpdate）。</param>
        /// <param name="useNum">使用数量。有格子时不能小于 1，也不能大于该格子 ItemNum。</param>
        /// <param name="locType">道具所在 ItemLocType，与客户端传入一致。</param>
        public static int UseItem(Unit unit, int itemId, BagInfo useBagInfo, M2C_RoleBagUpdate bagUpdate, int useNum, ItemLocType locType)
        {
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            RoleDailyDataComponentServer daily = unit.GetComponent<RoleDailyDataComponentServer>();
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            if (useBagInfo != null)
            {
                itemId = useBagInfo.ItemID;
                if (useNum < 1 || useNum > useBagInfo.ItemNum)
                {
                    return ErrorCode.ERR_ItemNotEnoughError;
                }
            }

            LDItem ldItem = LDItemCategory.Instance.Get(itemId);
            if (ldItem == null)
            {
                return ErrorCode.ERR_Success;
            }

            if (ldItem.SumUseNum > 0 && roleInfoComponentServer.GetTotalUseTimes(ldItem.Id) >= ldItem.SumUseNum)
            {
                return ErrorCode.ERR_ItemNoUseTime;
            }

            int recipeMakeId = 0;
            if (ldItem.ItemType == ItemTypeEnum.SubType_Recipe_98)
            {
                recipeMakeId = ldItem.GetTypeParam1();
                if (recipeMakeId <= 0 || !LDSkill_MakeCategory.Instance.Contain(recipeMakeId))
                {
                    return ErrorCode.ERR_ModifyData;
                }
            }

            if (useBagInfo != null && !bagComponentServer.OnCostItemData(useBagInfo, locType, useNum))
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
