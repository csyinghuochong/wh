namespace ET
{
    public class PlayerDisconnectEvent_Server : AEvent<EventType.PlayerDisconnect>
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
            if (sceneTypeEnum == (int)MapTypeEnum.Home)
            {
                HomeSceneComponent homeSceneComponent = scene.GetParent<HomeSceneComponent>();
                homeSceneComponent.OnUnitLeave(scene);
            }
            if (sceneTypeEnum == (int)MapTypeEnum.OneChallenge)
            {
                OneChallengeDungeonComponent oneChallengeSceneComponent = scene.GetParent<OneChallengeDungeonComponent>();
                oneChallengeSceneComponent.OnUnitLeave(scene);
            }
        }

    }
}
