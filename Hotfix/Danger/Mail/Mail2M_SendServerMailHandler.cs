using System;

namespace ET
{

    /// <summary>
    /// 在线全服邮件通知。实际投递与登录补发共用入口，保存成功后才推进角色进度。
    /// </summary>
    [ActorMessageHandler]
    public class Mail2M_SendServerMailHandler : AMActorLocationHandler<Unit, Mail2M_SendServerMailItem>
    {

        protected override async ETTask Run(Unit unit, Mail2M_SendServerMailItem message)
        {
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            if (message.ServerMailIdMax <= roleInfoComponentServer.RoleInfo.ServerMailIdCur)
            {
                return;
            }

            await MailHelp.SyncServerMails(unit);
        }
    }
}
