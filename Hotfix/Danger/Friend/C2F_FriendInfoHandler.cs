using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public  class C2F_FriendInfoHandler : AMActorRpcHandler<Scene, C2F_FriendInfoRequest, F2C_FriendInfoResponse>
    {
        protected override async ETTask Run(Scene scene, C2F_FriendInfoRequest request, F2C_FriendInfoResponse response, Action reply)
        {
            DBFriendInfo dBFriendInfo = await DBHelper.GetComponent<DBFriendInfo>(scene.DomainZone(), request.UserID);
            if (dBFriendInfo == null)
            {
                Log.Warning($"C2F_FriendInfo==null: {request.UserID}");
                reply();
                return;
            }
            HashSet<long> onlineIds = await ServerMessageHelper.GetChatOnlineUnitIds(scene.DomainZone());
            response.FriendList = await FriendHelper.GetFriendInfos(dBFriendInfo.FriendList, onlineIds);
            response.ApplyList = await FriendHelper.GetFriendInfos(dBFriendInfo.ApplyList, onlineIds);
            response.Blacklist = await FriendHelper.GetFriendInfos(dBFriendInfo.Blacklist, onlineIds);


            HashSet<long> friendIdSet = new HashSet<long>();
            for (int k = 0; k < response.FriendList.Count; k++)
            {
                friendIdSet.Add(response.FriendList[k].UserId);
            }

            for (int i = dBFriendInfo.FriendChats.Count - 1;i >= 0; i-- )
            {
                if (!friendIdSet.Contains(dBFriendInfo.FriendChats[i].UserId))
                {
                    dBFriendInfo.FriendChats.RemoveAt(i);   
                }
            }

            response.FriendChats = dBFriendInfo.FriendChats;
            reply();
        }
    }
}
