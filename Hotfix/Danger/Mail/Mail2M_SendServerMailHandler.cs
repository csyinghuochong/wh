using System;

namespace ET
{

    /// <summary>
    /// 这个是用来处理全服邮件的 暂时不要乱用
    /// </summary>
    [ActorMessageHandler]
    public class Mail2M_SendServerMailHandler : AMActorLocationHandler<Unit, Mail2M_SendServerMailItem>
    {

        protected override async ETTask Run(Unit unit, Mail2M_SendServerMailItem message)
        {
            //Log.Console($"asdsadada : 全服邮件{message.ServerMailItem.ServerMailIId}");
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            if (message.ServerMailItem.ServerMailIId > roleInfoComponentServer.RoleInfo.ServerMailIdCur)
            {
                roleInfoComponentServer.RoleInfo.ServerMailIdCur = message.ServerMailItem.ServerMailIId;
            }
            await ETTask.CompletedTask;
        }
    }
}
