using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2F_FriendDeleteHandler : AMActorRpcHandler<Scene, C2F_FriendDeleteRequest, F2C_FriendDeleteResponse>
    {
        protected override async ETTask Run(Scene scene, C2F_FriendDeleteRequest request, F2C_FriendDeleteResponse response, Action reply)
        {
            int selfZone = FriendHelper.PrepareHomeZone(request.UserID);
            if (selfZone == 0)
            {
                reply();
                return;
            }
            DBFriendInfo dBFriendInfo = await DBHelper.GetComponent<DBFriendInfo>(selfZone, request.UserID);
            if (dBFriendInfo == null || !dBFriendInfo.FriendList.Remove(request.FriendID))
            {
                reply();
                return;
            }

            DBHelper.SaveComponent(selfZone, request.UserID, dBFriendInfo).Coroutine();

            int friendZone = FriendHelper.PrepareHomeZone(request.FriendID);
            DBFriendInfo dBFriendInfo_2 = friendZone == 0 ? null : await DBHelper.GetComponent<DBFriendInfo>(friendZone, request.FriendID);
            if (dBFriendInfo_2 != null && dBFriendInfo_2.FriendList.Remove(request.UserID))
            {
                DBHelper.SaveComponent(friendZone, request.FriendID, dBFriendInfo_2).Coroutine();
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
