using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class M2Mail_ReceiveMailandler : AMActorRpcHandler<Scene, M2Mail_ReceiveMailRequest, Mail2M_ReceiveMailResponse>
    {

        protected override async ETTask Run(Scene scene, M2Mail_ReceiveMailRequest request, Mail2M_ReceiveMailResponse response, Action reply)
        {
            long dbCacheId = DBHelper.GetDbCacheId(scene.DomainZone());
           
            DBMailInfo dBMailInfo = await DBHelper.GetComponent<DBMailInfo>(scene.DomainZone(),  request.Id);
            for (int i = dBMailInfo.MailInfoList.Count - 1; i >= 0; i--)
            {
                if (dBMailInfo.MailInfoList[i].MailId == request.MailId)
                {
                    MailInfo mailInfo = dBMailInfo.MailInfoList[i];
                    dBMailInfo.MailInfoList.RemoveAt(i);
                    response.MailInfo = mailInfo;
                    break;
                }
            }
            await DBHelper.SaveComponent(scene.DomainZone(), request.Id,dBMailInfo);
            reply();
        }
    }
}
