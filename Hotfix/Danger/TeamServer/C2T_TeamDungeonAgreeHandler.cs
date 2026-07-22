using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2T_TeamDungeonAgreeHandler : AMActorRpcHandler<Scene, C2T_TeamDungeonAgreeRequest, T2C_TeamDungeonAgreeResponse>
    {
        protected override async ETTask Run(Scene scene, C2T_TeamDungeonAgreeRequest request, T2C_TeamDungeonAgreeResponse response, Action reply)
        {
            TeamSceneComponent teamSceneComponent = scene.GetComponent<TeamSceneComponent>();
            if (teamSceneComponent.GetTeamInfo(request.TeamPlayerInfo.UserID) != null)
            {
                response.Error = ErrorCode.ERR_IsHaveTeam;
                reply();
                return;
            }

            if (!await PlayerOnlineHelper.IsInLocation(request.TeamPlayerInfo.UserID))
            {
                //对方已下线
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
