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
                long mailServerId = DBHelper.GetMailServerId(unit);
                E2M_EMailReceiveResponse g_SendChatRequest = (E2M_EMailReceiveResponse)await ActorMessageSenderComponent.Instance.Call
                    (mailServerId, new M2E_EMailReceiveRequest() { Id = roleInfoComponentServer.RoleInfo.UserId, MailId = request.MailId });

                MailInfo mailInfo = g_SendChatRequest.MailInfo;
                if (mailInfo == null)
                {
                    reply();
                    return;
                }

                BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
                long receiveMailTime = TimeHelper.ServerNow();
                for (int i = mailInfo.ItemList.Count - 1; i >= 0; i--)
                {
                    if (mailInfo.ItemList[i].ItemID == 110000164)

                    {
                        mailInfo.ItemList[i].ItemID = 10000164;
                    }
                    if (!string.IsNullOrEmpty(mailInfo.ItemList[i].GetWay))
                    {
                        bagComponentServer.OnAddItemData(mailInfo.ItemList[i], mailInfo.ItemList[i].GetWay);
                        //string[] getwayInfo = mailInfo.ItemList[i].GetWay.Split('_');
                        //if (getwayInfo.Length >= 2 && mailInfo.ItemList[i].ItemID == 1 && int.Parse(getwayInfo[0]) == ItemGetWay.PaiMaiSell)
                        //{
                        //    unit.GetComponent<DataCollationComponent>().UpdateBuySelfPlayerList(mailInfo.ItemList[i].ItemNum, mailInfo.BuyPlayerId );
                        //}
                    }
                    else
                    {
                        bagComponentServer.OnAddItemData(mailInfo.ItemList[i], $"{ItemGetWay.ReceieMail}_{receiveMailTime}");
                    }
                }
                
                if (mailInfo.ItemSell != null)
                {
                    LDItem ldItem = LDItemCategory.Instance.Get(mailInfo.ItemSell.ItemID);
                    if (ldItem.ItemType == 3)
                    {
                        chengJiuComponentServer.TriggerEvent(ChengJiuTargetEnum.PaiMaiSellNumber_218, 0, 1);
                    }
                }
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
