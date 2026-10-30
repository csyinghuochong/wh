using System;

namespace ET
{

    [ActorMessageHandler]
    public class M2T_TeamDungeonPrepareHandler : AMActorRpcHandler<Scene, M2T_TeamDungeonPrepareRequest, T2M_TeamDungeonPrepareResponse>
    {
        protected override async ETTask Run(Scene scene, M2T_TeamDungeonPrepareRequest request, T2M_TeamDungeonPrepareResponse response, Action reply)
        {
            TeamInfo teamInfo = scene.GetComponent<TeamSceneComponent>().GetTeamInfo(request.TeamId);
            if (teamInfo == null)
            {
                Log.Debug($"M2T_TeamDungeonPrepare: teamInfo == null");
                response.Error = ErrorCode.Err_TeamIsNull;
                reply();
                return;
            }
            
            // 队长发起：自己准备，其余清成未准备，方便重新确认。队员只改自己。
            bool leaderStart = teamInfo.TeamId == request.UnitID && request.Prepare == 1;
            bool found = false;
            for (int i = 0; i < teamInfo.PlayerList.Count; i++)
            {
                if (leaderStart)
                {
                    teamInfo.PlayerList[i].Prepare = teamInfo.PlayerList[i].UserID == teamInfo.TeamId ? 1 : 0;
                    if (teamInfo.PlayerList[i].UserID == request.UnitID)
                    {
                        found = true;
                    }
                    continue;
                }

                if (teamInfo.PlayerList[i].UserID != request.UnitID)
                {
                    continue;
                }

                teamInfo.PlayerList[i].Prepare = request.Prepare;
                found = true;
            }

            if (!found)
            {
                response.Error = ErrorCode.Err_TeamIsNull;
                reply();
                return;
            }

            //0未选择 1同意 2拒绝
            int errorCode = request.ErrorCode;
            for(int i = 0; i < teamInfo.PlayerList.Count; i++)
            {
                if (teamInfo.PlayerList[i].Prepare == 2)
                {
                    errorCode = ErrorCode.Err_PlayerRefuse;
                    break;
                }
                if (teamInfo.PlayerList[i].Prepare == 0)
                {
                    errorCode = ErrorCode.Err_HaveNotPrepare;
                    break;
                }
            }

            M2C_TeamDungeonPrepareResult m2C_HorseNoticeInfo = new M2C_TeamDungeonPrepareResult()
            {
                TeamInfo = teamInfo,
                ErrorCode = errorCode,
            };
            for (int i = 0; i < teamInfo.PlayerList.Count; i++)
            {
                long userId = teamInfo.PlayerList[i].UserID;
                await ServerMessageHelper.SendToClient(UnitZoneHelper.GetHomeZone(userId), userId, m2C_HorseNoticeInfo);
            }
            response.TeamInfo = teamInfo;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
