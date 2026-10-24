using System;

namespace ET
{
    [ActorMessageHandler]
    public class T2G_SetTeamServerHandler : AMActorRpcHandler<Scene, T2G_SetTeamServerRequest, G2T_SetTeamServerResponse>
    {
        protected override async ETTask Run(Scene scene, T2G_SetTeamServerRequest request, G2T_SetTeamServerResponse response, Action reply)
        {
            Player player = scene.GetComponent<PlayerComponent>().GetByUserId(request.UserID);
            if (player != null)
            {
                player.JoinedTeamServerID = request.TeamServerId;
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
