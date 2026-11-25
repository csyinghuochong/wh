using System;
using System.Collections.Generic;

namespace ET
{
    public static class ItemUseHelper
    {
        /// <summary>校验归属、数量及使用次数后扣除道具；礼包先入包，溢出附件保存到邮箱后才返回成功。</summary>
        /// <param name="useBagInfo">背包使用传道具，获得后自动使用传空（不扣背包、不推 bagUpdate）。</param>
        /// <param name="useNum">使用数量。有格子时不能小于 1，也不能大于该格子 ItemNum。</param>
        /// <param name="locType">道具所在 ItemLocType，与客户端传入一致。</param>
        /// <param name="overflowMail">可选的溢出邮件接收对象，成功后附件非空表示本次已发送邮件。</param>
        public static async ETTask<int> UseItem(Unit unit, int itemId, BagInfo useBagInfo, M2C_RoleBagUpdate bagUpdate, int useNum, ItemLocType locType, string operatePar = "", MailInfo overflowMail = null)
        {
            itemId = useBagInfo != null ? useBagInfo.ItemID : itemId;
            if (useNum < 1 || itemId <= 0 || !LDItemCategory.Instance.Contain(itemId))
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
            }

            LDItem ldItem = LDItemCategory.Instance.Get(itemId);
            bool isPackage = ldItem.ItemType >= ItemTypeEnum.SubType_Package_All && ldItem.ItemType <= ItemTypeEnum.SubType_Package_Random;
            // 礼包先取得邮箱，避免扣除后才发现邮箱不可用；与邮件领取共用锁，保存后才返回使用成功。
            using CoroutineLock mailLock = isPackage ? await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Received, unit.Id) : null;
            if (unit.IsDisposed)
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
            }
            DBMailInfo mailBox = isPackage ? await DBHelper.GetComponent<DBMailInfo>(UnitZoneHelper.GetHomeZone(unit), unit.Id) : null;
            if (unit.IsDisposed)
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
            }
            if (isPackage && mailBox == null)
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Login_Unregister);
            }

            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            RoleDailyDataComponentServer daily = unit.GetComponent<RoleDailyDataComponentServer>();
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            if (useBagInfo != null)
            {
                // 等待邮箱期间背包可能发生变化，按当前归属、格子及数量重新校验。
                if (!ReferenceEquals(bagComponentServer.GetItemByLoc(locType, useBagInfo.BagInfoID), useBagInfo))
                {
                    return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Item_NotExist);
                }
                if (useNum < 1 || useNum > useBagInfo.ItemNum)
                {
                    return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_NoItem);
                }
            }

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
            int overflowMailConfigId = 0;
            if (isPackage)
            {
                int packageError = GetPackageRewards(ldItem, useNum, operatePar, out packageRewards);
                if (packageError != ErrorCode.ERR_Success) return packageError;
                overflowMailConfigId = LDMailCategory.Instance.GetMailByKey(MailKey.Mail_Over_Bag_Volume);
                if (overflowMailConfigId <= 0)
                {
                    return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Invalid_Data);
                }
            }

            int recipeMakeId = 0;
            if (ldItem.ItemType == ItemTypeEnum.SubType_Recipe_95)
            {
                recipeMakeId = ldItem.GetTypeParam1();
                if (recipeMakeId <= 0 || !LDSkill_MakeCategory.Instance.Contain(recipeMakeId))
                {
                    return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Invalid_Data);
                }
            }

            if (useBagInfo != null && !bagComponentServer.OnCostItemData(useBagInfo, locType, useNum))
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Item_NotExist);
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
                overflowMail ??= new MailInfo();
                AddPackageRewards(bagComponentServer, packageRewards, overflowMail.ItemList);
            }

            daily.AddDayItemUse(ldItem.Id, useNum);
            roleInfoComponentServer.AddTotalItemUse(ldItem.Id, useNum);

            if (recipeMakeId > 0)
            {
                roleInfoComponentServer.LearnRecipe(recipeMakeId);
            }

            if (overflowMail != null && overflowMail.ItemList.Count > 0)
            {
                overflowMail.MailId = IdGenerater.Instance.GenerateId();
                overflowMail.ConfigId = overflowMailConfigId;
                overflowMail.BelongId = overflowMail.GetBelongId();
                overflowMail.SendTime = TimeHelper.ServerNow();
                MailHelp.TrimForNewMail(mailBox.MailInfoList, overflowMail, unit.Id);
                mailBox.MailInfoList.Add(overflowMail);
                await DBHelper.SaveComponent(UnitZoneHelper.GetHomeZone(unit), unit.Id, mailBox);
            }

            return ErrorCode.ERR_Success;
        }

        /// <summary>扣除礼包后按实际空间逐项发奖：先叠加并占用空格，仅将无法入包的剩余数量转成邮件附件。</summary>
        public static void AddPackageRewards(BagComponentServer bag, List<RewardItem> rewards, List<BagInfo> overflowItems)
        {
            string getWay = $"{ItemGetWay.ItemBox_6}_{TimeHelper.ServerNow()}";
            foreach (RewardItem reward in rewards)
            {
                List<RewardItem> items = new List<RewardItem> { reward };
                if (bag.OnAddItemData(items, getWay))
                {
                    continue;
                }

                RewardItem partial = new RewardItem
                {
                    ItemType = reward.ItemType, ItemID = reward.ItemID, ItemFlags = reward.ItemFlags,
                };
                items[0] = partial;
                int low = 0;
                int high = reward.ItemNum;
                // 复用入包容量规则查找可放数量，避免空格为 0 时漏掉已有堆叠的剩余空间。
                while (low < high)
                {
                    int middle = low + (int)(((long)high - low + 1) / 2);
                    partial.ItemNum = middle;
                    if (bag.CanAddItemData(items))
                    {
                        low = middle;
                    }
                    else
                    {
                        high = middle - 1;
                    }
                }
                partial.ItemNum = low;
                if (low > 0 && !bag.OnAddItemData(items, getWay))
                {
                    low = 0;
                }
                overflowItems.Add(new BagInfo
                {
                    ItemType = reward.ItemType, ItemID = reward.ItemID, ItemNum = reward.ItemNum - low,
                    ItemFlags = ItemNewHelper.ResolveAddItemFlags(reward), GetWay = getWay,
                });
            }
        }

        /// <summary>
        /// 计算礼包奖励，不扣礼包、不入包、不发邮件；调用方校验成功并扣除礼包后，再入包并邮寄溢出部分。
        /// ItemType=11、13：Item.Reward 有配置时，读取全部条目，每项数量乘以 useNum；忽略 operatePar。
        /// Item.Reward 未配置时才读取 Item.Drop_Id，按 useNum 次独立执行 DropHelper.DropIDToDropItem，汇总实际结果；
        /// 遵循 LDDrop.Type 的抽取规则，允许抽空，不将一次抽取结果倍增。有 Reward 时不再抽取 Drop_Id。
        /// ItemType=12 任选：读取 Item.Reward，按 operatePar 指定的序号选择一项，该项数量乘以 useNum；同一批只选同一项。
        /// 最后按物品类型、ID、状态合并数量并校验奖励；禁止奖励中含有获得后自动开启的 11/12/13 礼包，避免循环发奖。
        /// </summary>
        /// <param name="item">Item 表中的礼包配置，仅支持 ItemType=11、12、13。</param>
        /// <param name="useNum">使用数量，单个传 1，批量传选定数量，范围 1~1000；库存和次数限制由调用方校验。</param>
        /// <param name="operatePar">12 传 Item.Reward 中从 1 开始的选项序号；11、13 传空。</param>
        /// <param name="rewards">计算后的奖励列表，仅返回成功时可发放；11/13 走 Drop_Id 合法抽空时列表可以为空。</param>
        /// <returns>成功返回 ERR_Success；数量、选项或奖励配置非法时返回对应的 Word_Prompt 提示 ID。</returns>
        public static int GetPackageRewards(LDItem item, int useNum, string operatePar, out List<RewardItem> rewards)
        {
            rewards = new List<RewardItem>();
            if (useNum < 1 || useNum > 1000)
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
            }
            if (item.ItemType != ItemTypeEnum.SubType_Package_All && item.ItemType != ItemTypeEnum.SubType_Package_Select
                && item.ItemType != ItemTypeEnum.SubType_Package_Random)
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
            }
            if (item.ItemType != ItemTypeEnum.SubType_Package_Select && string.IsNullOrWhiteSpace(item.Reward))
            {
                if (item.Drop_Id <= 0 || !LDDropCategory.Instance.Contain(item.Drop_Id))
                {
                    return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                }
                for (int i = 0; i < useNum; i++)
                {
                    DropHelper.DropIDToDropItem(item.Drop_Id, rewards);
                }
                // 11/13 未配 Reward 时按 Drop 规则抽取，概率 Drop 允许本次抽空。
            }
            else
            {
                rewards = ItemNewHelper.GetPackageShowItems(item.Id);
                if (rewards.Count == 0)
                {
                    return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                }
                if (item.ItemType == ItemTypeEnum.SubType_Package_Select)
                {
                    if (!int.TryParse(operatePar, out int index) || index < 1 || index > rewards.Count)
                    {
                        return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                    }
                    rewards = new List<RewardItem> { rewards[index - 1] };
                }
                foreach (RewardItem reward in rewards)
                {
                    if (reward.ItemNum <= 0 || reward.ItemNum > int.MaxValue / useNum)
                    {
                        return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                    }
                    reward.ItemNum *= useNum;
                }
            }
            DropHelper.ZhengLiRewardItems(rewards);
            foreach (RewardItem reward in rewards)
            {
                if (reward.ItemNum <= 0 || !ItemTradeStatus.CanGrant(reward.ItemFlags)
                        || !ItemNewHelper.IsValidItem(reward.ItemType, reward.ItemID))
                {
                    return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                }
                // 自动开启包裹不能继续产生自动开启的包裹，防止循环发奖及循环补发邮件。
                if (reward.ItemType == ItemBigType.Type_Item)
                {
                    LDItem output = LDItemCategory.Instance.Get(reward.ItemID);
                    if (output.IfAutoUse == 1 && output.ItemType >= 11 && output.ItemType <= 13)
                    {
                        return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                    }
                }
            }
            return ErrorCode.ERR_Success;
        }
    }
}
