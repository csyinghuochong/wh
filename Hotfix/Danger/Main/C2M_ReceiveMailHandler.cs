using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_ReceiveMailHandler : AMActorLocationRpcHandler<Unit, C2M_ReceiveMailRequest, M2C_ReceiveMailResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ReceiveMailRequest request, M2C_ReceiveMailResponse response, Action reply)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Received, unit.Id))
            {
                int zone = UnitZoneHelper.GetHomeZone(unit);
                DBMailInfo dBMailInfo = await DBHelper.GetComponent<DBMailInfo>(zone, unit.Id);
                MailInfo mailInfo = null;
                if (dBMailInfo != null)
                {
                    for (int i = dBMailInfo.MailInfoList.Count - 1; i >= 0; i--)
                    {
                        if (dBMailInfo.MailInfoList[i].MailId == request.MailId)
                        {
                            mailInfo = dBMailInfo.MailInfoList[i];
                            dBMailInfo.MailInfoList.RemoveAt(i);
                            break;
                        }
                    }

                    if (mailInfo != null)
                    {
                        await DBHelper.SaveComponent(zone, unit.Id, dBMailInfo);
                    }
                }

                if (mailInfo == null)
                {
                    reply();
                    return;
                }

                BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
                long receiveMailTime = TimeHelper.ServerNow();
                List<BagInfo> mailItems = mailInfo.ItemList;
                if (mailItems != null && mailItems.Count > 0)
                {
                    for (int i = mailItems.Count - 1; i >= 0; i--)
                    {
                        BagInfo item = mailItems[i];
                        if (!string.IsNullOrEmpty(item.GetWay))
                        {
                            bagComponentServer.OnAddItemData(item, item.GetWay);
                        }
                        else
                        {
                            bagComponentServer.OnAddItemData(item, $"{ItemGetWay.ReceieMail}_{receiveMailTime}");
                        }
                    }
                }
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
