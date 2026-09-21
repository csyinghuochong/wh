using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2T_TeamFollowHandler : AMActorRpcHandler<Scene, C2T_TeamFollowRequest, T2C_TeamFollowResponse>
    {
        protected override async ETTask Run(Scene scene, C2T_TeamFollowRequest request, T2C_TeamFollowResponse response, Action reply)
        {
            TeamSceneComponent teamSceneComponent = scene.GetComponent<TeamSceneComponent>();
            TeamInfo teamInfo = teamSceneComponent.GetTeamInfo(request.UserId);
            if (teamInfo == null)
            {
                reply();
                return;
            }

            if (teamInfo.TeamId == request.UserId)
            {
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            for (int i = 0; i < teamInfo.PlayerList.Count; i++)
            {
                if (teamInfo.PlayerList[i].UserID == request.UserId)
                {
                    teamInfo.PlayerList[i].Followe = 1;
                    break;
                }
            }

            teamSceneComponent.SyncTeamInfo(teamInfo, teamInfo.PlayerList).Coroutine();
            reply();
            await ETTask.CompletedTask;
        }
    }
}
