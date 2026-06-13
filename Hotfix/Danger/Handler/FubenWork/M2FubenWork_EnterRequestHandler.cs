using System;

namespace ET
{

    
    /// <summary>
    /// 真正创建副本的服务，  1  直接从玩家当前scene发消息过来创建     2  走fubencenter 统一调度
    /// </summary>
    [ActorMessageHandler]
    public class M2FubenWork_EnterRequestHandler : AMActorRpcHandler<Scene, M2FubenWork_EnterRequest, FubenWork2M_EnterResponse>
    {
        protected override async ETTask Run(Scene scene, M2FubenWork_EnterRequest request, FubenWork2M_EnterResponse response, Action reply)
        {
            switch (request.SceneType)
            {
                case MapTypeEnum.LocalDungeon:
                    long fubenid = IdGenerater.Instance.GenerateId();
                    long fubenInstanceId = IdGenerater.Instance.GenerateInstanceId();
                    Scene fubnescene = SceneFactory.Create(Game.Scene, fubenid, fubenInstanceId, scene.DomainZone(), "LocalDungeon" + fubenid.ToString(), SceneType.Map);
                    fubnescene.AddComponent<YeWaiRefreshComponent>();
                    LocalDungeonComponent localDungeon = fubnescene.AddComponent<LocalDungeonComponent>();
                    localDungeon.FubenDifficulty = request.Difficulty;
                    fubnescene.GetComponent<MapComponent>().SetMapInfo((int)MapTypeEnum.LocalDungeon, request.SceneId, 0);
                    response.FubenInstanceId = fubenInstanceId;
                    TransferHelper.NoticeFubenCenter(fubnescene, 1).Coroutine();
                    break;
                case MapTypeEnum.Battle:
                    //动态创建副本
                    int sceneId = request.SceneId;  
                    fubenid = IdGenerater.Instance.GenerateId();
                    fubenInstanceId = IdGenerater.Instance.GenerateInstanceId();
                    fubnescene = SceneFactory.Create(Game.Scene, fubenid, fubenInstanceId, scene.DomainZone(), "Battle" + fubenid.ToString(), SceneType.Map);
                    //Console.WriteLine($"M2LocalDungeon_Enter: {fubnescene.Name}   {scene.DomainZone()}");
                    fubnescene.AddComponent<BattleDungeonComponent>().SendReward = false;
                    fubnescene.GetComponent<BattleDungeonComponent>().BattleOpenTime = TimeHelper.ServerNow();
                    MapComponent mapComponent = fubnescene.GetComponent<MapComponent>();
                    mapComponent.SetMapInfo((int)MapTypeEnum.Battle, sceneId, 0);
                    mapComponent.NavMeshId = LDSceneCategory.Instance.Get(sceneId).Id;
                    Game.Scene.GetComponent<RecastPathComponent>().Update(mapComponent.NavMeshId);
                    fubnescene.AddComponent<YeWaiRefreshComponent>().SceneId = sceneId;
                    FubenHelp.CreateNpc(fubnescene, sceneId);
                    
                    //FubenHelp.CreateMonsterList(fubnescene, LDSceneCategory.Instance.Get(sceneId).CreateMonsterPosi);
                    response.FubenId = fubenid;
                    response.FubenInstanceId = fubenInstanceId;
                    TransferHelper.NoticeFubenCenter(fubnescene, 1).Coroutine();
                    break;
            }


            reply();
            await ETTask.CompletedTask;
        }
    }
}
