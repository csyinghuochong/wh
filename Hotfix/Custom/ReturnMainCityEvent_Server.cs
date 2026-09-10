namespace ET
{
    public class ReturnMainCityEvent_Server : AEvent<EventType.ReturnMainCity>
    {
        protected override void Run(EventType.ReturnMainCity args)
        {
            Scene scene = args.DomainScene;
            long userId = args.UnitId;

            if (scene.IsDisposed)
            {
                Log.Warning($"ReturnMainCity: scene.IsDisposed");
                return;
            }
            int sceneTypeEnum = scene.GetComponent<MapComponent>().MapTypeEnum;
            if (SceneConfigHelper.IsSingleFuben(sceneTypeEnum))
            {
                TransferHelper.NoticeFubenCenter(scene, 2).Coroutine();
                scene.Dispose();
            }
            if (sceneTypeEnum == MapTypeEnum.TeamDungeon)
            {
                TeamSceneComponent teamSceneComponent = scene.GetParent<TeamSceneComponent>();
                teamSceneComponent.OnUnitReturn(scene, userId);
            }
            if (sceneTypeEnum == (int)MapTypeEnum.Arena)
            {
                ArenaDungeonComponent areneSceneComponent = scene.GetComponent<ArenaDungeonComponent>();
                areneSceneComponent.OnUnitDisconnect(userId);
            }
            if (sceneTypeEnum == MapTypeEnum.Home)
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
