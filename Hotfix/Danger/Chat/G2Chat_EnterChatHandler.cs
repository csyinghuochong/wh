using System;


namespace ET
{
    [ActorMessageHandler]
    public class G2Chat_EnterChatHandler : AMActorRpcHandler<Scene, G2Chat_EnterChat, Chat2G_EnterChat>
    {
        protected override async ETTask Run(Scene scene, G2Chat_EnterChat request, Chat2G_EnterChat response, Action reply)
        {
            //// 战区 Chat 进线时写一条测试数据，用于验证 game_wz200x 落库（测完可删）
            //if (StartZoneConfigCategory.Instance.IsWarShareZone(scene.DomainZone()))
            //{
            //    int warZone = scene.DomainZone();
            //    DBServerInfo testInfo = await DBHelper.GetComponent<DBServerInfo>(warZone, warZone);
            //    if (testInfo == null)
            //    {
            //        testInfo = new DBServerInfo();
            //        testInfo.Id = warZone;
            //    }
            //    await DBHelper.SaveComponent(warZone, testInfo.Id, testInfo);
            //    Log.Console($"[WarZoneDBTest] EnterWarChat zone={warZone} unitId={request.UnitId} name={request.Name} saved DBServerInfo → DBName={StartZoneConfigCategory.Instance.Get(warZone).DBName}");
            //}

            ChatSceneComponent chatInfoUnitsComponent = scene.GetComponent<ChatSceneComponent>();
            ChatInfoUnit chatInfoUnit = chatInfoUnitsComponent.Get(request.UnitId);

            if (chatInfoUnit != null && !chatInfoUnit.IsDisposed)
            {
                chatInfoUnit.Name = request.Name;
                chatInfoUnit.Level = request.Level; 
                chatInfoUnit.UnionId = request.UnionId;
                chatInfoUnit.GateSessionActorId = request.GateSessionActorId;
                response.ChatInfoUnitInstanceId = chatInfoUnit.InstanceId;
                reply();
                return;
            }

            ChatInfoUnit chatInfoUnit1 = chatInfoUnitsComponent.GetChild<ChatInfoUnit>(request.UnitId);
            chatInfoUnit1?.Dispose();

            chatInfoUnit = chatInfoUnitsComponent.AddChildWithId<ChatInfoUnit>(request.UnitId);
            chatInfoUnit.AddComponent<MailBoxComponent>();

            chatInfoUnit.Name = request.Name;
            chatInfoUnit.Level = request.Level;
            chatInfoUnit.UnionId = request.UnionId;
            chatInfoUnit.GateSessionActorId = request.GateSessionActorId;
            response.ChatInfoUnitInstanceId = chatInfoUnit.InstanceId;
            chatInfoUnitsComponent.Add(chatInfoUnit);

            reply();
            await ETTask.CompletedTask;
        }
    }
}
