using System.Collections.Generic;

namespace ET
{
    public static class CenterServerCrowdSystem
    {
        public const int RefreshSeconds = 600;

        /// <summary>进游戏直接写中心库。重复登录只覆盖时间，人数等十分钟重算。</summary>
        public static async ETTask SaveRoleLogin(int zone, long roleId, long loginTime)
        {
            if (zone <= 0 || roleId <= 0 || loginTime <= 0)
            {
                return;
            }

            DBComponent db = Game.Scene.GetComponent<DBComponent>();
            List<DBCenterZoneCrowdInfo> list = await db.Query<DBCenterZoneCrowdInfo>(CommonConfig.CenterZoneId, d => d.Id == zone);
            DBCenterZoneCrowdInfo info = list != null && list.Count > 0 ? list[0] : new DBCenterZoneCrowdInfo { Id = zone };
            if (list != null)
            {
                for (int i = 1; i < list.Count; i++)
                {
                    list[i].Dispose();
                }
            }

            info.RoleLoginTime ??= new Dictionary<long, long>();
            info.RoleLoginTime[roleId] = loginTime;
            await db.Save(CommonConfig.CenterZoneId, info);
            info.Dispose();
        }

        public static async ETTask LoadZoneCrowd(this CenterServerComponent self)
        {
            List<DBCenterZoneCrowdInfo> list = await Game.Scene.GetComponent<DBComponent>()
                    .Query<DBCenterZoneCrowdInfo>(CommonConfig.CenterZoneId, d => d.Id > 0);
            foreach (DBCenterZoneCrowdInfo old in self.ZoneCrowdInfos.Values)
            {
                old.Dispose();
            }

            self.ZoneCrowdInfos.Clear();
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    DBCenterZoneCrowdInfo info = list[i];
                    info.RoleLoginTime ??= new Dictionary<long, long>();
                    self.ZoneCrowdInfos[(int)info.Id] = info;
                }
            }

            await self.PruneAndSave();
        }

        public static int GetServerCrowdState(this CenterServerComponent self, int zone)
        {
            if (self.StopServer)
            {
                return ServerListState.Maintain;
            }

            int count = self.ZoneCrowdInfos.TryGetValue(zone, out DBCenterZoneCrowdInfo info) ? info.Count : 0;
            LDGlobalValueCategory config = LDGlobalValueCategory.Instance;
            // 窗口内登录人数。0|100|200：[0,100) 流畅，[100,200) 拥挤，[200,+∞) 爆满
            if (config.ServerCrowdFull > 0 && count >= config.ServerCrowdFull)
            {
                return ServerListState.Full;
            }

            if (config.ServerCrowdBusy > 0 && count >= config.ServerCrowdBusy)
            {
                return ServerListState.Busy;
            }

            return ServerListState.Smooth;
        }

        private static async ETTask PruneAndSave(this CenterServerComponent self)
        {
            long expireBefore = TimeHelper.ServerNow() - LDGlobalValueCategory.Instance.ServerCrowdWindowMs;
            DBComponent db = Game.Scene.GetComponent<DBComponent>();
            foreach (KeyValuePair<int, DBCenterZoneCrowdInfo> pair in self.ZoneCrowdInfos)
            {
                Dictionary<long, long> roles = pair.Value.RoleLoginTime;
                List<long> expired = null;
                foreach (KeyValuePair<long, long> role in roles)
                {
                    if (role.Value >= expireBefore)
                    {
                        continue;
                    }

                    expired ??= new List<long>();
                    expired.Add(role.Key);
                }

                if (expired != null)
                {
                    for (int i = 0; i < expired.Count; i++)
                    {
                        roles.Remove(expired[i]);
                    }

                    await db.Save(CommonConfig.CenterZoneId, pair.Value);
                }

                pair.Value.Count = roles.Count;
            }
        }
    }
}
