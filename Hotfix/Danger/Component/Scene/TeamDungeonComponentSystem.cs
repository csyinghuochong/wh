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
                    bool ret = unit.GetComponent<BagComponentServer>().OnAddItemData(rewardItems, string.Empty, $"{ItemGetWay.PickItem}_{serverTime}");
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
            for (int i = 0; i < allPlayer.Count; i++)
            {
                M2C_SyncMiJingDamage m2C_SyncMiJingDamage = new M2C_SyncMiJingDamage();
                m2C_SyncMiJingDamage.DamageList.AddRange(self.TeamInfo.PlayerList);
                MessageHelper.SendToClient(allPlayer[i], m2C_SyncMiJingDamage);
            }
        }

        public static async ETTask CheckFriend(this TeamDungeonComponent self, Dictionary<long,int> hurts)
        {
            List<Unit> players = UnitHelper.GetUnitList( self.DomainScene(), UnitType.Player );
            for (int i = 0; i < players.Count; i++)
            {
                Unit unit = players[i]; 
                if (unit.IsRobot())
                {
                    continue;
                }

                DBFriendInfo dBFriendInfo = await DBHelper.GetComponent<DBFriendInfo>(UnitZoneHelper.GetHomeZone(unit.Id), unit.Id);
                if (dBFriendInfo == null)
                {
                    continue;
                }
                bool haveFriend = false;
                for (  int friend = 0;  friend < dBFriendInfo.FriendList.Count; friend++  )
                {
                    if (hurts.ContainsKey(dBFriendInfo.FriendList[friend]))
                    {
                        haveFriend = true;
                        break;
                    }
                }

                //if (self.DomainZone() == 56 && unit.Id == 2122034524618555392)
                //{
                //    Log.Warning($"与好友完成组队副本： {unit.Id}   {dBFriendInfo.FriendList.Count}");
                //}

                if (haveFriend)
                {
                    unit.GetComponent<TaskComponentServer>().TriggerTaskEvent(TastConditionType.FriendPassFuben_138, 0, 1);
                
                }
            }
            await ETTask.CompletedTask;
        }

        public static void OnKillEvent(this TeamDungeonComponent self, Unit unit)
        {
            if (unit.Type != UnitType.Monster)
            {
                return;
            }

            if (unit.IsBoss())
            {
                Unit leader = unit.GetParent<UnitComponent>().Get(self.TeamInfo.TeamId);
                if (leader != null && self.FubenType == TeamFubenType.XieZhu)
                {
                    //协助副本掉落
                    int dropId = int.Parse(LDGlobalValueCategory.Instance.Get(75).Value);
                    UnitFactory.CreateDropItems(leader, unit, 1, dropId, "1");
                }
                if (self.FubenType == TeamFubenType.ShenYuan)
                {
                    //深渊副本掉落
                    int dropId = int.Parse(LDGlobalValueCategory.Instance.Get(77).Value);
                    UnitFactory.CreateDropItems(null, unit, 0, dropId, "1");
                }

                self.BossDeadPosition = unit.Position;
            }

            LDScene ldScene = LDSceneCategory.Instance.Get(self.TeamInfo.SceneId);
            if (unit.ConfigId != ldScene.GetBossID())
            {
                return;
            }

            M2C_TeamDungeonSettlement m2C_FubenSettlement = new M2C_TeamDungeonSettlement();
            m2C_FubenSettlement.PassTime = 5*60 * 1000;
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


            self.CheckFriend(hurtList).Coroutine();

            //TeamDungeonHurt_136
            UnitComponent unitComponent = unit.GetParent<UnitComponent>();
            List<Unit> units = unitComponent.GetAll();
            for (int i = 0; i < units.Count; i++)
            {
                Unit unititem = units[i] as Unit;
                if (unititem.Type != UnitType.Player || unititem.IsRobot())
                {
                    continue;
                }

                hurtList.TryGetValue(unititem.Id, out int hurtvalue);
                int hurtRate = (int)(hurtvalue * 100f / damageTotal);
                TaskComponentServer taskComponent = unititem.GetComponent<TaskComponentServer>();
                ChengJiuComponentServer chengJiu = unititem.GetComponent<ChengJiuComponentServer>();
                taskComponent.TriggerTaskEvent( TastConditionType.TeamDungeonHurt_136, self.TeamInfo.SceneId, hurtRate);
             
                taskComponent.OnPassTeamFuben();
                chengJiu.TriggerEvent(ChengJiuTargetEnum.PassTeamFubenNumber_20, 0, 1);
                if (self.FubenType == TeamFubenType.ShenYuan)
                {
                    chengJiu.TriggerEvent(ChengJiuTargetEnum.PassTeamShenYuanNumber_21, 0, 1);
                }
                RoleInfoComponentServer roleInfoComponentServer = unititem.GetComponent<RoleInfoComponentServer>();
                if (roleInfoComponentServer.RoleInfo.UserId == idExtra && m2C_FubenSettlement.RewardExtraItem.Count > 0)
                {
                }
                MessageHelper.SendToClient(unititem, m2C_FubenSettlement);
            }

            (self.Parent.Parent as TeamSceneComponent).OnDungeonOver(self.TeamInfo.TeamId);
        }
    }
}
