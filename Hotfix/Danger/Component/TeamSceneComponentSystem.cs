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
            teamDungeonComponent.TeamInfo = teamInfo;
            teamDungeonComponent.EnterTime = TimeHelper.ServerNow();
            teamDungeonComponent.FubenType = teamInfo.FubenType;
            teamDungeonComponent.BossDeadPosition = new Vector3((float)ldScene.Pos_Born[0] , (float)ldScene.Pos_Born[1] , (float)ldScene.Pos_Born[2] );
            teamDungeonComponent.InitPlayerList();
            teamInfo.FubenInstanceId = fubenInstanceId;
            teamInfo.FubenUUId = fubenid;
            Game.Scene.GetComponent<RecastPathComponent>().Update(mapComponent.NavMeshId);
        
            if (teamInfo.FubenType == TeamFubenType.ShenYuan)
            {
                /*if (CommonConfig.ShenYuanCreateConfig.ContainsKey(teamInfo.SceneId))
                {
                    int postionid = CommonConfig.ShenYuanCreateConfig[teamInfo.SceneId];
                    FubenHelp.CreateMonsterByPos(fubnescene, postionid);
                }
                else
                {
                    Console.WriteLine($"ConfigHelper.ShenYuanCreateConfig.error: {teamInfo.SceneId}");
                }*/
            }

            //69和 72的2个副本 有10%概率 在每个BOSS附近刷新出 80002010 这个宝箱，
            //这个宝箱的掉落是只有自己的可以拾取的，并且宝箱和掉落只有自己可见，
            //在聊天广播中提示要加入XXX玩家通过XXX宝箱拾取XXX,而不是之前的那个通用的广播

            TransferHelper.NoticeFubenCenter(fubnescene, 1).Coroutine();
        }

        public static void OnDungeonOver(this TeamSceneComponent self, long teamId)
        {
            TeamInfo teamInfo = self.GetTeamInfo(teamId);
            if (teamInfo != null)
            {
                for (int i = 0;i < teamInfo.PlayerList.Count; i++)
                {
                    teamInfo.PlayerList[i].Damage = 0;
                }
                teamInfo.FubenUUId = 0;
                teamInfo.FubenInstanceId = 0;
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
            M2C_TeamUpdateResult m2C_HorseNoticeInfo = self.m2C_TeamUpdateResult;
            m2C_HorseNoticeInfo.TeamInfo = teamInfo;
            T2M_TeamUpdateRequest t2M_TeamUpdateRequest = self.t2M_TeamUpdateRequest;

            long gateServerId = DBHelper.GetGateServerId(self.DomainZone());
            for (int i = 0; i < userIds.Count; i++)
            {
                long userId = userIds[i].UserID;
                G2T_GateUnitInfoResponse g2M_UpdateUnitResponse = (G2T_GateUnitInfoResponse)await ActorMessageSenderComponent.Instance.Call
                    (gateServerId, new T2G_GateUnitInfoRequest()
                    {
                        UserID = userId
                    });

                if (g2M_UpdateUnitResponse.PlayerState == (int)PlayerState.Game && g2M_UpdateUnitResponse.SessionInstanceId > 0)
                {
                    t2M_TeamUpdateRequest.TeamId = self.GetTeamInfoId(userId);
                    MessageHelper.SendActor(g2M_UpdateUnitResponse.SessionInstanceId, m2C_HorseNoticeInfo);
                    MessageHelper.SendToLocationActor(userId, t2M_TeamUpdateRequest);
                }
            }
        }

        /// <summary>
        /// 离开队伍
        /// </summary>
        /// <param name="self"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static  void OnRecvUnitLeave(this TeamSceneComponent self, long userId, bool exitgame = false)
        {
            Log.Debug($"TeamSceneComponent Leave {userId} {exitgame}");

            if (self.DomainZone() == 5)
            {
                //Console.WriteLine($"TeamSceneComponent.OnRecvUnitLeave:  {userId}");
            }

            TeamInfo teamInfo = self.GetTeamInfo(userId);
            if (teamInfo == null)
            {
                return;
            }
            //玩家Id
            List<TeamPlayerInfo> userIDList = new List<TeamPlayerInfo>();
            userIDList.AddRange(teamInfo.PlayerList);
            for (int i = userIDList.Count - 1; i >= 0; i--)
            {
                if (exitgame && userIDList[i].UserID == userId)
                {
                    userIDList.RemoveAt(i);
                }
            }

            for (int k = teamInfo.PlayerList.Count - 1; k >= 0; k--)
            {
                if (teamInfo.PlayerList[k].UserID == userId)
                {
                    teamInfo.PlayerList.RemoveAt(k);
                    break;
                }
            }

            if (teamInfo.PlayerList.Count == 0 || teamInfo.TeamId == userId)
            {
                teamInfo.PlayerList.Clear();   //队伍解算
                self.TeamList.Remove(teamInfo);
            }

            self.SyncTeamInfo(teamInfo, userIDList).Coroutine();
        }

        /// <summary>
        /// 组队副本返回主城
        /// </summary>
        /// <param name="self"></param>
        /// <param name="unitId"></param>
        /// <returns></returns>
        public static void  OnUnitReturn(this TeamSceneComponent self, Scene fubnescene, long unitId)
        {
            int realPlayerNumber = 0;
            int robotNumber = 0;
            TeamInfo teamInfo = self.GetTeamInfo(unitId);

            List<Unit> allunits = UnitHelper.GetUnitList(fubnescene, UnitType.Player);

            for (int i = 0; i < allunits.Count; i++)
            {
                if (allunits[i].GetComponent<RoleInfoComponent>().UserInfo.RobotId == 0)
                {
                    realPlayerNumber++;
                    continue;
                }
                if (teamInfo != null && unitId == teamInfo.TeamId)
                {
                    robotNumber++;
                    MessageHelper.SendToClient(allunits[i], self.M2C_TeamDungeonQuitMessage);
                }
            }

            //self.DomainZone() == 5 &&
            //队长中途退出并且还有其他玩家才解算退伍。。。。
            if ( teamInfo != null && teamInfo.FubenInstanceId > 0 && unitId == teamInfo.TeamId && realPlayerNumber >= 1)
            {
                Console.WriteLine($"TeamSceneComponent.OnUnitReturn [队长离开 解算队伍！]");
                
                if (teamInfo != null)
                {
                    List<TeamPlayerInfo> userIDList = new List<TeamPlayerInfo>();
                    userIDList.AddRange(teamInfo.PlayerList);
                    self.SyncTeamInfo(teamInfo, userIDList).Coroutine();

                    teamInfo.PlayerList.Clear();   //队伍解算
                    self.TeamList.Remove(teamInfo);
                }
            }

            if (allunits.Count > 0)
            {
                return;
            }
            self.OnDungeonOver(unitId);
            TeamDungeonComponent teamDungeonComponent = fubnescene.GetComponent<TeamDungeonComponent>();
            Log.Debug($"TeamDungeonDispose {teamDungeonComponent.TeamInfo.TeamId}{fubnescene.InstanceId}");
            TransferHelper.NoticeFubenCenter(fubnescene, 2).Coroutine();
            fubnescene.Dispose();
        }

        /// <summary>
        /// 玩家离线， unit已经移除了
        /// </summary>
        /// <param name="self"></param>
        /// <param name="unitId"></param>
        /// <returns></returns>
        public static void  OnUnitDisconnect(this TeamSceneComponent self, Scene fubnescene, long unitId)
        {
            TeamDungeonComponent teamDungeonComponent = fubnescene.GetComponent<TeamDungeonComponent>();
            TeamInfo teamInfo = teamDungeonComponent.TeamInfo;
            if (teamDungeonComponent.IsHavePlayer())
            {
                return;
            }
            self.OnDungeonOver(teamInfo.TeamId);
            
            Log.Debug($"TeamDungeonDispose {teamDungeonComponent.TeamInfo.TeamId}{fubnescene.InstanceId}");
            TransferHelper.NoticeFubenCenter(fubnescene, 2).Coroutine();
            fubnescene.Dispose();
        }
    }
}
