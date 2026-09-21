using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2T_TeamApplyListHandler : AMActorRpcHandler<Scene, C2T_TeamApplyListRequest, T2C_TeamApplyListResponse>
    {
        protected override async ETTask Run(Scene scene, C2T_TeamApplyListRequest request, T2C_TeamApplyListResponse response, Action reply)
        {
            TeamSceneComponent teamSceneComponent = scene.GetComponent<TeamSceneComponent>();
            TeamInfo teamInfo = teamSceneComponent.GetTeamInfo(request.UserId);
            if (teamInfo == null || teamInfo.TeamId != request.UserId)
            {
                response.Error = ErrorCode.ERR_IsNotLeader;
                response.PlayerList = new List<TeamPlayerInfo>();
                reply();
                return;
            }

            response.PlayerList = teamSceneComponent.GetApplyList(teamInfo.TeamId);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
