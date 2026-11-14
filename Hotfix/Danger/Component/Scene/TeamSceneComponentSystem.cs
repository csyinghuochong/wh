using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    public static class TeamSceneComponentSystem
    {

        public static void  CreateTeamDungeon(this TeamSceneComponent self, TeamInfo teamInfo)
        {
            //动态创建副本
            long fubenid = IdGenerater.Instance.GenerateId();
            long fubenInstanceId = IdGenerater.Instance.GenerateInstanceId();
            Scene fubnescene = SceneFactory.Create(self, fubenid, fubenInstanceId, self.DomainZone(), "TeamDungeon" + fubenid.ToString(), SceneType.Map);
            TeamDungeonComponent teamDungeonComponent = fubnescene.AddComponent<TeamDungeonComponent>();
            MapComponent mapComponent = fubnescene.GetComponent<MapComponent>();
            LDScene ldScene = LDSceneCategory.Instance.Get(teamInfo.SceneId);
            mapComponent.SetMapInfo((int)MapTypeEnum.TeamDungeon, teamInfo.SceneId, 0);
            mapComponent.NavMeshId = ldScene.GetNavMeshId();

            teamDungeonComponent.EnterTime = TimeHelper.ServerNow();
            teamDungeonComponent.FubenType = teamInfo.FubenType;
            teamDungeonComponent.BossDeadPosition = new Vector3((float)ldScene.Born_Pos[0] , (float)ldScene.Born_Pos[1] , (float)ldScene.Born_Pos[2] );

            teamInfo.FubenInstanceId = fubenInstanceId;
            teamInfo.FubenUUId = fubenid;
            teamInfo.FubenState = (int)TeamDungeonState.Opened;
            teamInfo.FubenApplyEndTime = 0;
            teamInfo.FubenCreateTime = teamDungeonComponent.EnterTime;

            // 实例使用独立快照；队伍通关后可再申请，旧实例不能跟随队伍修改。
            teamDungeonComponent.TeamInfo = MongoHelper.Clone(teamInfo);
            teamDungeonComponent.InitPlayerList();
            teamDungeonComponent.Timer = TimerComponent.Instance.NewRepeatedTimer(1000, TimerType.TeamDropTimer, teamDungeonComponent);
            Game.Scene.GetComponent<RecastPathComponent>().Update(mapComponent.NavMeshId);
            SceneCreatureHelp.CreateSceneRole(fubnescene, teamInfo.SceneId);
            teamDungeonComponent.NotifyDungeonStartMarquee();

            //69和 72的2个副本 有10%概率 在每个BOSS附近刷新出 80002010 这个宝箱，
            //这个宝箱的掉落是只有自己的可以拾取的，并且宝箱和掉落只有自己可见，
            //在聊天广播中提示要加入XXX玩家通过XXX宝箱拾取XXX,而不是之前的那个通用的广播

            TransferHelper.NoticeFubenCenter(fubnescene, 1).Coroutine();
        }

        /// <summary>等到申请结束时间后创建副本；取消或被新申请替代时终止。</summary>
        public static async ETTask OpenTeamDungeonLater(this TeamSceneComponent self, long teamId, long applyId)
        {
            TeamInfo applyingTeam = self.GetTeamInfo(teamId);
            if (applyingTeam == null || applyingTeam.FubenState != (int)TeamDungeonState.Applying)
            {
                return;
            }
            await TimerComponent.Instance.WaitAsync(Math.Max(0, applyingTeam.FubenApplyEndTime - TimeHelper.ServerNow()));
            if (self.IsDisposed)
            {
                return;
            }

            if (!self.OpeningTeams.TryGetValue(teamId, out long currentApplyId) || currentApplyId != applyId)
            {
                return;
            }

            self.OpeningTeams.Remove(teamId);
            TeamInfo teamInfo = self.GetTeamInfo(teamId);
            if (teamInfo == null || teamInfo.TeamId != teamId || teamInfo.FubenState != (int)TeamDungeonState.Applying)
            {
                return;
            }

            if (teamInfo.SceneId == 0 || !LDSceneCategory.Instance.Contain(teamInfo.SceneId))
            {
                return;
            }

            self.CreateTeamDungeon(teamInfo);
            await self.BroadcastDungeonApply(teamInfo);
        }

        /// <summary>向全队同步申请、取消或已开启阶段；当前状态由 FubenState 判断。</summary>
        public static async ETTask BroadcastDungeonApply(this TeamSceneComponent self, TeamInfo teamInfo, bool isCanceled = false)
        {
            // 申请中 FubenInstanceId 仍是 0。ErrorCode 不能填 Success，旧客户端看到 Success 会直接 RequestTransfer。
            int errorCode = isCanceled || teamInfo.FubenInstanceId != 0 ? ErrorCode.ERR_Success : ErrorCode.Err_HaveNotPrepare;
            M2C_TeamDungeonApplyResult message = new M2C_TeamDungeonApplyResult()
            {
                TeamInfo = CloneTeamInfo(teamInfo),
                ErrorCode = errorCode,
                IsCanceled = isCanceled,
            };
            List<TeamPlayerInfo> players = message.TeamInfo.PlayerList;
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].RobotId != 0)
                {
                    continue;
                }

                // 广播过程中阶段可能已变化，停止发送过期快照。
                if (teamInfo.FubenState != message.TeamInfo.FubenState
                    || teamInfo.FubenInstanceId != message.TeamInfo.FubenInstanceId
                    || teamInfo.FubenApplyEndTime != message.TeamInfo.FubenApplyEndTime)
                {
                    return;
                }
                long userId = players[i].UserID;
                await ServerMessageHelper.SendToClient(UnitZoneHelper.GetHomeZone(userId), userId, message);
            }
        }

        public static void OnDungeonOver(this TeamSceneComponent self, long fubenInstanceId)
        {
            // 仅清理仍指向该实例的队伍，旧实例结束不得覆盖新副本。
            foreach (TeamInfo teamInfo in self.TeamList)
            {
                if (teamInfo.FubenInstanceId != fubenInstanceId)
                {
                    continue;
                }
                teamInfo.FubenState = (int)TeamDungeonState.None;
                teamInfo.FubenApplyEndTime = 0;
                teamInfo.FubenUUId = 0;
                teamInfo.FubenInstanceId = 0;
                teamInfo.FubenCreateTime = 0;

                foreach (TeamPlayerInfo player in teamInfo.PlayerList)
                {
                    player.Damage = 0;
                }
                self.SyncTeamInfo(teamInfo, new List<TeamPlayerInfo>(teamInfo.PlayerList)).Coroutine();
                return;
            }
        }

        public static TeamInfo GetTeamInfo(this TeamSceneComponent self, long userId)
        {
            TeamInfo teamInfo = null;
            for (int i = 0; i < self.TeamList.Count; i++)
            {
                TeamInfo tempTeampInfo = self.TeamList[i];
                if (tempTeampInfo.TeamId == userId)
                {
                    teamInfo = tempTeampInfo;
                    break;
                }

                for (int k = tempTeampInfo.PlayerList.Count - 1; k >= 0; k--)
                {
                    if (tempTeampInfo.PlayerList[k].UserID == userId)
                    {
                        teamInfo = tempTeampInfo;
                        break;
                    }
                }
            }
            return teamInfo;
        }

        public static long GetTeamInfoId(this TeamSceneComponent self, long userId)
        {
            TeamInfo teamInfo = self.GetTeamInfo(userId);
            return teamInfo != null ? teamInfo.TeamId : 0;
        }

        public static TeamInfo CreateTeamInfo(this TeamSceneComponent self, TeamPlayerInfo teamPlayerInfo, int fubenId)
        {
            TeamInfo teamInfo = self.GetTeamInfo(teamPlayerInfo.UserID);
            if (teamInfo != null)
            {
                Log.Error($"teamInfo != null {teamPlayerInfo.UserID}");
                return teamInfo;
            }
            teamInfo = new TeamInfo() { TeamId = teamPlayerInfo.UserID, SceneId = fubenId };
            teamInfo.PlayerList.Add(teamPlayerInfo);
            self.TeamList.Add(teamInfo);
            return teamInfo;
        }

        public static async ETTask SyncTeamInfo(this TeamSceneComponent self, TeamInfo teamInfo, List<TeamPlayerInfo> userIds)
        {
            if (userIds == null || userIds.Count == 0)
            {
                return;
            }

            int count = userIds.Count;
            long[] notifyUserIds = new long[count];
            long[] notifyTeamIds = new long[count];
            for (int i = 0; i < count; i++)
            {
                long userId = userIds[i].UserID;
                notifyUserIds[i] = userId;
                notifyTeamIds[i] = self.GetTeamInfoId(userId);
            }

            M2C_TeamUpdateResult m2C_TeamUpdateResult = new M2C_TeamUpdateResult()
            {
                TeamInfo = CloneTeamInfo(teamInfo),
            };

            long teamSceneId = self.DomainScene().InstanceId;
            for (int i = 0; i < count; i++)
            {
                long userId = notifyUserIds[i];
                int homeZone = UnitZoneHelper.GetHomeZone(userId);
                await ServerMessageHelper.SendToClient(homeZone, userId, m2C_TeamUpdateResult);
                MessageHelper.SendToLocationActor(userId, new T2M_TeamUpdateRequest() { TeamId = notifyTeamIds[i] });
                long bindTeamServer = notifyTeamIds[i] == 0 ? 0 : teamSceneId;
                await ServerMessageHelper.SetTeamServer(homeZone, userId, bindTeamServer);
            }
        }

        private static TeamInfo CloneTeamInfo(TeamInfo teamInfo)
        {
            if (teamInfo == null)
            {
                return null;
            }

            TeamInfo clone = new TeamInfo()
            {
                SceneId = teamInfo.SceneId,
                TeamId = teamInfo.TeamId,
                FubenInstanceId = teamInfo.FubenInstanceId,
                FubenUUId = teamInfo.FubenUUId,
                FubenCreateTime = teamInfo.FubenCreateTime,
                FubenState = teamInfo.FubenState,
                FubenApplyEndTime = teamInfo.FubenApplyEndTime,

                FubenType = teamInfo.FubenType,
                LevelMin = teamInfo.LevelMin,
                LevelMax = teamInfo.LevelMax,
                RecruitMsg = teamInfo.RecruitMsg,
            };
            clone.PlayerList.AddRange(teamInfo.PlayerList);
            return clone;
        }

        /// <summary>
        /// 离开队伍。队员离队只移除自己；队长离队且仍有成员则 TeamId 顺位给下一名；没人则解散。
        /// </summary>
        public static  void OnRecvUnitLeave(this TeamSceneComponent self, long userId)
        {
            Log.Debug($"TeamSceneComponent Leave {userId}");

            TeamInfo teamInfo = self.GetTeamInfo(userId);
            if (teamInfo == null)
            {
                return;
            }

            List<TeamPlayerInfo> userIDList = new List<TeamPlayerInfo>();
            userIDList.AddRange(teamInfo.PlayerList);

            for (int k = teamInfo.PlayerList.Count - 1; k >= 0; k--)
            {
                if (teamInfo.PlayerList[k].UserID == userId)
                {
                    teamInfo.PlayerList.RemoveAt(k);
                    break;
                }
            }

            if (teamInfo.PlayerList.Count == 0)
            {
                long teamId = teamInfo.TeamId;
                self.TeamList.Remove(teamInfo);
                self.ClearApply(teamId);
                self.SyncTeamInfo(teamInfo, userIDList).Coroutine();
                return;
            }

            if (teamInfo.TeamId == userId)
            {
                long oldTeamId = teamInfo.TeamId;
                self.OpeningTeams.Remove(oldTeamId);
                if (teamInfo.FubenState == (int)TeamDungeonState.Applying)
                {
                    teamInfo.FubenState = (int)TeamDungeonState.None;
                    teamInfo.FubenApplyEndTime = 0;
                }
                TeamPlayerInfo newLeader = teamInfo.PlayerList[0];
                teamInfo.TeamId = newLeader.UserID;
                newLeader.Followe = 0;

                if (self.ApplyDict.TryGetValue(oldTeamId, out List<TeamPlayerInfo> applyList))
                {
                    self.ApplyDict.Remove(oldTeamId);
                    self.ApplyDict[newLeader.UserID] = applyList;
                }

                TeamPlayerInfo leaver = null;
                for (int i = userIDList.Count - 1; i >= 0; i--)
                {
                    if (userIDList[i].UserID == userId)
                    {
                        leaver = userIDList[i];
                        userIDList.RemoveAt(i);
                        break;
                    }
                }

                if (leaver != null)
                {
                    self.SyncTeamInfo(new TeamInfo() { TeamId = oldTeamId }, new List<TeamPlayerInfo> { leaver }).Coroutine();
                }
            }

            self.SyncTeamInfo(teamInfo, userIDList).Coroutine();
        }

        /// <summary>
        /// 队长解散队伍：清空成员并同步 M2C_TeamUpdateResult。
        /// </summary>
        public static void OnRecvTeamDismiss(this TeamSceneComponent self, long userId)
        {
            TeamInfo teamInfo = self.GetTeamInfo(userId);
            if (teamInfo == null || teamInfo.TeamId != userId)
            {
                return;
            }

            List<TeamPlayerInfo> userIDList = new List<TeamPlayerInfo>();
            userIDList.AddRange(teamInfo.PlayerList);
            long teamId = teamInfo.TeamId;
            teamInfo.PlayerList.Clear();
            self.TeamList.Remove(teamInfo);
            self.ClearApply(teamId);
            self.SyncTeamInfo(teamInfo, userIDList).Coroutine();
        }



        public static bool AddApply(this TeamSceneComponent self, long teamId, TeamPlayerInfo teamPlayerInfo)
        {
            if (teamId == 0 || teamPlayerInfo == null || teamPlayerInfo.UserID == 0)
            {
                return false;
            }

            if (!self.ApplyDict.TryGetValue(teamId, out List<TeamPlayerInfo> applyList))
            {
                applyList = new List<TeamPlayerInfo>();
                self.ApplyDict.Add(teamId, applyList);
            }

            for (int i = 0; i < applyList.Count; i++)
            {
                if (applyList[i].UserID == teamPlayerInfo.UserID)
                {
                    applyList[i] = teamPlayerInfo;
                    return true;
                }
            }

            if (applyList.Count >= TeamSceneComponent.ApplyMaxCount)
            {
                return false;
            }

            applyList.Add(teamPlayerInfo);
            return true;
        }

        public static void RemoveApply(this TeamSceneComponent self, long teamId, long userId)
        {
            if (!self.ApplyDict.TryGetValue(teamId, out List<TeamPlayerInfo> applyList))
            {
                return;
            }

            for (int i = applyList.Count - 1; i >= 0; i--)
            {
                if (applyList[i].UserID == userId)
                {
                    applyList.RemoveAt(i);
                    break;
                }
            }

            if (applyList.Count == 0)
            {
                self.ApplyDict.Remove(teamId);
            }
        }

        public static List<TeamPlayerInfo> GetApplyList(this TeamSceneComponent self, long teamId)
        {
            if (!self.ApplyDict.TryGetValue(teamId, out List<TeamPlayerInfo> applyList) || applyList == null)
            {
                return new List<TeamPlayerInfo>();
            }

            return applyList;
        }

        public static void ClearApply(this TeamSceneComponent self, long teamId)
        {
            self.ApplyDict.Remove(teamId);
            self.OpeningTeams.Remove(teamId);
        }
    }
}
