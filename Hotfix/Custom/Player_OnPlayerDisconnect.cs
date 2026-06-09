namespace ET
{
    public class Player_OnPlayerDisconnect : AEvent<EventType.PlayerDisconnect>
    {

        protected override void Run(EventType.PlayerDisconnect args)
        {
            Scene scene = args.DomainScene;
            long userId = args.UnitId;
            int sceneTypeEnum = args.DomainScene.GetComponent<MapComponent>().MapTypeEnum;

            if (SceneConfigHelper.IsSingleFuben(sceneTypeEnum))
            {
                //动态删除副本
                TransferHelper.NoticeFubenCenter(scene, 2).Coroutine();
                scene.Dispose();
            }
            if (sceneTypeEnum == (int)MapTypeEnum.TeamDungeon)
            {
                TeamSceneComponent teamSceneComponent = scene.GetParent<TeamSceneComponent>();
                teamSceneComponent.OnUnitDisconnect(scene, userId);
            }
            if (sceneTypeEnum == (int)MapTypeEnum.Arena)
            {
                ArenaDungeonComponent areneSceneComponent = scene.GetComponent<ArenaDungeonComponent>();
                areneSceneComponent.OnUnitDisconnect(userId);
            }
            if (sceneTypeEnum == (int)MapTypeEnum.JiaYuan)
            {
                JiaYuanSceneComponent jiayuanSceneComponent = scene.GetParent<JiaYuanSceneComponent>();
                jiayuanSceneComponent.OnUnitLeave(scene);
            }
            if (sceneTypeEnum == (int)MapTypeEnum.OneChallenge)
            {
                OneChallengeDungeonComponent jiayuanSceneComponent = scene.GetParent<OneChallengeDungeonComponent>();
                jiayuanSceneComponent.OnUnitLeave(scene);
            }
        }

    }
}
