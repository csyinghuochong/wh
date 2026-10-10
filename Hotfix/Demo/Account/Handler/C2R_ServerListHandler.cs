using System;
using System.Collections.Generic;

namespace ET
{
    //游戏服务器处理
    [MessageHandler]
    public class C2R_ServerListHandler : AMRpcHandler<C2R_ServerList, R2C_ServerList>
    {
        protected override async ETTask Run(Session session, C2R_ServerList request, R2C_ServerList response, Action reply)
        {
            try
            {
                if (session.GetComponent<SessionLockingComponent>() != null)
                {
                    response.Error = ErrorCode.ERR_RequestRepeatedly;
                    reply();
                    session.Disconnect().Coroutine();
                    return;
                }

                using (session.AddComponent<SessionLockingComponent>())
                {
                    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.GetServerList, 0))
                    {
                        long serverTime = TimeHelper.ServerNow();
                        List<ServerItem> serverItems = ServerHelper.GetServerList();
                        CenterServerComponent centerServer = session.DomainScene().GetComponent<CenterServerComponent>();

                        response.ServerItems.Clear();
                        for (int i = 0; i < serverItems.Count; i++)
                        {
                            ServerItem serverItem = serverItems[i];
                            if (serverItem.Show == 0 || serverItem.ServerOpenTime > serverTime)
                            {
                                continue;
                            }

                            ServerItem copy = CopyServerItem(serverItem);
                            copy.State = centerServer.GetServerCrowdState(copy.ServerId);
                            response.ServerItems.Add(copy);
                        }

                        ApplyServerTags(response.ServerItems);

                        response.Message = centerServer.TianQiValue.ToString();
                        string[] stringxxx = LogHelper.GetNoticeNew().Split('@');

                        long timeNow = TimeHelper.ServerNow();
                        long timeColse = 0;
                        if (stringxxx.Length == 3)
                        {
                            response.NoticeVersion = stringxxx[0];
                            timeColse = long.Parse(stringxxx[1]);
                            response.NoticeText = stringxxx[2];
                        }

                        string[] stringxxx_EN = LogHelper.GetNoticeNew_EN().Split('@');
                        if (stringxxx_EN.Length == 3)
                        {
                            response.NoticeVersion_EN = stringxxx_EN[0];
                            timeColse = long.Parse(stringxxx_EN[1]);
                            response.NoticeText_EN = stringxxx_EN[2];
                        }

                        if (timeColse > 0 && timeNow > timeColse + TimeHelper.OneDay * 3)
                        {
                            response.ShowNotice = false;
                        }
                        else
                        {
                            response.ShowNotice = true;
                        }

                        response.SmsVerifyType = 0; //0 mob  1 aliyun
                    }

                    await FillAccountRoleList(request.Account, response);
                    reply();
                    await ETTask.CompletedTask;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private static async ETTask FillAccountRoleList(string account, R2C_ServerList response)
        {
            response.RoleList.Clear();
            if (string.IsNullOrEmpty(account))
            {
                return;
            }

            account = account.Trim().ToLower();
            if (string.IsNullOrEmpty(account))
            {
                return;
            }

            List<DBCenterAccountInfo> centerAccountInfoList = await Game.Scene.GetComponent<DBComponent>()
                .Query<DBCenterAccountInfo>(CommonConfig.CenterZoneId, d => d.Account.Equals(account));
            if (centerAccountInfoList == null || centerAccountInfoList.Count == 0)
            {
                return;
            }

            DBCenterAccountInfo dbCenterAccountInfo = centerAccountInfoList[0];
            try
            {
                List<CreateRoleInfo> roleList = dbCenterAccountInfo.RoleList;
                if (roleList == null)
                {
                    return;
                }

                for (int i = 0; i < roleList.Count; i++)
                {
                    CreateRoleInfo roleInfo = roleList[i];
                    if (roleInfo == null || roleInfo.State == (int)RoleInfoState.Freeze)
                    {
                        continue;
                    }

                    if (!LDOccupationCategory.Instance.Contain(roleInfo.PlayerOcc))
                    {
                        continue;
                    }

                    response.RoleList.Add(CloneHelper.DeepClone(roleInfo));
                }

                SortAccountRoles(response.RoleList);
            }
            finally
            {
                dbCenterAccountInfo.Dispose();
            }
        }

        private static ServerItem CopyServerItem(ServerItem src)
        {
            ServerItem copy = new ServerItem
            {
                ServerId = src.ServerId,
                ServerIp = src.ServerIp,
                ServerName = src.ServerName,
                ServerOpenTime = src.ServerOpenTime,
                Show = src.Show,
                New = src.New,
            };
            if (src.PlatformList != null && src.PlatformList.Count > 0)
            {
                copy.PlatformList.AddRange(src.PlatformList);
            }

            return copy;
        }

        private static void ApplyServerTags(List<ServerItem> servers)
        {
            if (servers.Count == 0)
            {
                return;
            }

            List<ServerItem> order = new List<ServerItem>(servers);
            order.Sort((a, b) =>
            {
                int timeCmp = b.ServerOpenTime.CompareTo(a.ServerOpenTime);
                return timeCmp != 0 ? timeCmp : b.ServerId.CompareTo(a.ServerId);
            });
            order[0].Tag = ServerListTag.New;
            int recommendCount = servers.Count < 3 ? servers.Count : 3;
            for (int i = 1; i < recommendCount; i++)
            {
                order[i].Tag = ServerListTag.Recommend;
            }
        }

        /// <summary>
        /// 最新登录一个，然后 30 天内按登录时间，其余按服务器顺序、等级从高到低、创建时间从早到晚。
        /// </summary>
        private static void SortAccountRoles(List<CreateRoleInfo> roles)
        {
            if (roles.Count == 0)
            {
                return;
            }

            long now = TimeHelper.ServerNow();
            const long recentMs = 30L * TimeHelper.OneDay;
            int latestIndex = -1;
            long latestTime = 0;
            for (int i = 0; i < roles.Count; i++)
            {
                if (roles[i].LastLoginTime > latestTime)
                {
                    latestTime = roles[i].LastLoginTime;
                    latestIndex = i;
                }
            }

            for (int i = 0; i < roles.Count; i++)
            {
                CreateRoleInfo role = roles[i];
                if (i == latestIndex)
                {
                    role.LoginTag = RoleLoginTag.Latest;
                }
                else if (role.LastLoginTime > 0 && now - role.LastLoginTime <= recentMs)
                {
                    role.LoginTag = RoleLoginTag.Recent;
                }
                else
                {
                    role.LoginTag = RoleLoginTag.None;
                }
            }

            Dictionary<int, int> serverOrder = new Dictionary<int, int>();
            List<ServerItem> serverItems = ServerHelper.GetServerList();
            for (int i = 0; i < serverItems.Count; i++)
            {
                serverOrder[serverItems[i].ServerId] = i;
            }

            roles.Sort((a, b) =>
            {
                int groupCmp = RoleGroup(a).CompareTo(RoleGroup(b));
                if (groupCmp != 0)
                {
                    return groupCmp;
                }

                if (a.LoginTag != RoleLoginTag.None)
                {
                    int timeCmp = b.LastLoginTime.CompareTo(a.LastLoginTime);
                    return timeCmp != 0 ? timeCmp : a.UserID.CompareTo(b.UserID);
                }

                int serverCmp = ServerRank(a.ServerId).CompareTo(ServerRank(b.ServerId));
                if (serverCmp != 0)
                {
                    return serverCmp;
                }

                int levelCmp = b.PlayerLv.CompareTo(a.PlayerLv);
                if (levelCmp != 0)
                {
                    return levelCmp;
                }

                int createCmp = a.CreateTime.CompareTo(b.CreateTime);
                return createCmp != 0 ? createCmp : a.UserID.CompareTo(b.UserID);
            });

            int RoleGroup(CreateRoleInfo role)
            {
                if (role.LoginTag == RoleLoginTag.Latest)
                {
                    return 0;
                }

                return role.LoginTag == RoleLoginTag.Recent ? 1 : 2;
            }

            int ServerRank(int serverId)
            {
                return serverOrder.TryGetValue(serverId, out int order) ? order : int.MaxValue;
            }
        }
    }
}
