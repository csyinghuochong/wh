using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2F_FriendSearchHandler : AMActorRpcHandler<Scene, C2F_FriendSearchRequest, F2C_FriendSearchResponse>
    {
        protected override async ETTask Run(Scene scene, C2F_FriendSearchRequest request, F2C_FriendSearchResponse response, Action reply)
        {
            string keyword = request.Keyword == null ? "" : request.Keyword.Trim();
            if (keyword.Length == 0 || !StringHelper.IsSafeSqlString(keyword))
            {
                response.Error = ErrorCode.ERR_Parameter;
                reply();
                return;
            }

            List<long> ids = await FriendHelper.SearchPlayerIds(scene.DomainZone(), request.UserID, keyword);
            HashSet<long> onlineIds = await FriendHelper.CollectOnlineIds(ids);
            response.RoleList = await FriendHelper.GetFriendInfos(ids, onlineIds);
            int myZone = FriendHelper.PrepareHomeZone(request.UserID);
            DBFriendInfo myFriendInfo = myZone == 0 ? null : await DBHelper.GetComponent<DBFriendInfo>(myZone, request.UserID);
            if (myFriendInfo != null)
            {
                for (int i = 0; i < response.RoleList.Count; i++)
                {
                    if (myFriendInfo.SendApplyList.Contains(response.RoleList[i].UserId))
                    {
                        response.RoleList[i].Applied = 1;
                    }
                }
            }

            reply();
        }
    }
}
