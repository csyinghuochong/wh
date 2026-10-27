using System;

namespace ET
{
    [ActorMessageHandler]
    public class M2Chat_UpdateChatShowHandler : AMActorRpcHandler<Scene, M2Chat_UpdateChatShow, Chat2M_UpdateChatShow>
    {
        protected override async ETTask Run(Scene scene, M2Chat_UpdateChatShow request, Chat2M_UpdateChatShow response, Action reply)
        {
            ChatInfoUnit chatInfoUnit = scene.SceneType == SceneType.WZChat
                ? scene.GetComponent<WZChatSceneComponent>().Get(request.UnitId)
                : scene.GetComponent<ChatSceneComponent>().Get(request.UnitId);
            if (chatInfoUnit != null)
            {
                chatInfoUnit.VipLevel = request.VipLevel;
                chatInfoUnit.TitleId = request.TitleId;
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
