using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2Chat_GetChatHandler : AMActorRpcHandler<ChatInfoUnit, C2Chat_GetChatRequest, Chat2C_GetChatResponse>
    {
        protected override async ETTask Run(ChatInfoUnit chatInfoUnit, C2Chat_GetChatRequest request, Chat2C_GetChatResponse response, Action reply)
        {
            long serverTime = TimeHelper.ServerNow();
            ChatSceneComponent chatInfoUnitsComponent = chatInfoUnit.DomainScene().GetComponent<ChatSceneComponent>();
            long cutoff = serverTime - TimeHelper.OneDay;
            List<ChatInfo> wordChats = chatInfoUnitsComponent.WordChatInfos;
            for (int i = 0; i < wordChats.Count; i++)
            {
                ChatInfo chatInfo = wordChats[i];
                if (chatInfo.Time >= cutoff)
                {
                    response.ChatInfos.Add(chatInfo);
                }
            }
            

            reply();
            await ETTask.CompletedTask;
        }
    }
}
