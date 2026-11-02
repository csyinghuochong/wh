using System;

namespace ET
{
    [ActorMessageHandler]
    public class M2Chat_UpdateRoleHandler : AMActorRpcHandler<Scene, M2Chat_UpdateRole, Chat2M_UpdateRole>
    {
        protected override async ETTask Run(Scene scene, M2Chat_UpdateRole request, Chat2M_UpdateRole response, Action reply)
        {
            ChatInfoUnit chatInfoUnit = scene.SceneType == SceneType.WZChat
                ? scene.GetComponent<WZChatSceneComponent>().Get(request.UnitId)
                : scene.GetComponent<ChatSceneComponent>().Get(request.UnitId);
            if (chatInfoUnit != null)
            {
                chatInfoUnit.Level = request.Level;
                chatInfoUnit.RoleHeadId = request.RoleHeadId;
                chatInfoUnit.RoleHeadFrame = request.RoleHeadFrame;
            }
            reply();
            await ETTask.CompletedTask;
        }
    }
}
