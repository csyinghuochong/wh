using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{
    [Timer(TimerType.TeamDropTimer)]
    public class TeamDropTimer : ATimer<TeamDungeonComponent>
    {
        public override void Run(TeamDungeonComponent self)
        {
            try
            {
                self.Check();
            }
            catch (Exception e)
            {
                Log.Error($"move timer error: {self.Id}\n{e}");
            }
        }
    }

    [ObjectSystem]
    public class TeamDungeonComponentAwakeSystem : AwakeSystem<TeamDungeonComponent>
    {
        public override void Awake(TeamDungeonComponent self)
        {
            self.EnterTime = 0;
            self.TeamInfo = null;
            self.IsAbandoning = false;
            self.IsCompleted = false;
            self.EnterPlayers.Clear();
            self.BoxReward.Clear();
            self.ItemFlags.Clear();
            self.TeamDropItems.Clear();
            self.KillBossList.Clear();
            self.TeamPlayers.Clear();
        }
    }

    [ObjectSystem]
    public class TeamDungeonComponentDestroySystem : DestroySystem<TeamDungeonComponent>
    {
        public override void Destroy(TeamDungeonComponent self)
        {
            TimerComponent.Instance.Remove(ref self.Timer);
        }
    }

    public static class TeamDungeonComponentSystem
    {
        private const int TeamDungeonStartMarqueeId = 101;
        private const int TeamDungeonEndMarqueeId = 102;

        /// <summary>放弃当前实例：停止新进入，全部玩家回主城后统一清理副本，保留队伍。</summary>
        public static async ETTask<int> Abandon(this TeamDungeonComponent self)
        {
            if (self.IsAbandoning)
            {
                return ErrorCode.ERR_RequestRepeatedly;
            }

            Scene scene = self.DomainScene();
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            self.IsAbandoning = true;
            try
            {
                // 已开始分片传送的玩家仍须接收落地，避免销毁正在接收数据的实例。
                long waitUntil = TimeHelper.ServerNow() + 10 * TimeHelper.Second;
                while (HasPendingTransfers(unitComponent))
                {
                    if (TimeHelper.ServerNow() >= waitUntil)
                    {
                        return ErrorCode.ERR_TransferFailError;
                    }

                    await TimerComponent.Instance.WaitFrameAsync();
                }

                List<long> playerIds = UnitHelper.GetUnitList(scene, UnitType.Player).Select(player => player.Id).ToList();
                int transferError = ErrorCode.ERR_Success;
                for (int i = 0; i < playerIds.Count; i++)
                {
                    long playerId = playerIds[i];
                    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Transfer, playerId, 0))
                    {
                        // 等锁期间玩家可能已正常回城或断线，重新从本实例取玩家。
                        Unit player = unitComponent.Get(playerId);
                        if (player == null)
                        {
                            continue;
                        }

                        int error = await TransferHelper.MainCityTransfer(player);
                        if (error != ErrorCode.ERR_Success)
                        {
                            transferError = error;
                        }
                    }
                }

                if (transferError != ErrorCode.ERR_Success)
                {
                    return transferError;
                }

                if (self.IsHavePlayer() || HasPendingTransfers(unitComponent))
                {
                    return ErrorCode.ERR_TransferFailError;
                }


                if (!self.IsCompleted)
                {
                    self.IsCompleted = true;
                    self.NotifyDungeonEndMarquee();
                }

                scene.GetParent<TeamSceneComponent>().OnDungeonOver(scene.InstanceId);
                TransferHelper.NoticeFubenCenter(scene, 2).Coroutine();
                scene.Dispose();
                return ErrorCode.ERR_Success;
            }
            finally
            {
                // 传送失败时保留副本和未离开的玩家，允许再次发起放弃。
                if (!self.IsDisposed)
                {
                    self.IsAbandoning = false;
                }
            }
        }

        /// <summary>校验当前队伍、实例、通关/关闭和到期；未通关时允许同实例再次进入。</summary>
        public static int CheckEnter(this TeamDungeonComponent self, long userId)
        {
            Scene scene = self.DomainScene();
            TeamInfo teamInfo = scene.GetParent<TeamSceneComponent>().GetTeamInfo(userId);
            if (teamInfo == null)
            {
                return ErrorCode.Err_TeamIsNull;
            }
            int limitTime = LDSceneCategory.Instance.Get(self.TeamInfo.SceneId).Limit_Time;
            if (self.IsAbandoning || self.IsCompleted || teamInfo.FubenState != (int)TeamDungeonState.Opened
                || teamInfo.FubenInstanceId != scene.InstanceId
                || (limitTime > 0 && TimeHelper.ServerNow() >= self.TeamInfo.FubenCreateTime + (long)limitTime * TimeHelper.Second))
            {
                return ErrorCode.ERR_AlreadyFinish;
            }
            return self.CheckBattleEnter(userId);
        }

        /// <summary>预留实际交战时的进入限制；当前不限制，后续可返回 ERR_TeamDungeonInBattle。</summary>
        public static int CheckBattleEnter(this TeamDungeonComponent self, long userId)
        {
            return ErrorCode.ERR_Success;
        }

        /// <summary>新增进入者使用独立信息，避免与队伍及后续副本共享伤害数据。</summary>
        public static void RegisterPlayer(this TeamDungeonComponent self, long userId)
        {
            if (self.TeamPlayers.ContainsKey(userId))
            {
                return;
            }
            TeamInfo currentTeam = self.DomainScene().GetParent<TeamSceneComponent>().GetTeamInfo(userId);
            foreach (TeamPlayerInfo player in currentTeam.PlayerList)
            {
                if (player.UserID != userId)
                {
                    continue;
                }
                TeamPlayerInfo snapshot = MongoHelper.Clone(player);
                snapshot.Damage = 0;
                self.TeamInfo.PlayerList.Add(snapshot);
                self.TeamPlayers.Add(userId, snapshot);
                return;
            }
        }

        private static bool HasPendingTransfers(UnitComponent unitComponent)
        {
            return unitComponent.UnitComponents.Count > 0;
        }

        private static TeamDropItem FindTeamDropItem(this TeamDungeonComponent self, long dropId)
        {
            for (int i = self.TeamDropItems.Count - 1; i >= 0; i--)
            {
                if (self.TeamDropItems[i].DropInfo.UnitId == dropId)
                {
                    return self.TeamDropItems[i];
                }
            }
            return null;
        }

        public static void InitPlayerList(this TeamDungeonComponent self)
        {
            for (int i = 0; i < self.TeamInfo.PlayerList.Count; i++)
            {
                self.TeamInfo.PlayerList[i].Damage = 0;
                if (!self.TeamPlayers.TryAdd(self.TeamInfo.PlayerList[i].UserID, self.TeamInfo.PlayerList[i]))
                {
                    Log.Warning($"InitPlayerList.Error: {self.TeamInfo.PlayerList[i].UserID}");
                }
            }
        }

        public static int InitPlayerNumber(this TeamDungeonComponent self)
        {
            int InitPlayerNumber = 0;
            for (int i = 0; i < self.TeamInfo.PlayerList.Count; i++)
            {
                InitPlayerNumber += (self.TeamInfo.PlayerList[i].RobotId == 0 ? 1 : 0);
            }
            return InitPlayerNumber;
        }

        public static void Check(this TeamDungeonComponent self)
        {
            if (self.IsAbandoning)
            {
                return;
            }

            // 使用当前实例自己的创建时间，队伍申请新副本不改变旧实例期限。
            int limitTime = LDSceneCategory.Instance.Get(self.TeamInfo.SceneId).Limit_Time;
            if (limitTime > 0 && TimeHelper.ServerNow() >= self.TeamInfo.FubenCreateTime + (long)limitTime * TimeHelper.Second)
            {
                // 关闭失败时下一次 Tick 重试，关闭期间拒绝新传送。
                self.Abandon().Coroutine();
                return;
            }
            if (self.TeamDropItems.Count == 0)
            {
                //TimerComponent.Instance.Remove(ref self.Timer);
                return;
            }
            long serverTime = TimeHelper.ServerNow();
            List<Unit> allunits = UnitHelper.GetUnitList(self.DomainScene(), UnitType.Player);
            int playerCount = allunits.Count;
            UnitComponent unitComponent = self.DomainScene().GetComponent<UnitComponent>();
            for (int i = self.TeamDropItems.Count - 1; i >= 0; i--)
            {
                TeamDropItem teamDropItem = self.TeamDropItems[i];
                if (teamDropItem.EndTime == -1)   // || teamDropItem.AllGive)
                {
                    continue;
                }
                List<long> needIds = teamDropItem.NeedPlayers;
                List<long> giveIds = teamDropItem.GivePlayers;
                bool finish = serverTime >= teamDropItem.EndTime || (needIds.Count + giveIds.Count >= playerCount);
                if (!finish)
                {
                    continue;
                }
                teamDropItem.EndTime = -1;
                if (playerCount == 0)
                {
                    Log.Warning($"TeamDungeonComponent.DropInfo：playerCount == 0");
                    //self.DomainScene().GetComponent<UnitComponent>().Remove(teamDropItem.DropInfo.UnitId);   
                    continue;
                }

                //全部放弃 则 可以自由拾取。
                if (giveIds.Count >= playerCount || needIds.Count == 0)
                {
                    //for (int allunit = 0; allunit < allunits.Count; allunit++)
                    //{
                    //    needIds.Add(allunits[allunit].Id);
                    //}
                    //teamDropItem.AllGive = true;
                    continue;
                }
                
                int maxNumber = 0;
                List<int> randomNumbers = new List<int>();
                for (int p = 0; p < needIds.Count; p++)
                {
                    randomNumbers.Add(RandomHelper.RandomNumber(1, 100));
                    if (randomNumbers[p] > maxNumber)
                    {
                        maxNumber = randomNumbers[p];
                    }
                }
                int onwerIndex = randomNumbers.IndexOf(maxNumber);
                long unitid =  teamDropItem.NeedPlayers[onwerIndex];
                Unit unit = unitComponent.Get(unitid);
                if (unit != null)
                {
                    List<RewardItem> rewardItems = new List<RewardItem>();
                    rewardItems.Add(new RewardItem() { ItemID = teamDropItem.DropInfo.ItemID, ItemNum = teamDropItem.DropInfo.ItemNum });
                    bool ret = unit.GetComponent<BagComponentServer>().OnAddItemData(rewardItems, $"{ItemGetWay.PickItem}_{serverTime}");
                    //Log.Warning($"TeamDungeonComponent.DropInfo：{ret}  {unit.Id} {teamDropItem.DropInfo.ItemID} {teamDropItem.DropInfo.ItemNum}");
                    if (ret)
                    {
                        SceneCreatureHelp.SendTeamPickMessage(unit, teamDropItem.DropInfo, needIds, randomNumbers);
                        unitComponent.Remove(teamDropItem.DropInfo.UnitId);       //移除掉落ID
                        continue;
                    }

                    self.ItemFlags.TryAdd(teamDropItem.DropInfo.UnitId, unitid);
                }
                else
                {
                    Log.Warning($"TeamDungeonComponent.DropInfo：unit == null");
                    //self.DomainScene().GetComponent<UnitComponent>().Remove(teamDropItem.DropInfo.UnitId);       //移除掉落ID
                }
            }
        }

        public static bool IsAllGiveDrop(this TeamDungeonComponent self, long dropId)
        {
            TeamDropItem teamDropItem = self.FindTeamDropItem(dropId);
            return teamDropItem != null && teamDropItem.EndTime == -1;
        }

        public static bool IsInTeamDrop(this TeamDungeonComponent self, long dropId)
        {
            return self.FindTeamDropItem(dropId) != null;
        }

        public static TeamDropItem GetTeamDropItem(this TeamDungeonComponent self, long dropId)
        {
            return self.FindTeamDropItem(dropId);
        }

        public static TeamDropItem AddTeamDropItem(this TeamDungeonComponent self,  DropInfo dropInfo)
        {
            for (int i = self.TeamDropItems.Count - 1; i >=  0; i--)
            {
                if (self.TeamDropItems[i].EndTime == -1)
                {
                    //Log.Warning($"self.TeamDropItems[i].EndTime == -1:   {dropInfo.ItemID}");
                    self.TeamDropItems.RemoveAt(i);
                    continue;
                }
                if (self.TeamDropItems[i].DropInfo.UnitId == dropInfo.UnitId)
                {
                    self.Check();
                    return null;
                }
            }

            TeamDropItem teamDropItem = null;
            if (self.GetChild<TeamDropItem>(dropInfo.UnitId) != null)
            {
                teamDropItem = self.GetChild<TeamDropItem>(dropInfo.UnitId);
            }
            else
            {
                teamDropItem = self.AddChildWithId<TeamDropItem>(dropInfo.UnitId);
            }

            teamDropItem.DropInfo = dropInfo;
            teamDropItem.EndTime = TimeHelper.ServerNow() + 60 * 1000;

            self.TeamDropItems.Add(teamDropItem);
            if (self.Timer == 0)
            {
                self.Timer = TimerComponent.Instance.NewRepeatedTimer(1000, TimerType.TeamDropTimer, self);
            }
            M2C_TeamPickMessage teamPickMessage = self.m2C_TeamPickMessage;
            teamPickMessage.DropItems.Clear();
            teamPickMessage.DropItems.Add(dropInfo);

            List<Unit> players = UnitHelper.GetUnitList(self.DomainScene(),UnitType.Player);
            MessageHelper.SendToClient(players, teamPickMessage);
            return teamDropItem;
        }

        public static bool IsHavePlayer(this TeamDungeonComponent self)
        {
            UnitComponent unitComponent = self.DomainScene().GetComponent<UnitComponent>();
            foreach (Entity entity in unitComponent.Children.Values)
            {
                if (entity is Unit unit && unit.Type == UnitType.Player)
                {
                    return true;
                }
            }
            return false;
        }

        public static void OnUpdateDamage(this TeamDungeonComponent self, Unit attack, Unit defend, long damage)
        {
            if (defend.Type != UnitType.Monster)
            {
                return;
            }

            List<TeamPlayerInfo> vs = self.TeamInfo.PlayerList;
            for (int i = 0; i < vs.Count; i++)
            {
                if (vs[i].UserID == attack.Id)
                {
                    vs[i].Damage += (int)damage;
                }
            }
           
            if (TimeHelper.ServerNow() - self.EnterTime < 5000)
            {
                return;
            }
            self.EnterTime = TimeHelper.ServerNow();
            List<Unit> allPlayer = UnitHelper.GetUnitList(self.DomainScene(), UnitType.Player);
        }

        /// <summary>怪物死亡后检查场景内全部怪物；尸体未移除不影响通关，仍存活的怪物必须全部清除。</summary>
        public static void OnKillEvent(this TeamDungeonComponent self, Unit unit)
        {
            if (unit.Type != UnitType.Monster || self.IsAbandoning || self.IsCompleted)
            {
                return;
            }

            if (unit.IsBoss())
            {
                self.BossDeadPosition = unit.Position;
            }

            foreach (Unit monster in self.DomainScene().GetComponent<UnitComponent>().GetAll())
            {
                if (monster.Type == UnitType.Monster
                    && monster.GetComponent<NumericComponent>().GetAsInt(NumericType.Now_Dead) != 1)
                {
                    return;
                }
            }

            self.OnDungeonOver();
        }

        public static void OnDungeonOver(this TeamDungeonComponent self)
        {
            if (self.IsAbandoning || self.IsCompleted)
            {
                return;
            }
            self.IsCompleted = true;

            M2C_TeamDungeonSettlement m2C_FubenSettlement = new M2C_TeamDungeonSettlement();
            m2C_FubenSettlement.PassTime = TimeHelper.ServerNow() - self.TeamInfo.FubenCreateTime;
            m2C_FubenSettlement.PlayerList = self.TeamInfo.PlayerList;

            //最高伤害额外奖励
            long idExtra = 0;
            int damageMax = 0;
            int damageTotal = 0;
            Dictionary<long, int> hurtList = new Dictionary<long, int>();
            for (int i = 0; i < m2C_FubenSettlement.PlayerList.Count; i++)
            {
                TeamPlayerInfo teamPlayerInfo = m2C_FubenSettlement.PlayerList[i];
                if (teamPlayerInfo.Damage >= damageMax)
                {
                    damageMax = teamPlayerInfo.Damage;
                    idExtra = teamPlayerInfo.UserID;
                }
                damageTotal += teamPlayerInfo.Damage;
                hurtList.TryAdd(teamPlayerInfo.UserID, teamPlayerInfo.Damage);
            }


            //TeamDungeonHurt_136
            UnitComponent unitComponent = self.DomainScene().GetComponent<UnitComponent>();
            List<Unit> units = unitComponent.GetAll();
            for (int i = 0; i < units.Count; i++)
            {
                Unit unititem = units[i] as Unit;
                if (unititem.Type != UnitType.Player || unititem.IsRobot())
                {
                    continue;
                }

                hurtList.TryGetValue(unititem.Id, out int hurtvalue);
                int hurtRate = 0;
                if (damageTotal > 0)
                {
                    hurtRate = (int)(hurtvalue * 100f / damageTotal);
                }
                DungeonSettlementHelper.SettleTeamDungeon(
                    unititem, self.TeamInfo.SceneId, hurtRate);
                RoleInfoComponentServer roleInfoComponentServer = unititem.GetComponent<RoleInfoComponentServer>();
                if (roleInfoComponentServer.RoleInfo.UserId == idExtra && m2C_FubenSettlement.RewardExtraItem.Count > 0)
                {
                }
                MessageHelper.SendToClient(unititem, m2C_FubenSettlement);
            }

            self.NotifyDungeonEndMarquee();
            self.DomainScene().GetParent<TeamSceneComponent>().OnDungeonOver(self.DomainScene().InstanceId);
        }

        public static void NotifyDungeonStartMarquee(this TeamDungeonComponent self)
        {
            self.NotifyTeamMarquee(TeamDungeonStartMarqueeId);
        }

        public static void NotifyDungeonEndMarquee(this TeamDungeonComponent self)
        {
            self.NotifyTeamMarquee(TeamDungeonEndMarqueeId);
        }

        private static void NotifyTeamMarquee(this TeamDungeonComponent self, int marqueeId)
        {
            TeamInfo teamInfo = self.TeamInfo;
            if (teamInfo == null)
            {
                return;
            }

            string sceneName = string.Empty;
            int nameId = 0;
            if (LDSceneCategory.Instance.Contain(teamInfo.SceneId))
            {
                nameId = LDSceneCategory.Instance.Get(teamInfo.SceneId).Name;
            }

            if (nameId != 0 && LDWordCategory.Instance.Contain(nameId))
            {
                sceneName = LDWordCategory.Instance.Get(nameId).CN ?? string.Empty;
            }

            M2C_MarqueeMessage message = new M2C_MarqueeMessage()
            {
                MarqueeInfo = new MarqueeInfo()
                {
                    MarqueeId = marqueeId,
                    StartTime = TimeHelper.ServerNow(),
                },
            };
            message.MarqueeInfo.ParamList.Add(sceneName);
            TeamInfo recipients = teamInfo;
            foreach (TeamInfo current in self.DomainScene().GetParent<TeamSceneComponent>().TeamList)
            {
                if (current.FubenInstanceId == self.DomainScene().InstanceId)
                {
                    recipients = current;
                    break;
                }
            }
            SendMarqueeToTeam(recipients, message).Coroutine();
        }

        private static async ETTask SendMarqueeToTeam(TeamInfo teamInfo, M2C_MarqueeMessage message)
        {
            List<TeamPlayerInfo> players = teamInfo.PlayerList;
            int count = players.Count;
            long[] userIds = new long[count];
            int realCount = 0;
            for (int i = 0; i < count; i++)
            {
                TeamPlayerInfo player = players[i];
                if (player.RobotId != 0)
                {
                    continue;
                }

                userIds[realCount] = player.UserID;
                realCount++;
            }

            for (int i = 0; i < realCount; i++)
            {
                int homeZone = UnitZoneHelper.GetHomeZone(userIds[i]);
                await ServerMessageHelper.SendToClient(homeZone, userIds[i], message);
            }
        }
    }
}
