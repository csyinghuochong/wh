using System;
using System.Collections.Generic;

namespace ET
{
    public static class ItemUseHelper
    {
        /// <param name="useBagInfo">背包使用传道具，获得后自动使用传空（不扣背包、不推 bagUpdate）。</param>
        /// <param name="useNum">使用数量。有格子时不能小于 1，也不能大于该格子 ItemNum。</param>
        /// <param name="locType">道具所在 ItemLocType，与客户端传入一致。</param>
        public static int UseItem(Unit unit, int itemId, BagInfo useBagInfo, M2C_RoleBagUpdate bagUpdate, int useNum, ItemLocType locType, string operatePar = "")
        {
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            RoleDailyDataComponentServer daily = unit.GetComponent<RoleDailyDataComponentServer>();
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            if (useBagInfo != null)
            {
                itemId = useBagInfo.ItemID;
                if (useNum < 1 || useNum > useBagInfo.ItemNum)
                {
                    return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_NoItem);
                }
            }

            if (useNum < 1 || itemId <= 0 || !LDItemCategory.Instance.Contain(itemId)) return ErrorCode.ERR_Parameter;


            LDItem ldItem = LDItemCategory.Instance.Get(itemId);
            int useError = daily.CheckDayItemUse(ldItem, useNum);
            if (useError != ErrorCode.ERR_Success)
            {
                return useError;
            }

            useError = roleInfoComponentServer.CheckTotalItemUse(ldItem, useNum);
            if (useError != ErrorCode.ERR_Success)
            {
                return useError;
            }

            List<RewardItem> packageRewards = null;
            if (ldItem.ItemType >= ItemTypeEnum.SubType_Package_All && ldItem.ItemType <= ItemTypeEnum.SubType_Package_Random)
            {
                int packageError = GetPackageRewards(ldItem, useNum, operatePar, out packageRewards);
                if (packageError != ErrorCode.ERR_Success) return packageError;
                BagInfo freedItem = useBagInfo != null && useNum == useBagInfo.ItemNum ? useBagInfo : null;
                BagInfo remainingItem = null;
                if (useBagInfo != null && freedItem == null)
                {
                    remainingItem = new BagInfo { BagInfoID = useBagInfo.BagInfoID, ItemType = useBagInfo.ItemType,
                        ItemID = useBagInfo.ItemID, ItemNum = useBagInfo.ItemNum - useNum, ItemFlags = useBagInfo.ItemFlags,
                        Loc = useBagInfo.Loc, MakePlayer = useBagInfo.MakePlayer, MakeTime = useBagInfo.MakeTime };
                    remainingItem.BindOnOperate();
                }
                if (!bagComponentServer.CanAddItemData(packageRewards, freedItem, remainingItem)) return ErrorCode.ERR_BagIsFull;
            }

            int recipeMakeId = 0;
            if (ldItem.ItemType == ItemTypeEnum.SubType_Recipe_95)
            {
                recipeMakeId = ldItem.GetTypeParam1();
                if (recipeMakeId <= 0 || !LDSkill_MakeCategory.Instance.Contain(recipeMakeId))
                {
                    return ErrorCode.ERR_ModifyData;
                }
            }

            if (useBagInfo != null && !bagComponentServer.OnCostItemData(useBagInfo, locType, useNum))
            {
                return ErrorCode.ERR_ItemNotExist;
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


            if (packageRewards != null)
            {
                bagComponentServer.OnAddItemData(packageRewards, $"{ItemGetWay.ItemBox_6}_{TimeHelper.ServerNow()}");
            }

            daily.AddDayItemUse(ldItem.Id, useNum);
            roleInfoComponentServer.AddTotalItemUse(ldItem.Id, useNum);

            if (recipeMakeId > 0)
            {
                roleInfoComponentServer.LearnRecipe(recipeMakeId);
            }

            return ErrorCode.ERR_Success;
        }
        /// <summary>选择序号从 1 开始；随机包裹每份单独抽取，再按类型/ID/状态合并。</summary>
        public static int GetPackageRewards(LDItem item, int useNum, string operatePar, out List<RewardItem> rewards)
        {
            rewards = new List<RewardItem>();
            if (useNum < 1 || useNum > 1000) return ErrorCode.ERR_Parameter;
            if (item.ItemType == ItemTypeEnum.SubType_Package_Random)
            {
                if (item.Drop_Id <= 0 || !LDDropCategory.Instance.Contain(item.Drop_Id)) return ErrorCode.ERR_Parameter;
                for (int i = 0; i < useNum; i++) DropHelper.DropIDToDropItem(item.Drop_Id, rewards);
                // 概率 Drop 允许本次抽空，不能补发展示奖励。
            }
            else
            {
                rewards = ItemNewHelper.GetPackageShowItems(item.Id);
                if (rewards.Count == 0) return ErrorCode.ERR_Parameter;
                if (item.ItemType == ItemTypeEnum.SubType_Package_Select)
                {
                    if (!int.TryParse(operatePar, out int index) || index < 1 || index > rewards.Count) return ErrorCode.ERR_Parameter;
                    rewards = new List<RewardItem> { rewards[index - 1] };
                }
                else if (item.ItemType != ItemTypeEnum.SubType_Package_All) return ErrorCode.ERR_Parameter;
                foreach (RewardItem reward in rewards)
                {
                    if (reward.ItemNum <= 0 || reward.ItemNum > int.MaxValue / useNum) return ErrorCode.ERR_Parameter;
                    reward.ItemNum *= useNum;
                }
            }
            DropHelper.ZhengLiRewardItems(rewards);
            foreach (RewardItem reward in rewards)
            {
                if (reward.ItemNum <= 0 || !ItemNewHelper.IsValidItem(reward.ItemType, reward.ItemID)) return ErrorCode.ERR_Parameter;
                // 自动开启包裹不能继续产生自动开启的包裹，防止循环发奖及绕过容量预检。
                if (reward.ItemType == ItemBigType.Type_Item)
                {
                    LDItem output = LDItemCategory.Instance.Get(reward.ItemID);
                    if (output.IfAutoUse == 1 && output.ItemType >= 11 && output.ItemType <= 13) return ErrorCode.ERR_Parameter;
                }
            }
            return ErrorCode.ERR_Success;
        }
    }
}
