using System;
using System.Collections.Generic;

namespace ET
{
    //游戏服务器处理
    [MessageHandler]
    public class C2R_ServerListHandler : AMRpcHandler<C2R_ServerList, R2C_ServerList>
    {
        protected override async ETTask Run(Session session, C2R_ServerList request, R2C_ServerList response, Action reply)
        {
            try
            {
                if (session.GetComponent<SessionLockingComponent>() != null)
                {
                    response.Error = ErrorCode.ERR_RequestRepeatedly;
                    reply();
                    session.Disconnect().Coroutine();
                    return;
                }

                using (session.AddComponent<SessionLockingComponent>())
                {
                    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.GetServerList, 0))
                    {
                        long serverTime = TimeHelper.ServerNow();
                        List<ServerItem> serverItems = ServerHelper.GetServerList();

                        //Console.WriteLine($"C2A_ServerList: ServerItems {ServerHelper.GetServerList().Count}");

                        response.ServerItems.Clear();
                        for (int i = 0; i < serverItems.Count; i++)
                        {
                            if (serverItems[i].Show != 0 && serverItems[i].ServerOpenTime <= serverTime)
                            {
                                response.ServerItems.Add(serverItems[i]);
                            }
                        }
                        response.Message = session.DomainScene().GetComponent<CenterServerComponent>().TianQiValue.ToString();
                        string[] stringxxx = LogHelper.GetNoticeNew().Split('@');

                        long timeNow = TimeHelper.ServerNow();
                        long timeColse = 0;
                        if (stringxxx.Length == 3)
                        {
                            response.NoticeVersion = stringxxx[0];
                            timeColse = long.Parse(stringxxx[1]);
                            response.NoticeText = stringxxx[2];
                        }
                        
                        string[] stringxxx_EN = LogHelper.GetNoticeNew_EN().Split('@');
                        if (stringxxx_EN.Length == 3)
                        {
                            response.NoticeVersion_EN = stringxxx_EN[0];
                            timeColse = long.Parse(stringxxx_EN[1]);
                            response.NoticeText_EN = stringxxx_EN[2];
                        }

                        if (timeColse > 0 && timeNow > timeColse + TimeHelper.OneDay * 3)
                        {
                            response.ShowNotice = false;
                        }
                        else
                        {
                            response.ShowNotice = true;
                        }
                        
                        string outeIp = StartMachineConfigCategory.Instance.Get(1).OuterIP;
                        response.SmsVerifyType = 0; //0 mob  1 aliyun
                        reply();
                        await ETTask.CompletedTask;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
    }
}
