using System;
using System.Collections.Generic;

namespace ET
{
    public static class MailHelp
    {

        /// <summary>后台附件：类型~ID~数量~交易状态，多项用 | 分隔；缺少状态时按 0 处理。</summary>
        public static bool TryGetGMMailItems(string itemList, out List<BagInfo> items)
        {
            items = new List<BagInfo>();
            if (string.IsNullOrWhiteSpace(itemList))
            {
                return false;
            }

            string getWay = $"{ItemGetWay.ReceieMail}_{TimeHelper.ServerNow()}";
            foreach (string reward in itemList.Split('|'))
            {
                string[] fields = reward.Split('~');
                if (fields.Length != 3 && fields.Length != 4)
                {
                    return false;
                }

                if (!int.TryParse(fields[0], out int itemType) || itemType <= 0
                    || !int.TryParse(fields[1], out int itemId) || itemId <= 0
                    || !int.TryParse(fields[2], out int itemNum) || itemNum <= 0)
                {
                    return false;
                }

                int status = 0;
                if (fields.Length == 4 && !string.IsNullOrEmpty(fields[3])
                    && (!int.TryParse(fields[3], out status) || status < 0))
                {
                    return false;
                }

                if (status != ItemFlagEnum.TradeStatus.Tradable && status != ItemFlagEnum.TradeStatus.BindOnEquip
                    && status != ItemFlagEnum.TradeStatus.DungeonTradable && status != ItemFlagEnum.TradeStatus.Limited
                    && status != ItemFlagEnum.TradeStatus.NonTradable && status != ItemFlagEnum.TradeStatus.Inherit)
                {
                    return false;
                }

                if (!ItemNewHelper.IsValidItem(itemType, itemId))
                {
                    return false;
                }

                items.Add(new BagInfo
                {
                    ItemType = itemType,
                    ItemID = itemId,
                    ItemNum = itemNum,
                    ItemFlags = ItemFlagEnum.ResolveItemFlags(status, 0),
                    GetWay = getWay,
                });
            }

            return true;
        }

        /// <summary>
        /// 指定寄售上架成功后，向服务端解析出的 TargetUserId 发送 Mail_Exchange_Designate 通知，不带附件。
        /// 模板参数顺序：0 卖家名字，1 商品名字；普通公开寄售不发送此通知。
        /// </summary>
        public static async ETTask SendConsignDesignateMail(ConsignItemInfo consignItem)
        {
            if (consignItem?.BagInfo == null || consignItem.TargetUserId <= 0)
            {
                return;
            }

            MailInfo mailInfo = new MailInfo();
            mailInfo.MailId = IdGenerater.Instance.GenerateId();
            mailInfo.ConfigId = LDMailCategory.Instance.GetMailByKey(MailKey.Mail_Exchange_Designate);
            mailInfo.ParamList.Add(consignItem.PlayerName);
            mailInfo.ParamList.Add(ItemNewHelper.GetItemName(consignItem.BagInfo));

            await SendUserMail(UnitZoneHelper.GetHomeZone(consignItem.TargetUserId), consignItem.TargetUserId, mailInfo);
        }

