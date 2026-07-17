using System;

namespace ET
{
    [ObjectSystem]
    public class WZChatSceneComponentAwake : AwakeSystem<WZChatSceneComponent>
    {
        public override void Awake(WZChatSceneComponent self)
        {
            self.ChatInfoUnitsDict.Clear();
            self.WarChatInfos.Clear();
            Log.Console($"[WZChat] Awake zone={self.DomainZone()}");
        }
    }

    [ObjectSystem]
    public class WZChatSceneComponentDestroy : DestroySystem<WZChatSceneComponent>
    {
        public override void Destroy(WZChatSceneComponent self)
        {
            foreach (ChatInfoUnit chatInfoUnit in self.ChatInfoUnitsDict.Values)
            {
                chatInfoUnit?.Dispose();
            }
            self.ChatInfoUnitsDict.Clear();
            self.WarChatInfos.Clear();
        }
    }

    public static class WZChatSceneComponentSystem
    {
        public static void Add(this WZChatSceneComponent self, ChatInfoUnit chatInfoUnit)
        {
            if (self.ChatInfoUnitsDict.ContainsKey(chatInfoUnit.Id))
            {
                Log.Error($"[WZChat] chatInfoUnit exist: {chatInfoUnit.Id}");
                return;
            }
            self.ChatInfoUnitsDict.Add(chatInfoUnit.Id, chatInfoUnit);
        }

        public static ChatInfoUnit Get(this WZChatSceneComponent self, long id)
        {
            self.ChatInfoUnitsDict.TryGetValue(id, out ChatInfoUnit chatInfoUnit);
            return chatInfoUnit;
        }

        public static void Remove(this WZChatSceneComponent self, long id)
        {
            if (self.ChatInfoUnitsDict.TryGetValue(id, out ChatInfoUnit chatInfoUnit))
            {
                self.ChatInfoUnitsDict.Remove(id);
                chatInfoUnit?.Dispose();
            }
        }

        public static ChatInfoUnit Enter(this WZChatSceneComponent self, G2Chat_EnterChat request)
        {
            ChatInfoUnit chatInfoUnit = self.Get(request.UnitId);
            if (chatInfoUnit != null && !chatInfoUnit.IsDisposed)
            {
                chatInfoUnit.Name = request.Name;
                chatInfoUnit.Level = request.Level;
                chatInfoUnit.UnionId = request.UnionId;
                chatInfoUnit.GateSessionActorId = request.GateSessionActorId;
                return chatInfoUnit;
            }

            ChatInfoUnit old = self.GetChild<ChatInfoUnit>(request.UnitId);
            old?.Dispose();

            chatInfoUnit = self.AddChildWithId<ChatInfoUnit>(request.UnitId);
            chatInfoUnit.AddComponent<MailBoxComponent>();
            chatInfoUnit.Name = request.Name;
            chatInfoUnit.Level = request.Level;
            chatInfoUnit.UnionId = request.UnionId;
            chatInfoUnit.GateSessionActorId = request.GateSessionActorId;
            self.Add(chatInfoUnit);
            return chatInfoUnit;
        }

        public static void BroadcastWarChat(this WZChatSceneComponent self, ChatInfo chatInfo)
        {
            M2C_SyncChatInfo msg = new M2C_SyncChatInfo { ChatInfo = chatInfo };
            foreach (ChatInfoUnit otherUnit in self.ChatInfoUnitsDict.Values)
            {
                if (otherUnit.GateSessionActorId == 0)
                {
                    continue;
                }
                MessageHelper.SendActor(otherUnit.GateSessionActorId, msg);
            }

            self.WarChatInfos.Add(chatInfo);
            if (self.WarChatInfos.Count > 20)
            {
                self.WarChatInfos.RemoveAt(0);
            }
        }
    }
}
