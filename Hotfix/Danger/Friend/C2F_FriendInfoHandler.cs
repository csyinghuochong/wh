using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public  class C2F_FriendInfoHandler : AMActorRpcHandler<Scene, C2F_FriendInfoRequest, F2C_FriendInfoResponse>
    {
        protected override async ETTask Run(Scene scene, C2F_FriendInfoRequest request, F2C_FriendInfoResponse response, Action reply)
        {
            int selfZone = FriendHelper.PrepareHomeZone(request.UserID);
            if (selfZone == 0)
            {
                reply();
                return;
            }
            DBFriendInfo dBFriendInfo = await DBHelper.GetComponent<DBFriendInfo>(selfZone, request.UserID);
            if (dBFriendInfo == null)
            {
                Log.Warning($"C2F_FriendInfo==null: {request.UserID}");
                reply();
                return;
            }
            List<long> watchIds = new List<long>();
            watchIds.AddRange(dBFriendInfo.FriendList);
            watchIds.AddRange(dBFriendInfo.ApplyList);
            watchIds.AddRange(dBFriendInfo.Blacklist);
            HashSet<long> onlineIds = await FriendHelper.CollectOnlineIds(watchIds);
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
            response.SendApplyList.AddRange(dBFriendInfo.SendApplyList);
            reply();
        }
    }
}