        /// <summary>
        /// 登录时按配置的游戏日切点检查一次交易暂存区，只对本人的过期商品生成提醒邮件。
        /// 检查进度和邮件一起保存；没有过期商品也记下首次检查，机器人跳过。此过程不发放或取回暂存资产。
        /// </summary>
        /// <returns>0 表示已检查或本游戏日已经检查过；非 0 为邮箱读取或交易服查询错误。</returns>
        public static async ETTask<int> SyncConsignStorageMails(Unit unit)
        {
            if (unit.IsRobot())
            {
                return ErrorCode.ERR_Success;
            }

            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Received, unit.Id))
            {
                int zone = UnitZoneHelper.GetHomeZone(unit);
                DBMailInfo mailBox = await DBHelper.GetComponent<DBMailInfo>(zone, unit.Id);
                if (mailBox == null)
                {
                    return ErrorCode.ERR_NotFindAccount;
                }

                long now = TimeHelper.ServerNow();
                if (mailBox.ConsignLoginCheckTime > 0 && ActivityHelper.IsSameGameResetDay(mailBox.ConsignLoginCheckTime, now))
                {
                    return ErrorCode.ERR_Success;
                }

                Consign2M_StorageResponse storage = await ConsignStorageHelper.RequestStorage(unit);
                if (storage.Error != ErrorCode.ERR_Success)
                {
                    return storage.Error;
                }

                ApplyConsignStorageMails(mailBox, storage.Items, unit.Id, now);
                await DBHelper.SaveComponent(zone, unit.Id, mailBox);
                return ErrorCode.ERR_Success;
            }
        }

        /// <summary>
        /// 将本次登录查询到的本人过期商品转为 Mail_Exchange_Failure 通知，模板参数 0 为商品名字，邮件不带附件。
        /// 本方法只更新内存邮箱和检查时间，调用方负责每日判重、加锁和保存。
        /// </summary>
        /// <param name="mailBox">当前角色的邮箱及上次暂存检查进度。</param>
        /// <param name="items">交易服返回的暂存快照；成交收益及非本人记录不会生成过期提醒。</param>
        /// <param name="unitId">当前角色 ID，用于核对记录归属。</param>
        /// <param name="now">本次检查的服务器毫秒时间戳，用于记录检查时间和邮件发送时间。</param>
        private static void ApplyConsignStorageMails(DBMailInfo mailBox, List<ConsignStorageInfo> items, long unitId, long now)
        {
            foreach (ConsignStorageInfo item in items)
            {
                if (!item.ExpiredItem || item.UserId != unitId)
                {
                    continue;
                }

                MailInfo mailInfo = new MailInfo();
                mailInfo.MailId = IdGenerater.Instance.GenerateId();
                mailInfo.ConfigId = LDMailCategory.Instance.GetMailByKey(MailKey.Mail_Exchange_Failure);
                mailInfo.BelongId = mailInfo.GetBelongId();
                mailInfo.SendTime = now;
                mailInfo.ParamList.Add(ItemNewHelper.GetItemName(item.BagInfo));
                TrimForNewMail(mailBox.MailInfoList, mailInfo, unitId);
                mailBox.MailInfoList.Add(mailInfo);
            }

            // 没有暂存商品也记下首次检查，避免当天后续登录补发。
            mailBox.ConsignLoginCheckTime = now;
        }

        public static async ETTask SendWantBuyItemMail(long userId, BagInfo bagInfo)
        {
            if (userId <= 0 || bagInfo == null)
            {
                return;
            }

            MailInfo mailInfo = new MailInfo();
            mailInfo.Status = 0;
            mailInfo.MailId = IdGenerater.Instance.GenerateId();
            bagInfo.GetWay = $"{ItemGetWay.PaiMaiBuy}_{TimeHelper.ServerNow()}";
            mailInfo.ItemList.Add(bagInfo);

            int homeZone = UnitZoneHelper.GetHomeZone(userId);
            await SendUserMail(homeZone, userId, mailInfo);
        }

        public static async ETTask SendWantBuyGoldMail(long userId, long goldNum)
        {
            if (userId <= 0 || goldNum <= 0)
            {
                return;
            }

            MailInfo mailInfo = new MailInfo();
            mailInfo.Status = 0;
            mailInfo.MailId = IdGenerater.Instance.GenerateId();
            BagInfo gold = new BagInfo();
            gold.ItemID = UserDataType.Gold;
            gold.ItemType = ItemBigType.Type_Item;
            gold.ItemNum = goldNum > int.MaxValue ? int.MaxValue : (int)goldNum;
            gold.GetWay = $"{ItemGetWay.PaiMaiSell}_{TimeHelper.ServerNow()}";
            mailInfo.ItemList.Add(gold);

            int homeZone = UnitZoneHelper.GetHomeZone(userId);
            await SendUserMail(homeZone, userId, mailInfo);
        }

        /// <summary>
        /// 上架商品成交后向卖家发送 Mail_Exchange_Success 通知，不带金币附件；收益发放待个人仓库接入。
        /// 模板参数顺序：0 商品名字，1 成交总价，2 手续费，3 实收；后三项均按铜币填写并带单位。
        /// </summary>
        /// <param name="paiMaiItemInfo">交易服返回的实际成交数据，包含卖家 ID、商品、成交单价及实际成交数量。</param>
        public static async ETTask SendPaiMaiEmail(ConsignItemInfo paiMaiItemInfo)
        {
            if (paiMaiItemInfo?.BagInfo == null || paiMaiItemInfo.UserId <= 0 || paiMaiItemInfo.BagInfo.ItemNum <= 0)
            {
                return;
            }

            long totalGold = (long)paiMaiItemInfo.Price * paiMaiItemInfo.BagInfo.ItemNum;
            if (totalGold < 0)
            {
                return;
            }

            long fee = ConsignHelper.GetConsignTradeFee(totalGold);
            long goldNum = totalGold - fee;
            MailInfo mailInfo = new MailInfo();
            mailInfo.Status = 0;
            mailInfo.MailId = IdGenerater.Instance.GenerateId();
            mailInfo.ConfigId = LDMailCategory.Instance.GetMailByKey(MailKey.Mail_Exchange_Success);
            mailInfo.ParamList.Add(ItemNewHelper.GetItemName(paiMaiItemInfo.BagInfo));
            mailInfo.ParamList.Add($"{totalGold}铜币");
            mailInfo.ParamList.Add($"{fee}铜币");
            mailInfo.ParamList.Add($"{goldNum}铜币");
            await SendUserMail(UnitZoneHelper.GetHomeZone(paiMaiItemInfo.UserId), paiMaiItemInfo.UserId, mailInfo);
        }

        /// <summary>在线通知和登录补发共用入口；与领取附件共用角色锁，每批只读写一次邮箱。</summary>
        public static async ETTask<int> SyncServerMails(Unit unit)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Received, unit.Id))
            {
                int zone = UnitZoneHelper.GetHomeZone(unit);
                DBMailInfo mailBox = await DBHelper.GetComponent<DBMailInfo>(zone, unit.Id);
                if (mailBox == null)
                {
                    return ErrorCode.ERR_NotFindAccount;
                }

                RoleInfo roleInfo = unit.GetComponent<RoleInfoComponentServer>().RoleInfo;
                int currentMailId = Math.Max(roleInfo.ServerMailIdCur, mailBox.ServerMailIdCur);
                if (roleInfo.ServerMailIdCur == -1 && mailBox.ServerMailIdCur == 0)
                {
                    currentMailId = -1;
                }

                Mail2G_EnterMail response = (Mail2G_EnterMail)await MessageHelper.CallActor(DBHelper.GetMailServerId(unit), new G2Mail_EnterMail
                {
                    UnitId = unit.Id,
                    ServerMailIdCur = currentMailId,
                });
                if (response.Error != ErrorCode.ERR_Success)
                {
                    return response.Error;
                }

                if (ApplyServerMails(unit, mailBox, response))
                {
                    await DBHelper.SaveComponent(zone, unit.Id, mailBox);
                }

                roleInfo.ServerMailIdCur = mailBox.ServerMailIdCur;
                return ErrorCode.ERR_Success;
            }
        }

        public static bool CheckSendMail(int MailType, string Title, long rechargeNum, RoleInfoComponentServer roleInfoComponentServer)
        {
            if (roleInfoComponentServer == null)
            {
                return false;
            }

            switch (MailType)
            {
                case 2: // 充值>=6元 10011003
                    if (!long.TryParse(Title, out long minRecharge) || minRecharge < 0 || rechargeNum < minRecharge)
                    {
                        return false;
                    }
                    break;
                case 3: //20级以上 补
                    if (!int.TryParse(Title, out int minLevel) || minLevel < 0 || roleInfoComponentServer.RoleInfo.Lv < minLevel)
                    {
                        return false;
                    }
                    break;
                case 5:
                    // 充值>=6<30元 10011003
                    //充值额度某个区间段
                    string[] needrecharge = Title.Split('_');
                    if (needrecharge.Length != 2
                        || !long.TryParse(needrecharge[0], out long min_value) || min_value < 0
                        || !long.TryParse(needrecharge[1], out long max_value) || max_value <= min_value
                        || rechargeNum < min_value
                        || rechargeNum >= max_value)
                    {
                        return false;
                    }
                    break;
                case 6:
                    break;
                default:
                    break;
            }
            //Log.Console($"CheckSendMail.true : {MailType} {Title}");
            return true;
        }

        /// <summary>按快照顺序投递，筛选使用当前角色数据；过期清理、邮件内容和处理进度由调用方一次保存。</summary>
        public static bool ApplyServerMails(Unit unit, DBMailInfo mailBox, Mail2G_EnterMail response)
        {
            RoleInfoComponentServer role = unit.GetComponent<RoleInfoComponentServer>();
            long rechargeNum = unit.GetComponent<RechargeComponentServer>().GetTotalRechargeNum();
            bool changed = RemoveExpiredMails(mailBox.MailInfoList, TimeHelper.ServerNow());
            foreach (ServerMailItem item in response.ServerMailItems)
            {
                if (item.ServerMailIId <= mailBox.ServerMailIdCur || role.RoleInfo.RobotId > 0
                    || string.IsNullOrEmpty(item.ParasmNew)
                    || !CheckSendMail(item.MailType, item.ParasmNew, rechargeNum, role))
                {
                    continue;
                }

                MailInfo mailInfo = new MailInfo
                {
                    ConfigId = 0,
                    MailId = IdGenerater.Instance.GenerateId(),
                    SendTime = TimeHelper.ServerNow(),
                };
                mailInfo.ParamList.Add("奖励");
                mailInfo.ParamList.Add("全服补偿邮件");
                foreach (BagInfo reward in item.ItemList)
                {
                    mailInfo.ItemList.Add(new BagInfo
                    {
                        ItemType = reward.ItemType,
                        ItemID = reward.ItemID,
                        ItemNum = reward.ItemNum,
                        ItemFlags = reward.ItemFlags,
                        GetWay = reward.GetWay,
                    });
                }

                TrimForNewMail(mailBox.MailInfoList, mailInfo, unit.Id);
                mailBox.MailInfoList.Add(mailInfo);
                changed = true;
            }

            int currentMailId = Math.Max(mailBox.ServerMailIdCur, Math.Max(role.RoleInfo.ServerMailIdCur, response.ServerMailIdMax));
            changed |= mailBox.ServerMailIdCur != currentMailId;
            mailBox.ServerMailIdCur = currentMailId;
            return changed;
        }

        //指定玩家发送邮件
        public static async ETTask<int> SendUserMail(int zone,long userID, MailInfo mailInfo )
        {
            DBMailInfo dBMainInfo = await DBHelper.GetComponent<DBMailInfo>(zone, userID);
            if (dBMainInfo == null)
            {
                return ErrorCode.ERR_NotFindAccount;
            }

            List<MailInfo> mailinfolist = dBMainInfo.MailInfoList;
            if (mailInfo.BelongId <= 0)
            {
                mailInfo.BelongId = mailInfo.GetBelongId();
            }

            if (mailInfo.SendTime <= 0)
            {
                mailInfo.SendTime = TimeHelper.ServerNow();
            }

            TrimForNewMail(mailinfolist, mailInfo, userID);
            mailinfolist.Add(mailInfo);

            await DBHelper.SaveComponent(zone, userID, dBMainInfo);
            return ErrorCode.ERR_Success;
        }

        /// <summary>
        /// 删掉已过期邮件。ValidTime 小于等于 0 视为永久；小于 1000000 是剩余天数，没有发送时间无法换算，不删。
        /// </summary>
        public static bool RemoveExpiredMails(List<MailInfo> mailList, long now)
        {
            if (mailList == null || mailList.Count == 0)
            {
                return false;
            }

            bool changed = false;
            for (int i = mailList.Count - 1; i >= 0; i--)
            {
                if (IsMailExpired(mailList[i], now))
                {
                    mailList.RemoveAt(i);
                    changed = true;
                }
            }

            return changed;
        }

        public static MailInfo FindMail(List<MailInfo> mailList, long mailId)
        {
            if (mailList == null)
            {
                return null;
            }

            for (int i = 0; i < mailList.Count; i++)
            {
                MailInfo mailInfo = mailList[i];
                if (mailInfo != null && mailInfo.MailId == mailId)
                {
                    return mailInfo;
                }
            }

            return null;
        }

        /// <summary>有附件且还没领。</summary>
        public static bool HasUnclaimedReward(MailInfo mailInfo)
        {
            if (mailInfo == null || mailInfo.RewardReceived)
            {
                return false;
            }

            return mailInfo.ItemList != null && mailInfo.ItemList.Count > 0;
        }

        /// <summary>邮件所属 BelongId。空邮件返回 0。</summary>
        public static int GetBelongId(MailInfo mailInfo)
        {
            return mailInfo == null ? 0 : mailInfo.BelongId;
        }

        public static bool GrantMailReward(BagComponentServer bag, MailInfo mailInfo)
        {
            if (bag == null || !HasUnclaimedReward(mailInfo))
            {
                return false;
            }

            List<BagInfo> mailItems = mailInfo.ItemList;
            Dictionary<int, int> needByLoc = new Dictionary<int, int>();
            for (int i = 0; i < mailItems.Count; i++)
            {
                BagInfo item = mailItems[i];
                if (item == null)
                {
                    continue;
                }

                if (ItemNewHelper.GetItemToUserDataType(item.ItemType, item.ItemID) != UserDataType.None)
                {
                    continue;
                }

                ItemLocType toLoc = ItemNewHelper.GetToItemLocType(item.ItemType, item.ItemID);
                if (!ItemNewHelper.IsValideBagLoc((int)toLoc)) return false;
                List<BagInfo> locList = bag.GetItemByLoc(toLoc);
                int pileSum = ItemNewHelper.GetNewItemPileSum(item);
                int need = ItemNewHelper.CalcNeedNewCells(locList, item.ItemType, item.ItemID, item.ItemNum, pileSum, item.ItemFlags, item.MakePlayer, item.MakeTime);
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
                if (bag.GetBagLeftCell(kv.Key) < kv.Value)
                {
                    return false;
                }
            }

            long receiveMailTime = TimeHelper.ServerNow();
            for (int i = mailItems.Count - 1; i >= 0; i--)
            {
                BagInfo item = mailItems[i];
                if (item == null)
                {
                    continue;
                }

                string getWay = string.IsNullOrEmpty(item.GetWay)
                        ? $"{ItemGetWay.ReceieMail}_{receiveMailTime}"
                        : item.GetWay;
                bag.OnAddItemData(item, getWay);
            }

            mailInfo.RewardReceived = true;
            return true;
        }

        /// <summary>
        /// 新邮件入库前：先删过期，再按该邮件所属页签腾位。
        /// 优先删最老的【没有奖励 / 奖励已领取】；该页签全是未领奖励时，删最老的一封。
        /// </summary>
        public static void TrimForNewMail(List<MailInfo> mailList, MailInfo newMail, long userId)
        {
            if (mailList == null)
            {
                return;
            }

            RemoveExpiredMails(mailList, TimeHelper.ServerNow());
            int belongId = GetBelongId(newMail);
            int maxNum = LDGlobalValueCategory.Instance.MailMaxNum;
            int count = CountTab(mailList, belongId);
            while (count >= maxNum)
            {
                int index = FindEvictIndex(mailList, belongId);
                if (index < 0)
                {
                    break;
                }

                MailInfo removed = mailList[index];
                if (HasUnclaimedReward(removed))
                {
                    Log.Warning($"邮箱已满且该页签都是未领奖励，删除最老邮件 userId={userId} belongId={belongId} mailId={removed.MailId}");
                }

                mailList.RemoveAt(index);
                count--;
            }
        }

        public static bool IsMailExpired(MailInfo mailInfo, long now)
        {
            if (mailInfo == null)
            {
                return true;
            }

            long validTime = mailInfo.ValidTime;
            if (validTime <= 0 || validTime < 1000000)
            {
                return false;
            }

            long expireTime = validTime > 1000000000000L ? validTime : validTime * 1000;
            return now >= expireTime;
        }

        private static int CountTab(List<MailInfo> mailList, int belongId)
        {
            int count = 0;
            for (int i = 0; i < mailList.Count; i++)
            {
                if (GetBelongId(mailList[i]) == belongId)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>列表顺序即入库顺序，下标靠前的更老。</summary>
        private static int FindEvictIndex(List<MailInfo> mailList, int belongId)
        {
            int oldest = -1;
            for (int i = 0; i < mailList.Count; i++)
            {
                if (GetBelongId(mailList[i]) != belongId)
                {
                    continue;
                }

                if (oldest < 0)
                {
                    oldest = i;
                }

                if (!HasUnclaimedReward(mailList[i]))
                {
                    return i;
                }
            }

            return oldest;
        }
    }
}
