using System;

namespace ET
{
    /// <summary>
    /// 创建队伍
    /// </summary>
    [ActorMessageHandler]
    public class C2T_TeamCreateHandler : AMActorRpcHandler<Scene, C2T_TeamCreateRequest, T2C_TeamCreateResponse>
    {
        protected override async ETTask Run(Scene scene, C2T_TeamCreateRequest request, T2C_TeamCreateResponse response, Action reply)
        {
            TeamSceneComponent teamSceneComponent = scene.GetComponent<TeamSceneComponent>();
            if (request.TeamPlayerInfo == null)
            {
                reply();
                return;
            }

            if (teamSceneComponent.GetTeamInfo(request.TeamPlayerInfo.UserID) != null)
            {
                response.Error = ErrorCode.ERR_IsHaveTeam;
                reply();
                return;
            }

            TeamInfo teamInfo = teamSceneComponent.CreateTeamInfo(request.TeamPlayerInfo, 0);
            teamSceneComponent.SyncTeamInfo(teamInfo, teamInfo.PlayerList).Coroutine();
            reply();
            await ETTask.CompletedTask;
        }
    }
}
