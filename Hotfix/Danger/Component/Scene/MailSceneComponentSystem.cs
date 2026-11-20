using System;
using System.Collections.Generic;

namespace ET
{

    [Timer(TimerType.MailSceneTimer)]
    public class MailSceneTimer : ATimer<MailSceneComponent>
    {
        public override void Run(MailSceneComponent self)
        {
            try
            {
                self.SaveDB().Coroutine();
            }
            catch (Exception e)
            {
                Log.Error($"move timer error: {self.Id}\n{e}");
            }
        }
    }


    [ObjectSystem]
    public class MailSceneComponentAwakeSystem : AwakeSystem<MailSceneComponent>
    {
        public override void Awake(MailSceneComponent self)
        {
            self.InitServerInfo().Coroutine();

            self.Timer = TimerComponent.Instance.NewRepeatedTimer(TimeHelper.Hour * 10 + self.DomainZone() * 1000, TimerType.MailSceneTimer, self);
        }
    }

    public static class MailSceneComponentSystem
    {

        public static async ETTask InitServerInfo(this MailSceneComponent self)
        {
            DBServerMailInfo dBServerInfo = await DBHelper.GetComponent<DBServerMailInfo>(self.DomainZone(), self.DomainZone());
            if (dBServerInfo == null)
            {
                dBServerInfo = self.AddChildWithId<DBServerMailInfo>((long)self.DomainZone());
            }
            else
            {
                self.AddChild(dBServerInfo);
            }
            self.dBServerMailInfo = dBServerInfo;
            self.SaveDB().Coroutine();
        }

        public static int GetMaxMaild(this MailSceneComponent self)
        {
            int maxId = 0; 
            foreach ((int id, ServerMailItem ServerItem) in self.dBServerMailInfo.ServerMailList)
            {
                if (id >= maxId)
                {
                    maxId = id;
                }
            }
            return maxId;
        }


        /// <summary>取得当前进度之后的邮件快照；按 ID 升序，-1 保留新角色不补发历史邮件的规则。</summary>
        public static List<ServerMailItem> GetPendingServerMails(this MailSceneComponent self, int currentMailId)
        {
            List<ServerMailItem> items = new List<ServerMailItem>();
            if (currentMailId == -1)
            {
                return items;
            }

            foreach (ServerMailItem item in self.dBServerMailInfo.ServerMailList.Values)
            {
                if (item.ServerMailIId > currentMailId)
                {
                    items.Add(item);
                }
            }

            items.Sort((left, right) => left.ServerMailIId.CompareTo(right.ServerMailIId));
            return items;
        }

        /// <summary>
        /// 全服邮件
        /// </summary>
        public static async ETTask<int> OnServerMail(this MailSceneComponent self, M2E_GMEMailSendRequest request, List<BagInfo> items)
        {
            int mailId;
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.EMail, -self.DomainZone()))
            {
                int maxId = self.GetMaxMaild();
                if (maxId == int.MaxValue)
                {
                    return ErrorCode.ERR_Parameter;
                }

                mailId = maxId + 1;
                DBServerMailInfo snapshot = new DBServerMailInfo
                {
                    Id = self.dBServerMailInfo.Id,
                    ServerMailList = new Dictionary<int, ServerMailItem>(self.dBServerMailInfo.ServerMailList),
                };
                snapshot.ServerMailList.Add(mailId, new ServerMailItem
                {
                    MailType = request.MailType,
                    ItemList = items,
                    ParasmNew = request.Title,
                    ServerMailIId = mailId,
                });

                // 先保存，再发布给登录补发和在线通知；保存失败时不暴露未落库的邮件。
                await DBHelper.SaveComponent(self.DomainZone(), self.DomainZone(), snapshot);
                self.dBServerMailInfo.ServerMailList = snapshot.ServerMailList;
            }

            self.SendAllOnLineMail(mailId).Coroutine();
            return ErrorCode.ERR_Success;
        }

        public static async ETTask SendAllOnLineMail(this MailSceneComponent self, int serverMailId)
        {
            int zone = self.DomainZone();
            long chatServerId = StartSceneConfigCategory.Instance.GetBySceneName(zone, Enum.GetName(SceneType.Chat)).InstanceId;
            Chat2Mail_GetUnitList response = (Chat2Mail_GetUnitList)await MessageHelper.CallActor(chatServerId, new Mail2Chat_GetUnitList());
            if (response.Error != ErrorCode.ERR_Success)
            {
                return;
            }

            foreach (long unitId in response.OnlineUnitIdList)
            {
                MessageHelper.SendToLocationActor(unitId, new Mail2M_SendServerMailItem
                {
                    ServerMailIdMax = serverMailId,
                });
            }
        }



        public static async ETTask SaveDB(this MailSceneComponent self)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.EMail, -self.DomainZone()))
            {
                await DBHelper.SaveComponent(self.DomainZone(), self.DomainZone(), self.dBServerMailInfo);
            }
        }

    }
}
