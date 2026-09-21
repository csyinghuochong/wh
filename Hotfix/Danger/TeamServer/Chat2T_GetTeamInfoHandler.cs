using System;

namespace ET
{

    [ActorMessageHandler]
    public class Chat2T_GetTeamInfoHandler : AMActorRpcHandler<Scene, Chat2T_GetTeamInfoRequest, T2Chat_GetTeamInfoResponse>
    {
        protected override async ETTask Run(Scene scene, Chat2T_GetTeamInfoRequest request, T2Chat_GetTeamInfoResponse response, Action reply)
        {
            TeamSceneComponent teamSceneComponent = scene.GetComponent<TeamSceneComponent>();
            response.TeamInfo = teamSceneComponent.GetTeamInfo(request.UserID); 
            reply();
            await ETTask.CompletedTask;

        }
    }
}
