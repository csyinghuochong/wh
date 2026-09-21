using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2T_TeamInfoHandler : AMActorRpcHandler<Scene, C2T_TeamInfoRequest, T2C_TeamInfoResponse>
    {
        protected override async ETTask Run(Scene scene, C2T_TeamInfoRequest request, T2C_TeamInfoResponse response, Action reply)
        {
            response.TeamInfo = scene.GetComponent<TeamSceneComponent>().GetTeamInfo(request.UserId);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
