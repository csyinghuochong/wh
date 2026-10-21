using System;
using System.Collections.Generic;
using MongoDB.Driver;

namespace ET
{
    public static class FriendHelper
    {
        public const int SearchMaxCount = 20;

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
                    OnLineTime = onlineIds != null && onlineIds.Contains(friendId) ? 1 : roleInfoComponentServer.LastOfflineTime,
                    PlayerName = roleInfo.Name,
                    Occ = roleInfo.Occ,
                    RoleHeadId = roleInfo.RoleHeadId,
                    RoleHeadFrame = roleInfo.RoleHeadFrame,
                    Combat = roleInfo.Combat,
                });
            }

            return friendInfos;
        }

        public static async ETTask<List<FriendInfo>> GetOnlineFriends(int zone, long userId)
        {
            List<FriendInfo> empty = new List<FriendInfo>();
            DBFriendInfo dBFriendInfo = await DBHelper.GetComponent<DBFriendInfo>(zone, userId);
            if (dBFriendInfo?.FriendList == null)
            {
                return empty;
            }

            HashSet<long> onlineIds = await ServerMessageHelper.GetChatOnlineUnitIds(zone);
            List<long> onlineFriends = new List<long>();
            for (int i = 0; i < dBFriendInfo.FriendList.Count; i++)
            {
                long friendId = dBFriendInfo.FriendList[i];
                if (friendId == userId)
                {
                    continue;
                }
                if (onlineIds.Contains(friendId))
                {
                    onlineFriends.Add(friendId);
                }
            }

            return await GetFriendInfos(onlineFriends, onlineIds);
        }

        /// <summary>名字模糊匹配 UserName。整段是数字时先按角色ID精确查。不含自己，最多 SearchMaxCount。</summary>
        public static async ETTask<List<long>> SearchPlayerIds(int zone, long selfId, string keyword)
        {
            List<long> ids = new List<long>();
            if (long.TryParse(keyword, out long userId) && userId > 0 && userId != selfId)
            {
                RoleInfoComponentServer roleInfoComponentServer = await DBHelper.GetPlayerComponent<RoleInfoComponentServer>(userId);
                if (roleInfoComponentServer != null)
                {
                    ids.Add(PlayerId(roleInfoComponentServer, userId));
                }
            }

            int maxNameLength = LDGlobalValueCategory.Instance.GetInt(GlobalValueKey.Global_Role_Name_Character_Length);
            if (keyword.Length > maxNameLength)
            {
                return ids;
            }

            List<RoleInfoComponentServer> named = await QueryByName(zone, keyword);
            for (int i = 0; i < named.Count; i++)
            {
                if (ids.Count >= SearchMaxCount)
                {
                    break;
                }

                long id = PlayerId(named[i], 0);
                if (id == 0 || id == selfId || ids.Contains(id))
                {
                    continue;
                }

                ids.Add(id);
            }

            return ids;
        }

        static long PlayerId(RoleInfoComponentServer roleInfoComponentServer, long fallback)
        {
            if (roleInfoComponentServer.RoleInfo.UserId > 0)
            {
                return roleInfoComponentServer.RoleInfo.UserId;
            }

            if (roleInfoComponentServer.Id > 0)
            {
                return roleInfoComponentServer.Id;
            }

            return fallback;
        }

        static async ETTask<List<RoleInfoComponentServer>> QueryByName(int zone, string keyword)
        {
            DBComponent db = Game.Scene.GetComponent<DBComponent>();
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, RandomHelper.RandInt64() % DBComponent.TaskCount))
            {
                FindOptions<RoleInfoComponentServer> options = new FindOptions<RoleInfoComponentServer> { Limit = SearchMaxCount };
                IAsyncCursor<RoleInfoComponentServer> cursor = await db.GetCollection<RoleInfoComponentServer>(zone)
                        .FindAsync(d => d.UserName.Contains(keyword), options);
                return await cursor.ToListAsync();
            }
        }
    }
}
