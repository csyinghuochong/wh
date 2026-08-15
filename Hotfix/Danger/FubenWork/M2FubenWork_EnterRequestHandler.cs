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
                    fubnescene.GetComponent<MapComponent>().NavMeshId = LDSceneCategory.Instance.Get(request.SceneId).GetNavMeshId();
                    
                    response.FubenInstanceId = fubenInstanceId;
                    TransferHelper.NoticeFubenCenter(fubnescene, 1).Coroutine();
                    break;
                case MapTypeEnum.Battle:
                    if (!LDSceneCategory.Instance.Contain(request.SceneId))
                    {
                        response.Error = ErrorCode.ERR_NotFindLevel;
                        break;
                    }
                    LDScene battleSceneConfig = LDSceneCategory.Instance.Get(request.SceneId);
                    long battleFubenId = IdGenerater.Instance.GenerateId();
                    long battleFubenInstanceId = IdGenerater.Instance.GenerateInstanceId();
                    Scene battleScene = SceneFactory.Create(Game.Scene, battleFubenId, battleFubenInstanceId, scene.DomainZone(), "Battle" + battleFubenId.ToString(), SceneType.Map);
                    BattleDungeonComponent battleDungeon = battleScene.AddComponent<BattleDungeonComponent>();
                    battleDungeon.BattleOpenTime = TimeHelper.ServerNow();
                    MapComponent battleMap = battleScene.GetComponent<MapComponent>();
                    battleMap.SetMapInfo(MapTypeEnum.Battle, request.SceneId, 0);
                    battleMap.NavMeshId = battleSceneConfig.GetNavMeshId();
                    Game.Scene.GetComponent<RecastPathComponent>().Update(battleMap.NavMeshId);
                    response.FubenId = battleFubenId;
                    response.FubenInstanceId = battleFubenInstanceId;
                    TransferHelper.NoticeFubenCenter(battleScene, 1).Coroutine();
                    break;
               default:
                    break;
            }


            reply();
            await ETTask.CompletedTask;
        }
    }
}
