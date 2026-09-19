using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2U_UnionOnlineHandler : AMActorRpcHandler<Scene, C2U_UnionOnlineRequest, U2C_UnionOnlineResponse>
    {
        protected override async ETTask Run(Scene scene, C2U_UnionOnlineRequest request, U2C_UnionOnlineResponse response, Action reply)
        {
            response.PlayerList = await UnionHelper.GetOnlinePlayers(scene, request.UnionId, request.UserId);
            reply();
        }
    }
}
