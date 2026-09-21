using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2T_TeamFollowAllHandler : AMActorRpcHandler<Scene, C2T_TeamFollowAllRequest, T2C_TeamFollowAllResponse>
    {
        protected override async ETTask Run(Scene scene, C2T_TeamFollowAllRequest request, T2C_TeamFollowAllResponse response, Action reply)
        {
            TeamSceneComponent teamSceneComponent = scene.GetComponent<TeamSceneComponent>();
            TeamInfo teamInfo = teamSceneComponent.GetTeamInfo(request.UserId);
            if (teamInfo == null || teamInfo.TeamId != request.UserId)
            {
                response.Error = ErrorCode.ERR_IsNotLeader;
                reply();
                return;
            }

            TeamPlayerInfo leaderInfo = null;
            for (int i = 0; i < teamInfo.PlayerList.Count; i++)
            {
                if (teamInfo.PlayerList[i].UserID == teamInfo.TeamId)
                {
                    leaderInfo = teamInfo.PlayerList[i];
                    break;
                }
            }

            int zone = scene.DomainZone();
            for (int i = 0; i < teamInfo.PlayerList.Count; i++)
            {
                TeamPlayerInfo playerInfo = teamInfo.PlayerList[i];
                if (playerInfo.UserID == teamInfo.TeamId)
                {
                    continue;
                }

                await ServerMessageHelper.SendToClient(zone, playerInfo.UserID,
                    new M2C_TeamFollowResult() { TeamPlayerInfo = leaderInfo });
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
