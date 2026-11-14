using System;

namespace ET
{
    [ActorMessageHandler]
    public class M2T_TeamDungeonAbandonHandler : AMActorRpcHandler<Scene, M2T_TeamDungeonAbandonRequest, T2M_TeamDungeonAbandonResponse>
    {
        protected override async ETTask Run(Scene scene, M2T_TeamDungeonAbandonRequest request, T2M_TeamDungeonAbandonResponse response, Action reply)
        {
            TeamSceneComponent teamScene = scene.GetComponent<TeamSceneComponent>();
            TeamInfo teamInfo = teamScene.GetTeamInfo(request.UserId);
            if (teamInfo == null)
            {
                response.Error = ErrorCode.Err_TeamIsNull;
                reply();
                return;
            }
            if (teamInfo.TeamId != request.UserId)
            {
                response.Error = ErrorCode.ERR_IsNotLeader;
                reply();
                return;
            }
            if (request.FubenInstanceId <= 0 || teamInfo.FubenState != (int)TeamDungeonState.Opened
                || teamInfo.FubenInstanceId != request.FubenInstanceId)
            {
                response.Error = ErrorCode.ERR_AlreadyFinish;
                reply();
                return;
            }

            Scene dungeonScene = teamScene.GetChild<Scene>(teamInfo.FubenUUId);
            if (dungeonScene == null || dungeonScene.InstanceId != request.FubenInstanceId)
            {
                response.Error = ErrorCode.ERR_AlreadyFinish;
                reply();
                return;
            }
            response.Error = await dungeonScene.GetComponent<TeamDungeonComponent>().Abandon();
            reply();
        }
    }
}
