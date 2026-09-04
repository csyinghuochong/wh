using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2T_TeamDungeonApplyHandler : AMActorRpcHandler<Scene, C2T_TeamDungeonApplyRequest, T2C_TeamDungeonApplyResponse>
    {
        protected override async ETTask Run(Scene scene, C2T_TeamDungeonApplyRequest request, T2C_TeamDungeonApplyResponse response, Action reply)
        {
            TeamSceneComponent teamSceneComponent = scene.GetComponent<TeamSceneComponent>();
            if (teamSceneComponent.GetTeamInfo(request.TeamPlayerInfo.UserID) != null)
            {
                response.Error = ErrorCode.ERR_IsHaveTeam;
                reply();
                return;
            }

            TeamInfo teamInfo = teamSceneComponent.GetTeamInfo(request.TeamId);
            if (teamInfo == null || teamInfo.PlayerList.Count == 3)
            {
                response.Error = ErrorCode.ERR_TeamIsFull;
                reply();
                return;
            }

            //需要判断次数就添加C2M
            M2C_TeamDungeonApplyResult m2C_HorseNoticeInfo = new M2C_TeamDungeonApplyResult() { TeamPlayerInfo = request.TeamPlayerInfo };
            await ServerMessageHelper.SendToClient(scene.DomainZone(), teamInfo.TeamId, m2C_HorseNoticeInfo);

            reply();
        }
    }
}
