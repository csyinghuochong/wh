using System;

namespace ET
{


    [ActorMessageHandler]
    public class C2Mail_GetAllMailHandler : AMActorRpcHandler<Scene, C2Mail_GetAllMailRequest, Mail2C_GetAllMailResponse>
    {
        protected override async ETTask Run(Scene scene, C2Mail_GetAllMailRequest request, Mail2C_GetAllMailResponse response, Action reply)
        {
      
            DBMailInfo dBMailInfo = await DBHelper.GetComponent<DBMailInfo>(scene.DomainZone(), request.ActorId);
            if (dBMailInfo != null)
            {
                bool changed = MailHelp.RemoveExpiredMails(dBMailInfo.MailInfoList, TimeHelper.ServerNow());
                if (changed)
                {
                    await DBHelper.SaveComponent(scene.DomainZone(), request.ActorId, dBMailInfo);
                }

                response.MailInfos = dBMailInfo.MailInfoList;
            }
            reply();
        }

    }



}
