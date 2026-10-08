using System;

namespace ET
{
    /// <summary>
    /// 点击邮件。走邮件服，不进地图服。过期则删掉这封。
    /// </summary>
    [ActorMessageHandler]
    public class C2Mail_ReadMailHandler : AMActorRpcHandler<Scene, C2Mail_ReadMailRequest, Mail2C_ReadMailResponse>
    {
        protected override async ETTask Run(Scene scene, C2Mail_ReadMailRequest request, Mail2C_ReadMailResponse response, Action reply)
        {
            int zone = scene.DomainZone();
            DBMailInfo dBMailInfo = await DBHelper.GetComponent<DBMailInfo>(zone, request.ActorId);
            if (dBMailInfo == null)
            {
                response.Error = ErrorCode.ERR_MailNotExist;
                reply();
                return;
            }

            MailInfo mailInfo = MailHelp.FindMail(dBMailInfo.MailInfoList, request.MailId);
            if (mailInfo == null)
            {
                response.Error = ErrorCode.ERR_MailNotExist;
                reply();
                return;
            }

            if (MailHelp.IsMailExpired(mailInfo, TimeHelper.ServerNow()))
            {
                dBMailInfo.MailInfoList.Remove(mailInfo);
                await DBHelper.SaveComponent(zone, request.ActorId, dBMailInfo);
                response.Error = ErrorCode.ERR_MailNotExist;
                reply();
                return;
            }

            if (!mailInfo.IsRead)
            {
                mailInfo.IsRead = true;
                await DBHelper.SaveComponent(zone, request.ActorId, dBMailInfo);
            }

            reply();
        }
    }
}
