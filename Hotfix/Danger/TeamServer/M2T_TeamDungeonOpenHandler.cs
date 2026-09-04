using System;

namespace ET
{

    [ActorMessageHandler]
    public class M2T_TeamDungeonOpenHandler : AMActorRpcHandler<Scene, M2T_TeamDungeonOpenRequest, T2M_TeamDungeonOpenResponse>
    {
        protected override async ETTask Run(Scene scene, M2T_TeamDungeonOpenRequest request, T2M_TeamDungeonOpenResponse response, Action reply)
        {
            TeamInfo teamInfo = scene.GetComponent<TeamSceneComponent>().GetTeamInfo(request.UserID);
            if (teamInfo == null)
            {
                Log.Debug($"M2T_TeamDungeonOpen: teamInfo == null");
                response.Error = ErrorCode.ERR_TeamIsFull;
                reply();
                return;
            }
            for (int i = 0; i < teamInfo.PlayerList.Count; i++)
            {
                teamInfo.PlayerList[i].Prepare = teamInfo.PlayerList[i].UserID == teamInfo.TeamId ? 1 : 0;
            }

            teamInfo.FubenType = request.FubenType;
            M2C_TeamDungeonOpenResult m2C_HorseNoticeInfo = new M2C_TeamDungeonOpenResult() { TeamInfo = teamInfo  };
            int zone = scene.DomainZone();
            for (int i = 0; i < teamInfo.PlayerList.Count; i++)
            {
                await ServerMessageHelper.SendToClient(zone, teamInfo.PlayerList[i].UserID, m2C_HorseNoticeInfo);
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
