using System;

namespace ET
{
    [ActorMessageHandler]
    public class G2Chat_EnterChatHandler : AMActorRpcHandler<Scene, G2Chat_EnterChat, Chat2G_EnterChat>
    {
        protected override async ETTask Run(Scene scene, G2Chat_EnterChat request, Chat2G_EnterChat response, Action reply)
        {
            if (scene.SceneType == SceneType.WZChat)
            {
                WZChatSceneComponent wzChat = scene.GetComponent<WZChatSceneComponent>();
                ChatInfoUnit chatInfoUnit = wzChat.Enter(request);
                response.ChatInfoUnitInstanceId = chatInfoUnit.InstanceId;
                reply();
                await ETTask.CompletedTask;
                return;
            }

            ChatSceneComponent chatInfoUnitsComponent = scene.GetComponent<ChatSceneComponent>();
            ChatInfoUnit unit = chatInfoUnitsComponent.Get(request.UnitId);

            if (unit != null && !unit.IsDisposed)
            {
                unit.Name = request.Name;
                unit.Level = request.Level;
                unit.UnionId = request.UnionId;
                unit.GateSessionActorId = request.GateSessionActorId;
                response.ChatInfoUnitInstanceId = unit.InstanceId;
                reply();
                return;
            }

            ChatInfoUnit old = chatInfoUnitsComponent.GetChild<ChatInfoUnit>(request.UnitId);
            old?.Dispose();

            unit = chatInfoUnitsComponent.AddChildWithId<ChatInfoUnit>(request.UnitId);
            unit.AddComponent<MailBoxComponent>();
            unit.Name = request.Name;
            unit.Level = request.Level;
            unit.UnionId = request.UnionId;
            unit.GateSessionActorId = request.GateSessionActorId;
            response.ChatInfoUnitInstanceId = unit.InstanceId;
            chatInfoUnitsComponent.Add(unit);

            reply();
            await ETTask.CompletedTask;
        }
    }
}
