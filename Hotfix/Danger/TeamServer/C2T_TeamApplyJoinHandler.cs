using System;

namespace ET
{

    /// <summary>
    /// 申请加入队伍
    /// </summary>
    [ActorMessageHandler]
    public class C2T_TeamApplyJoinHandler : AMActorRpcHandler<Scene, C2T_TeamApplyJoinRequest, T2C_TeamApplyJoinResponse>
    {
        protected override async ETTask Run(Scene scene, C2T_TeamApplyJoinRequest request, T2C_TeamApplyJoinResponse response, Action reply)
        {
            TeamSceneComponent teamSceneComponent = scene.GetComponent<TeamSceneComponent>();
            if (teamSceneComponent.GetTeamInfo(request.TeamPlayerInfo.UserID) != null)
            {
                response.Error = ErrorCode.ERR_IsHaveTeam;
                reply();
                return;
            }

            TeamInfo teamInfo = teamSceneComponent.GetTeamInfo(request.TeamId);
            if (teamInfo == null || teamInfo.PlayerList.Count >= SceneConfigHelper.GetTeamMemberMax(teamInfo.SceneId))
            {
                response.Error = ErrorCode.ERR_TeamIsFull;
                reply();
                return;
            }

            if (!teamSceneComponent.AddApply(teamInfo.TeamId, request.TeamPlayerInfo))
            {
                response.Error = ErrorCode.ERR_TeamApplyLimit;
                reply();
                return;
            }
            M2C_TeamApplyJoinMessage m2C_TeamApplyJoin = new M2C_TeamApplyJoinMessage() { TeamPlayerInfo = request.TeamPlayerInfo };
            await ServerMessageHelper.SendToClient(UnitZoneHelper.GetHomeZone(teamInfo.TeamId), teamInfo.TeamId, m2C_TeamApplyJoin);

            reply();
        }
    }
}
