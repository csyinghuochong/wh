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
                if (!EnsureDatabase(homeZone))
                {
                    continue;
                }
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

        public static async ETTask<List<FriendInfo>> GetOnlineFriends(long userId)
        {
            List<FriendInfo> empty = new List<FriendInfo>();
            int homeZone = PrepareHomeZone(userId);
            if (homeZone == 0)
            {
                return empty;
            }
            DBFriendInfo dBFriendInfo = await DBHelper.GetComponent<DBFriendInfo>(homeZone, userId);
            if (dBFriendInfo?.FriendList == null)
            {
                return empty;
            }

            HashSet<long> onlineIds = await CollectOnlineIds(dBFriendInfo.FriendList);
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

        /// <summary>全服搜索。整段是数字时按 RoleNo 里的区服只查那一区；否则按名字逐区模糊匹配。不含自己，最多 SearchMaxCount。</summary>
        public static async ETTask<List<long>> SearchPlayerIds(int zone, long selfId, string keyword)
        {
            List<long> ids = new List<long>();
            if (long.TryParse(keyword, out long roleNo) && roleNo > 0)
            {
                int roleZone = (int)(roleNo / RoleNoHelper.PlayerNoSpan);
                if (EnsureDatabase(roleZone))
                {
                    AppendPlayers(ids, await QueryByRoleNo(roleZone, roleNo), selfId);
                }
            }

            int maxNameLength = LDGlobalValueCategory.Instance.GetInt(GlobalValueKey.Global_Role_Name_Character_Length);
            if (keyword.Length > maxNameLength)
            {
                return ids;
            }

            List<int> zones = SearchZones(zone);
            for (int i = 0; i < zones.Count; i++)
            {
                if (ids.Count >= SearchMaxCount)
                {
                    break;
                }
                if (!EnsureDatabase(zones[i]))
                {
                    continue;
                }
                int limit = SearchMaxCount - ids.Count;
                AppendPlayers(ids, await QueryByName(zones[i], keyword, limit), selfId);
            }

            return ids;
        }

        /// <summary>按角色归属服问在线。搜索结果可能跨区，不能只用当前区的在线名单。</summary>
        public static async ETTask<HashSet<long>> CollectOnlineIds(List<long> ids)
        {
            HashSet<long> onlineIds = new HashSet<long>();
            if (ids == null || ids.Count == 0)
            {
                return onlineIds;
            }

            List<int> zones = new List<int>();
            for (int i = 0; i < ids.Count; i++)
            {
                int homeZone = UnitZoneHelper.GetHomeZone(ids[i]);
                if (homeZone <= 0 || zones.Contains(homeZone))
                {
                    continue;
                }
                zones.Add(homeZone);
            }

            for (int i = 0; i < zones.Count; i++)
            {
                HashSet<long> zoneOnline = await ServerMessageHelper.GetChatOnlineUnitIds(zones[i]);
                foreach (long id in zoneOnline)
                {
                    onlineIds.Add(id);
                }
            }

            return onlineIds;
        }

        static void AppendPlayers(List<long> ids, List<RoleInfoComponentServer> found, long selfId)
        {
            for (int i = 0; i < found.Count; i++)
            {
                if (ids.Count >= SearchMaxCount)
                {
                    return;
                }

                long id = PlayerId(found[i], 0);
                if (id == 0 || id == selfId || ids.Contains(id))
                {
                    continue;
                }

                ids.Add(id);
            }
        }

        static List<int> SearchZones(int selfZone)
        {
            List<int> zones = ServerMessageHelper.GetAllZone();
            int index = zones.IndexOf(selfZone);
            if (index > 0)
            {
                zones.RemoveAt(index);
                zones.Insert(0, selfZone);
            }
            if (index < 0 && selfZone > 0)
            {
                zones.Insert(0, selfZone);
            }

            return zones;
        }

        public static int PrepareHomeZone(long userId)
        {
            int zone = UnitZoneHelper.GetHomeZone(userId);
            if (!EnsureDatabase(zone))
            {
                return 0;
            }

            return zone;
        }

        public static bool EnsureDatabase(int zone)
        {
            if (zone <= 0 || !StartZoneConfigCategory.Instance.Contain(zone))
            {
                return false;
            }

            Game.Scene.GetComponent<DBComponent>().InitDatabase(StartZoneConfigCategory.Instance.Get(zone));
            return true;
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

        static async ETTask<List<RoleInfoComponentServer>> QueryByRoleNo(int zone, long roleNo)
        {
            if (!EnsureDatabase(zone))
            {
                return new List<RoleInfoComponentServer>();
            }

            DBComponent db = Game.Scene.GetComponent<DBComponent>();
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, RandomHelper.RandInt64() % DBComponent.TaskCount))
            {
                FindOptions<RoleInfoComponentServer> options = new FindOptions<RoleInfoComponentServer> { Limit = SearchMaxCount };
                IAsyncCursor<RoleInfoComponentServer> cursor = await db.GetCollection<RoleInfoComponentServer>(zone)
                        .FindAsync(d => d.RoleInfo.RoleNo == roleNo, options);
                return await cursor.ToListAsync();
            }
        }

        static async ETTask<List<RoleInfoComponentServer>> QueryByName(int zone, string keyword, int limit)
        {
            if (limit <= 0 || !EnsureDatabase(zone))
            {
                return new List<RoleInfoComponentServer>();
            }

            DBComponent db = Game.Scene.GetComponent<DBComponent>();
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, RandomHelper.RandInt64() % DBComponent.TaskCount))
            {
                FindOptions<RoleInfoComponentServer> options = new FindOptions<RoleInfoComponentServer> { Limit = limit };
                IAsyncCursor<RoleInfoComponentServer> cursor = await db.GetCollection<RoleInfoComponentServer>(zone)
                        .FindAsync(d => d.UserName.Contains(keyword), options);
                return await cursor.ToListAsync();
            }
        }
    }
}
