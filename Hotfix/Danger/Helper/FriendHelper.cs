using System;
using System.Collections.Generic;

namespace ET
{
    public static class FriendHelper
    {
        public static async ETTask<List<FriendInfo>> GetFriendInfos(List<long> friends, HashSet<long> onlineIds)
        {
            List<FriendInfo> friendInfos = new List<FriendInfo>();
            if (friends == null)
            {
                return friendInfos;
            }

            for (int i = 0; i < friends.Count; i++)
            {
                long friendId = friends[i];
                int homeZone = UnitZoneHelper.GetHomeZone(friendId);
                RoleInfoComponentServer roleInfoComponentServer = await DBHelper.GetComponent<RoleInfoComponentServer>(homeZone, friendId);
                if (roleInfoComponentServer == null)
                {
                    continue;
                }

                RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;
                friendInfos.Add(new FriendInfo()
                {
                    UserId = friendId,
                    PlayerLevel = roleInfo.Lv,
                    OnLineTime = onlineIds != null && onlineIds.Contains(friendId) ? 1 : 0,
                    PlayerName = roleInfo.Name,
                    Occ = roleInfo.Occ
                });
            }

            return friendInfos;
        }
    }
}
