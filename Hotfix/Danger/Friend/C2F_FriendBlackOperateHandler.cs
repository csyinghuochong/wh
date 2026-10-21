using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2F_FriendBlackOperateHandler : AMActorRpcHandler<Scene, C2F_FriendBlackOperateRequest, F2C_FriendBlackOperateResponse>
    {
        protected override async ETTask Run(Scene scene, C2F_FriendBlackOperateRequest request, F2C_FriendBlackOperateResponse response, Action reply)
        {
            DBFriendInfo dBFriendInfo = await DBHelper.GetComponent<DBFriendInfo>(scene.DomainZone(), request.UserID);
            if (dBFriendInfo.FriendList.Contains(request.FriendId))
            {
                //在好友列表
                reply();
                return;
            }

            if (request.OperateType == 1)
            {
                if (dBFriendInfo.Blacklist.Contains(request.FriendId))
                {
                    reply();
                    return;
                }
                dBFriendInfo.Blacklist.Add(request.FriendId);
            }
            if (request.OperateType == 2)
            {
                if (!dBFriendInfo.Blacklist.Remove(request.FriendId))
                {
                    reply();
                    return;
                }
            }
            
            await DBHelper.SaveComponent(scene.DomainZone(), request.UserID,dBFriendInfo );
            reply();
        }
    }
}
