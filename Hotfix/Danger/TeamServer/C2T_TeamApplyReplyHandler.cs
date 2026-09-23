using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2T_TeamApplyReplyHandler : AMActorRpcHandler<Scene, C2T_TeamApplyReplyRequest, T2C_TeamApplyReplyResponse>
    {
        protected override async ETTask Run(Scene scene, C2T_TeamApplyReplyRequest request, T2C_TeamApplyReplyResponse response, Action reply)
        {
            if (request.TeamPlayerInfo == null)
            {
                reply();
                return;
            }

            TeamSceneComponent teamSceneComponent = scene.GetComponent<TeamSceneComponent>();
            teamSceneComponent.RemoveApply(request.TeamId, request.TeamPlayerInfo.UserID);
            if (request.ReplyCode != 1)
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

            if (!await ServerMessageHelper.IsInGame(scene.DomainZone(), request.TeamPlayerInfo.UserID))
            {
                response.Error = ErrorCode.ERR_PlayerNotOnline;
                reply();
                return;
            }

            TeamInfo teamInfo = teamSceneComponent.GetTeamInfo(request.TeamId);
            if (teamInfo == null || teamInfo.TeamId != request.TeamId || teamInfo.PlayerList.Count == 3)
            {
                reply();
                return;
            }

            bool haveplayer = false;
            for (int i = 0; i < teamInfo.PlayerList.Count; i++)
            {
                if (teamInfo.PlayerList[i].UserID == request.TeamPlayerInfo.UserID)
                {
                    haveplayer = true;
                    break;
                }
            }
            if (!haveplayer)
            {
                teamInfo.PlayerList.Add(request.TeamPlayerInfo);
            }

            teamSceneComponent.SyncTeamInfo(teamInfo, teamInfo.PlayerList).Coroutine();
            reply();
        }
    }
}
