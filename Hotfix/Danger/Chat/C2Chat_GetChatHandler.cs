using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2Chat_GetChatHandler : AMActorRpcHandler<ChatInfoUnit, C2Chat_GetChatRequest, Chat2C_GetChatResponse>
    {
        protected override async ETTask Run(ChatInfoUnit chatInfoUnit, C2Chat_GetChatRequest request, Chat2C_GetChatResponse response, Action reply)
        {
            Scene scene = chatInfoUnit.DomainScene();
            ChatSceneComponent chatScene = scene.GetComponent<ChatSceneComponent>();
            if (chatScene != null)
            {
                AppendRecent(chatScene.WordChatInfos, response.ChatInfos);
            }

            WZChatSceneComponent wzChat = scene.GetComponent<WZChatSceneComponent>();
            if (wzChat != null)
            {
                AppendRecent(wzChat.WarChatInfos, response.ChatInfos);
            }

            reply();
            await ETTask.CompletedTask;
        }

        /// <summary>最近 10 条，且不超过 10 天。</summary>
        private static void AppendRecent(List<ChatInfo> source, List<ChatInfo> target)
        {
            if (source == null || source.Count == 0)
            {
                return;
            }

            long minTime = TimeHelper.ServerNow() - 10 * TimeHelper.OneDay;
            int start = source.Count > 10 ? source.Count - 10 : 0;
            for (int i = start; i < source.Count; i++)
            {
                ChatInfo chatInfo = source[i];
                if (chatInfo.Time != 0 && chatInfo.Time < minTime)
                {
                    continue;
                }

                target.Add(chatInfo);
            }
        }
    }
}
