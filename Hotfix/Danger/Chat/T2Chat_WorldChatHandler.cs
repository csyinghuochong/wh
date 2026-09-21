using System;

namespace ET
{
    [ActorMessageHandler]
    public class T2Chat_WorldChatHandler : AMActorHandler<Scene, T2Chat_WorldChatMessage>
    {
        protected override async ETTask Run(Scene scene, T2Chat_WorldChatMessage message)
        {
            if (message.ChatInfo == null)
            {
                return;
            }

            ChatSceneComponent chatScene = scene.GetComponent<ChatSceneComponent>();
            if (chatScene == null)
            {
                return;
            }

            if (message.ChatInfo.Time == 0)
            {
                message.ChatInfo.Time = TimeHelper.ServerNow();
            }

            M2C_SyncChatInfo m2C_SyncChatInfo = new M2C_SyncChatInfo()
            {
                ChatInfo = message.ChatInfo
            };
            foreach (var otherUnit in chatScene.ChatInfoUnitsDict.Values)
            {
                MessageHelper.SendActor(otherUnit.GateSessionActorId, m2C_SyncChatInfo);
            }

            chatScene.WordChatInfos.Add(message.ChatInfo);
            if (chatScene.WordChatInfos.Count > 10)
            {
                chatScene.WordChatInfos.RemoveAt(chatScene.WordChatInfos.Count - 1);
            }

            await ETTask.CompletedTask;
        }
    }
}
