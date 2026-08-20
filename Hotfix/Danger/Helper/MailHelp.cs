using System;
using System.Collections.Generic;

namespace ET
{
    public static class MailHelp
    {

        public static async ETTask SendPaiMaiEmail(int zone, ConsignItemInfo paiMaiItemInfo,int costNum, long unitid)
        {
            await ETTask.CompletedTask;

            Log.Error("MailInfo mailInfo = new MailInfo");
            MailInfo mailInfo = new MailInfo();
            LDItem ldItemCof = LDItemCategory.Instance.Get(paiMaiItemInfo.BagInfo.ItemID);
            //mailInfo.Status = 0;
            //mailInfo.Context = "你拍卖行出售的道具:" + ldItemCof.Name + ",已经被其他玩家购买" + costNum + "个。";
            //mailInfo.Title = "拍卖行邮件";
            mailInfo.MailId = IdGenerater.Instance.GenerateId();
            BagInfo reward = new BagInfo();
            reward.ItemID = 1;
            int sellPrice = (int)(paiMaiItemInfo.Price * 0.95f) * costNum;     //5%手续费
            reward.ItemNum = sellPrice;
            reward.GetWay = $"{ItemGetWay.PaiMaiSell}_{TimeHelper.ServerNow()}";
            mailInfo.ItemList.Add(reward);
           
            //发送到邮件服
            long mailServerId = StartSceneConfigCategory.Instance.GetBySceneName(zone, "EMail").InstanceId;      //获取邮件消息ID
            //E2M_EMailSendResponse g_EMailSendResponse = (E2M_EMailSendResponse)await ActorMessageSenderComponent.Instance.Call
            //    (mailServerId, new M2E_EMailSendRequest() { Id = paiMaiItemInfo.UserId, MailInfo = mailInfo });
        }

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
            long dbCacheId = DBHelper.GetDbCacheId(zone);
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

            //存储邮件
            if (mailinfolist.Count > 150)
            {
                mailinfolist.RemoveAt(0);
            }
            mailinfolist.Add(mailInfo);

            DBHelper.SaveComponent(zone, userID, dBMainInfo).Coroutine();
            return ErrorCode.ERR_Success;
        }
    }
}
