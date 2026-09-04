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

                if (!await ServerMessageHelper.SendToClient(scene.DomainZone(), request.Id, new M2C_UpdateMailInfo()))
                {
                    int homeZone = UnitZoneHelper.GetHomeZone(request.Id);
                    ReddotComponentServer reddotComponentServer = await DBHelper.GetComponent<ReddotComponentServer>(homeZone, request.Id);
                    if (reddotComponentServer != null)
                    {
                        await DBHelper.SaveComponent(homeZone, request.Id, reddotComponentServer);
                    }
                }

                reply();
            }
        }

    }
}
