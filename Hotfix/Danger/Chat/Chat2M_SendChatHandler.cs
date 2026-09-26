using System;

namespace ET
{
    [ActorMessageHandler]
    public class Chat2M_SendChatHandler : AMActorLocationHandler<Unit, Chat2M_SendChat>
    {
        protected override async ETTask Run(Unit unit, Chat2M_SendChat message)
        {
            unit.GetComponent<TaskComponentServer>()?.OnSendChat(message.ChannelId);
            await ETTask.CompletedTask;
        }
    }
}
