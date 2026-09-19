using System;

namespace ET
{
    /// <summary>
    /// 队长回复入队申请
    /// </summary>
    [ActorMessageHandler]
    public class C2T_TeamApplyReplyHandler : AMActorRpcHandler<Scene, C2T_TeamApplyReplyRequest, T2C_TeamApplyReplyResponse>
    {
        protected override async ETTask Run(Scene scene, C2T_TeamApplyReplyRequest request, T2C_TeamApplyReplyResponse response, Action reply)
        {
            TeamSceneComponent teamSceneComponent = scene.GetComponent<TeamSceneComponent>();
            if (teamSceneComponent.GetTeamInfo(request.TeamPlayerInfo.UserID) != null)
            {
                response.Error = ErrorCode.ERR_IsHaveTeam;
                reply();
                return;
            }

            if (!await ServerMessageHelper.IsInGame(scene.DomainZone(), request.TeamPlayerInfo.UserID))
            {
                reply();
                return;
            }

            TeamInfo teamInfo = teamSceneComponent.GetTeamInfo(request.TeamId);
            if (teamInfo == null || teamInfo.PlayerList.Count == 3)
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
            teamSceneComponent.SyncTeamInfo(teamInfo,teamInfo.PlayerList).Coroutine();
            reply();
        }
    }
}
