using System;

namespace ET
{
    [ActorMessageHandler]
    public class G2Chat_RequestExitChatHandler : AMActorRpcHandler<ChatInfoUnit, G2Chat_RequestExitChat, Chat2G_RequestExitChat>
    {
        protected override async ETTask Run(ChatInfoUnit unit, G2Chat_RequestExitChat request, Chat2G_RequestExitChat response, Action reply)
        {
            Scene scene = unit.DomainScene();
            if (scene.SceneType == SceneType.WZChat)
            {
                scene.GetComponent<WZChatSceneComponent>().Remove(unit.Id);
            }
            else
            {
                scene.GetComponent<ChatSceneComponent>().Remove(unit.Id);
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
