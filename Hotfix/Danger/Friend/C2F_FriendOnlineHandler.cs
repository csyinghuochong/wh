using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2F_FriendOnlineHandler : AMActorRpcHandler<Scene, C2F_FriendOnlineRequest, F2C_FriendOnlineResponse>
    {
        protected override async ETTask Run(Scene scene, C2F_FriendOnlineRequest request, F2C_FriendOnlineResponse response, Action reply)
        {
            response.FriendList = await FriendHelper.GetOnlineFriends(scene.DomainZone(), request.UserID);
            reply();
        }
    }
}
