using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    internal class M2E_GMEMailSendHandler : AMActorRpcHandler<Scene, M2E_GMEMailSendRequest, E2M_GMEMailSendResponse>
    {
        protected override async ETTask Run(Scene scene, M2E_GMEMailSendRequest request, E2M_GMEMailSendResponse response, Action reply)
        {
            Log.Warning($"M2E_GMEMailSendRequest:{request.UserName}");
            if (request.UserName == "0")
            {
                //dBMailInfos = await Game.Scene.GetComponent<DBComponent>().Query<DBMailInfo>(scene.DomainZone(), d => d.Id > 0);
                EventType.ServerMail serverMail = new EventType.ServerMail()
                {
                    Message = request,
                    MailScene = scene,
                };
                Game.EventSystem.Publish(serverMail);
                reply();
                return;
            }

            List<DBMailInfo> dBMailInfos = null;
            List<RoleInfoComponentServer> accountInfoList = await Game.Scene.GetComponent<DBComponent>().Query<RoleInfoComponentServer>(scene.DomainZone(), d => d.RoleInfo.Name == request.UserName);
            if (accountInfoList.Count > 0)
            {
                dBMailInfos = await Game.Scene.GetComponent<DBComponent>().Query<DBMailInfo>(scene.DomainZone(), d => d.Id == accountInfoList[0].Id);
            }
            if (dBMailInfos != null)
            {
                long serverTime = TimeHelper.ServerNow();
                DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();
                string[] needList = request.Itemlist.Split('@');
                for (int i = 0; i < dBMailInfos.Count; i++)
                {
                    long mailOwnerId = dBMailInfos[i].Id;
                    List<RechargeComponentServer> rechargeInfoList = await dbComponent.Query<RechargeComponentServer>(scene.DomainZone(), d => d.Id == mailOwnerId);
                    long rechargeNum = rechargeInfoList.Count > 0 ? rechargeInfoList[0].GetTotalRechargeNum() : 0;
                    List<RoleInfoComponentServer> RoleInfoComponents = await dbComponent.Query<RoleInfoComponentServer>(scene.DomainZone(), d => d.Id == mailOwnerId);
                    if (RoleInfoComponents.Count == 0)
                    {
                        continue;
                    }
                    RoleInfoComponentServer roleInfoComponent = RoleInfoComponents[0];
                    if (roleInfoComponent.RoleInfo.RobotId > 0)
                    {
                        continue;
                    }

                    List<BagComponentServer> bagInfoList = await dbComponent.Query<BagComponentServer>(scene.DomainZone(), d => d.Id == mailOwnerId);
                    if (bagInfoList.Count == 0)
                    {
                        continue;
                    }

                    bool cansendMail = MailHelp.CheckSendMail(request.MailType, request.Title, rechargeNum, roleInfoComponent, bagInfoList[0]);
                    if (cansendMail == false)
                    {
                        continue;
                    }

                    MailInfo mailInfo = new MailInfo();
                    mailInfo.Status = 0;
                    //mailInfo.Context = "福利发放";
                    //mailInfo.Title = "福利发放";
                    mailInfo.MailId = IdGenerater.Instance.GenerateId();
                    for (int k = 0; k < needList.Length; k++)
                    {
                        string[] itemInfo = needList[k].Split(';');
                        if (itemInfo.Length < 2)
                        {
                            continue;
                        }
                        int itemId = int.Parse(itemInfo[0]);
                        int itemNum = int.Parse(itemInfo[1]);
                        mailInfo.ItemList.Add(new BagInfo() { ItemID = itemId, ItemNum = itemNum, GetWay = $"{ItemGetWay.ReceieMail}_{serverTime}" });
                    }

                    await MailHelp.SendUserMail((int)request.ActorId, mailOwnerId, mailInfo);
                }
            }
            else 
            {
                response.Message = $"找不到:{request.UserName}";
                response.Error = ErrorCode.ERR_NotFindAccount;
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}