using System;

namespace ET
{
    [ActorMessageHandler]
    public class M2Mail_SendMailHandler: AMActorRpcHandler<Scene, M2Mail_SendMailRequest, Mail2M_SendMailResponse>
    {
        protected override async ETTask Run(Scene scene, M2Mail_SendMailRequest request, Mail2M_SendMailResponse response, Action reply)
        {
   
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.EMail, request.Id))
            {

                //存储邮件
                if (request.GetWay == ItemGetWay.RunRace)
                {
                    Log.Warning($"家族争霸赛邮件1: {request.Id}");
                }
                if (request.GetWay == ItemGetWay.MiJingBoss)
                {
                    Log.Warning($"世界BOSS邮件1: {request.Id}");
                }
                response.Error = await MailHelp.SendUserMail(scene.DomainZone(), request.Id, request.MailInfo);

                if (request.GetWay == ItemGetWay.RunRace)
                {
                    Log.Warning($"家族争霸赛邮件2: {response.Error}");
                }
                if (request.GetWay == ItemGetWay.MiJingBoss)
                {
                    Log.Warning($"世界BOSS邮件2: {response.Error}");
                }

                if (response.Error != ErrorCode.ERR_Success)
                {
                    response.Error = response.Error;
                    reply();
                    return;
                }

                long gateServerId = DBHelper.GetGateServerId(scene.DomainZone());
                G2T_GateUnitInfoResponse g2M_UpdateUnitResponse = (G2T_GateUnitInfoResponse)await ActorMessageSenderComponent.Instance.Call
                      (gateServerId, new T2G_GateUnitInfoRequest()
                      {
                          UserID = request.Id
                      });

                //在线直接推送
                if (g2M_UpdateUnitResponse.PlayerState == (int)PlayerState.Game && g2M_UpdateUnitResponse.SessionInstanceId > 0)
                {
                    M2C_UpdateMailInfo m2C_HorseNoticeInfo = new M2C_UpdateMailInfo();
                    MessageHelper.SendActor(g2M_UpdateUnitResponse.SessionInstanceId, m2C_HorseNoticeInfo);
                }
                if (g2M_UpdateUnitResponse.PlayerState == (int)PlayerState.None)
                {
                    long dbCacheId = DBHelper.GetDbCacheId(scene.DomainZone());
                    D2G_GetComponent d2GGetUnit = (D2G_GetComponent)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new G2D_GetComponent() { UnitId = request.Id, Component = DBHelper.ReddotComponent });
                    if (d2GGetUnit.Component != null)
                    {
                        ReddotComponentServer reddotComponentServer = d2GGetUnit.Component as ReddotComponentServer;
                        //reddotComponent.AddReddont((int)BelongReddot.Email);
                        D2M_SaveComponent d2M_SaveComponent = (D2M_SaveComponent)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new M2D_SaveComponent() { UnitId = request.Id, EntityByte = MongoHelper.ToBson(reddotComponentServer), ComponentType = DBHelper.ReddotComponent });
                    }
                }

                reply();
            }
        }

    }
}
