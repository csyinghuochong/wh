using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2T_TeamDismissHandler : AMActorRpcHandler<Scene, C2T_TeamDismissRequest, T2C_TeamDismissResponse>
    {
        protected override async ETTask Run(Scene scene, C2T_TeamDismissRequest request, T2C_TeamDismissResponse response, Action reply)
        {
            TeamSceneComponent teamSceneComponent = scene.GetComponent<TeamSceneComponent>();
            TeamInfo teamInfo = teamSceneComponent.GetTeamInfo(request.UserId);
            if (teamInfo == null || teamInfo.TeamId != request.UserId)
            {
                response.Error = ErrorCode.ERR_IsNotLeader;
                reply();
                return;
            }

            teamSceneComponent.OnRecvTeamDismiss(request.UserId);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
