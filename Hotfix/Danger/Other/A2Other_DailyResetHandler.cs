using System;

namespace ET
{
    [ActorMessageHandler]
    public class A2Other_DailyResetHandler : AMActorRpcHandler<Scene, A2Other_DailyResetRequest, Other2A_DailyResetResponse>
    {
        protected override async ETTask Run(Scene scene, A2Other_DailyResetRequest request, Other2A_DailyResetResponse response, Action reply)
        {
            LogHelper.LogWarning($"DailyReset: {scene.SceneType} zone: {scene.DomainZone()} openday: {request.OpenDay}", true);
            switch (scene.SceneType)
            {
                case SceneType.Gate:
                    Player[] players = scene.GetComponent<PlayerComponent>().GetAll();
                    for (int i = 0; i < players.Length; i++)
                    {
                        if (players[i].PlayerState != PlayerState.Game)
                        {
                            continue;
                        }

                        ActorLocationSenderComponent.Instance.Send(players[i].UnitId, new G2M_DailyReset());
                    }
                    break;
                case SceneType.Rank:
                    scene.GetComponent<RankSceneComponent>().OnDailyReset();
                    break;
                case SceneType.Union:
                    scene.GetComponent<UnionSceneComponent>().OnDailyReset();
                    break;
                case SceneType.Consign:
                    scene.GetComponent<ConsignSceneComponent>().OnDailyReset();
                    break;
                case SceneType.FubenCenter:
                    FubenCenterComponent fubenCenter = scene.GetComponent<FubenCenterComponent>();
                    foreach (var item in fubenCenter.Children)
                    {
                        YeWaiRefreshComponent yeWaiRefresh = item.Value.GetComponent<YeWaiRefreshComponent>();
                        if (yeWaiRefresh == null)
                        {
                            continue;
                        }

                        yeWaiRefresh.OnDailyReset(request.OpenDay);
                    }
                    break;
                default:
                    break;
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
