using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2Chat_MarqueeHandler : AMActorRpcHandler<ChatInfoUnit, C2Chat_MarqueeRequest, Chat2C_MarqueeResponse>
    {
        protected override async ETTask Run(ChatInfoUnit chatInfoUnit, C2Chat_MarqueeRequest request, Chat2C_MarqueeResponse response, Action reply)
        {
            ChatSceneComponent chatScene = chatInfoUnit.DomainScene().GetComponent<ChatSceneComponent>();
            if (chatScene == null)
            {
                reply();
                return;
            }

            RoleInfoComponentServer roleInfo = await DBHelper.GetPlayerComponent<RoleInfoComponentServer>(chatInfoUnit.Id);
            long lastOfflineTime = roleInfo != null ? roleInfo.LastOfflineTime : 0;
            response.MarqueeInfoList.AddRange(chatScene.GetImportantMarqueeList(lastOfflineTime));
            reply();
        }
    }
}
