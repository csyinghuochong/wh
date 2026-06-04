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
            
            long dbCacheId = DBHelper.GetDbCacheId(self.DomainZone());
            await TimerComponent.Instance.WaitAsync( RandomHelper.RandomNumber(4000,10000) );
            DBServerMailInfo dBServerInfo = await DBHelper.GetComponent<DBServerMailInfo>(self.DomainZone(), self.DomainZone());
            if (dBServerInfo == null)
            {
                dBServerInfo = new DBServerMailInfo();
                dBServerInfo.Id = self.DomainZone();
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




        public static async ETTask<int> OnLogin(this MailSceneComponent self, long unitid, int curmailid)
        {
            //检测没有发送的邮件
            foreach ((int id, ServerMailItem ServerItem) in self.dBServerMailInfo.ServerMailList)
            {
                if (curmailid >= id)
                {
                    continue;
                }
                if (!string.IsNullOrEmpty(ServerItem.ParasmNew))
                {
                   await  MailHelp.ServerMailItem(self.DomainZone(), unitid, ServerItem);
                }
                
                curmailid = id;
            }
            return curmailid;
        }


        public static async ETTask SaveDB(this MailSceneComponent self)
        {
            await DBHelper.SaveComponent(self.DomainZone(),self.DomainZone(), self.dBServerMailInfo);
        }

    }
}
