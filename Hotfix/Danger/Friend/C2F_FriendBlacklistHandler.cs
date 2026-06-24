using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2F_FriendBlacklistHandler : AMActorRpcHandler<Scene, C2F_FriendBlacklistRequest, F2C_FriendBlacklistResponse>
    {
        protected override async ETTask Run(Scene scene, C2F_FriendBlacklistRequest request, F2C_FriendBlacklistResponse response, Action reply)
        {
            long dbCacheId = StartSceneConfigCategory.Instance.GetBySceneName(scene.DomainZone(), Enum.GetName(SceneType.DBCache)).InstanceId;
 
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
                if (!dBFriendInfo.Blacklist.Contains(request.FriendId))
                {
                    reply();
                    return;
                }
                dBFriendInfo.Blacklist.Remove(request.FriendId);
            }
            
            await DBHelper.SaveComponent(scene.DomainZone(), request.UserID,dBFriendInfo );
            reply();
        }
    }
}
