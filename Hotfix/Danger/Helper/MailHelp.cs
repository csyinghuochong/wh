using System;
using System.Collections.Generic;

namespace ET
{
    public static class MailHelp
    {

        public static async ETTask SendConsignOverTimeMail(ConsignItemInfo paiMaiItemInfo)
        {
            if (paiMaiItemInfo?.BagInfo == null || paiMaiItemInfo.UserId <= 0)
            {
                return;
            }

            MailInfo mailInfo = new MailInfo();
            mailInfo.Status = 0;
            mailInfo.MailId = IdGenerater.Instance.GenerateId();
            paiMaiItemInfo.BagInfo.GetWay = $"{ItemGetWay.XiaJia}_{TimeHelper.ServerNow()}";
            mailInfo.ItemList.Add(paiMaiItemInfo.BagInfo);

            int homeZone = UnitZoneHelper.GetHomeZone(paiMaiItemInfo.UserId);
            await SendUserMail(homeZone, paiMaiItemInfo.UserId, mailInfo);
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

        public static async ETTask SendPaiMaiEmail(int zone, ConsignItemInfo paiMaiItemInfo, int costNum, long unitid)
        {
            if (paiMaiItemInfo?.BagInfo == null || paiMaiItemInfo.UserId <= 0 || costNum <= 0)
            {
                return;
            }

            long goldNum = ConsignHelper.GetConsignSellerGold((long)paiMaiItemInfo.Price * costNum);
            if (goldNum <= 0)
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

            await SendUserMail(zone, paiMaiItemInfo.UserId, mailInfo);
        }

        /// <summary>
        /// 用来发送全服邮件的 不要乱用
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="userID"></param>
        /// <param name="serverMailItem"></param>
        public static void  SendServerMail(int zone, long userID, ServerMailItem serverMailItem)
        {
            Mail2M_SendServerMailItem mail2M_SendServer = new Mail2M_SendServerMailItem();
            mail2M_SendServer.ServerMailItem = serverMailItem;
            MessageHelper.SendToLocationActor( userID, mail2M_SendServer);
        }

        public static bool CheckSendMail(int MailType, string Title, long rechargeNum, RoleInfoComponentServer roleInfoComponentServer, BagComponentServer bagComponentServer)
        {
            if (roleInfoComponentServer == null || bagComponentServer == null)
            {
                return false;
            }

            switch (MailType)
            {
                case 2: // 充值>=6元 10011003
                    if (rechargeNum < int.Parse(Title))
                    {
                        return false;
                    }
                    break;
                case 3: //20级以上 补
                    if (roleInfoComponentServer.RoleInfo.Lv < int.Parse(Title))
                    {
                        return false;
                    }
                    break;
                case 5:
                    // 充值>=6<30元 10011003
                    //充值额度某个区间段
                    string[] needrecharge = Title.Split('_');
                    int min_value = int.Parse(needrecharge[0]);
                    int max_value = int.Parse(needrecharge[1]);
                    if (rechargeNum < min_value
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

        public static async ETTask ServerMailItem(int zone, long userID, ServerMailItem serverMailItem)
        {
            //判断条件
            long dbCacheId = DBHelper.GetDbCacheId(zone);

            RoleInfoComponentServer roleInfoComponentServer =await DBHelper.GetComponent<RoleInfoComponentServer>(zone, userID);
            if (roleInfoComponentServer == null || roleInfoComponentServer.RoleInfo.RobotId > 0)
            {
                return;
            }
            RechargeComponentServer rechargeComponentServer = await DBHelper.GetComponent<RechargeComponentServer>(zone, userID);
            BagComponentServer bagComponentServer = await DBHelper.GetComponent<BagComponentServer>(zone, userID);
            if (bagComponentServer == null)
            {
                return;
            }

            long rechargeNum = rechargeComponentServer?.GetTotalRechargeNum() ?? 0;
            bool cansendMail = MailHelp.CheckSendMail(serverMailItem.MailType, serverMailItem.ParasmNew, rechargeNum, roleInfoComponentServer, bagComponentServer);
            if (cansendMail == false)
            {
                return;
            }
            Log.Error("MailInfo mailInfo = new MailInfo");
            MailInfo mailInfo = new MailInfo();
            mailInfo.Status = 0;
            //mailInfo.Title = "奖励";
            //mailInfo.Context = "全服补偿邮件";
            mailInfo.ItemList = serverMailItem.ItemList;
            mailInfo.MailId = IdGenerater.Instance.GenerateId();
            await SendUserMail(zone, userID, mailInfo);
        }

        //指定玩家发送邮件
        public static async ETTask<int> SendUserMail(int zone,long userID, MailInfo mailInfo )
        {
            DBMailInfo dBMainInfo = await DBHelper.GetComponent<DBMailInfo>(zone, userID);
            if (dBMainInfo == null)
            {
                //有可能玩家自己删除角色了。。还收到邮件。。列如：道具被拍卖。。。。=====
                //dBMainInfo = (DBMailInfo)await DBHelper.AddDataComponent<DBMailInfo>(zone, userID, DBHelper.DBMailInfo);
                Console.WriteLine($"AddDataComponent.DBMailInfo  {userID}");
            }
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

        public static void GrantMailReward(BagComponentServer bag, MailInfo mailInfo)
        {
            if (bag == null || !HasUnclaimedReward(mailInfo))
            {
                return;
            }

            long receiveMailTime = TimeHelper.ServerNow();
            List<BagInfo> mailItems = mailInfo.ItemList;
            for (int i = mailItems.Count - 1; i >= 0; i--)
            {
                BagInfo item = mailItems[i];
                if (item == null)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(item.GetWay))
                {
                    bag.OnAddItemData(item, item.GetWay);
                }
                else
                {
                    bag.OnAddItemData(item, $"{ItemGetWay.ReceieMail}_{receiveMailTime}");
                }
            }

            mailInfo.RewardReceived = true;
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
            while (CountTab(mailList, belongId) >= maxNum)
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
