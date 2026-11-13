using System;

namespace ET
{

    /// <summary>
    /// 队友进入组队副本
    /// </summary>
    [ActorMessageHandler]
    public class M2T_TeamDungeonEnterHandler : AMActorRpcHandler<Scene, M2T_TeamDungeonEnterRequest, T2M_TeamDungeonEnterResponse>
    {
        protected override async ETTask Run(Scene scene, M2T_TeamDungeonEnterRequest request, T2M_TeamDungeonEnterResponse response, Action reply)
        {
            TeamSceneComponent teamScene = scene.GetComponent<TeamSceneComponent>();
            TeamInfo teamInfo = teamScene.GetTeamInfo(request.UserID);
            if (teamInfo == null)
            {
                response.Error = ErrorCode.ERR_TransferFailError;
                reply();
                return;
            }
            // 申请流程里已经创建副本。这里不创建，没实例就不能进。
            if (teamInfo.FubenInstanceId == 0)
            {
                response.Error = ErrorCode.ERR_TransferFailError;
                reply();
                return;
            }

            Scene dungeonScene = teamScene.GetChild<Scene>(teamInfo.FubenUUId);
            TeamDungeonComponent dungeon = dungeonScene?.GetComponent<TeamDungeonComponent>();
            if (dungeon == null || dungeon.IsAbandoning || dungeonScene.InstanceId != teamInfo.FubenInstanceId)
            {
                response.Error = ErrorCode.ERR_TransferFailError;
                reply();
                return;
            }

            response.HasEntered = dungeon.EnterPlayers.Contains(request.UserID);
            response.FubenId = teamInfo.SceneId;
            response.FubenType = teamInfo.FubenType;
            response.FubenInstanceId = teamInfo.FubenInstanceId;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
