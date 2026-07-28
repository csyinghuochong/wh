using System;
using System.Collections.Generic;
using System.Linq;


namespace ET
{

    [ActorMessageHandler]
    public class C2M_EMailReceiveHandler : AMActorLocationRpcHandler<Unit, C2M_ReceiveMailRequest, M2C_ReceiveMailResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ReceiveMailRequest request, M2C_ReceiveMailResponse response, Action reply)
        {
            //领取邮件
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Received, unit.Id))
            {
                RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
                ChengJiuComponentServer chengJiuComponentServer = unit.GetComponent<ChengJiuComponentServer>();
                long userId = roleInfoComponentServer.RoleInfo.UserId;
                long mailServerId = DBHelper.GetMailServerId(unit);
                Mail2M_ReceiveMailResponse g_SendChatRequest = (Mail2M_ReceiveMailResponse)await ActorMessageSenderComponent.Instance.Call
                    (mailServerId, new M2Mail_ReceiveMailRequest() { Id = userId, MailId = request.MailId });

                MailInfo mailInfo = g_SendChatRequest.MailInfo;
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
                    // 统一 GetWay 的邮件走批量；混用 GetWay 时仍逐个加，避免绑错来源
                    string sharedGetWay = null;
                    bool sameGetWay = true;
                    for (int i = 0; i < mailItems.Count; i++)
                    {
                        BagInfo item = mailItems[i];
                        if (item.ItemID == 110000164)
                        {
                            item.ItemID = 10000164;
                        }
                        string itemGetWay = !string.IsNullOrEmpty(item.GetWay)
                            ? item.GetWay
                            : $"{ItemGetWay.ReceieMail}_{receiveMailTime}";
                        if (sharedGetWay == null)
                        {
                            sharedGetWay = itemGetWay;
                        }
                        else if (sharedGetWay != itemGetWay)
                        {
                            sameGetWay = false;
                        }
                    }

                    if (sameGetWay && mailItems.Count > 1)
                    {
                        bagComponentServer.OnAddItemData(mailItems, sharedGetWay);
                    }
                    else
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
                
                if (mailInfo.ItemSell != null)
                {
                    LDItem ldItem = LDItemCategory.Instance.Get(mailInfo.ItemSell.ItemID);
                    if (ldItem.ItemType == 3)
                    {
                        chengJiuComponentServer.OnPaiMaiSell();
                    }
                }
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
