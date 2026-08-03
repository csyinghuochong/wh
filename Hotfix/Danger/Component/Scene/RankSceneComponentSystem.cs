using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{
    [Timer(TimerType.RankeTimer)]
    public class RankeTimer : ATimer<RankSceneComponent>
    {
        public override void Run(RankSceneComponent self)
        {
            try
            {
                self.SaveDB().Coroutine();
            }
            catch (Exception e)
            {
                Log.Error($"move timer error: {self.Id}\n{e}");
            }
        }
    }

    [ObjectSystem]
    public class RankSceneComponentAwakeSystem : AwakeSystem<RankSceneComponent>
    {
        public override void Awake(RankSceneComponent self)
        {
            self.InitServerInfo().Coroutine();
            self.InitDBRankInfo().Coroutine();


            long dbTime = TimeHelper.Minute * 30 + RandomHelper.RandomNumber(1000, 10000);
            if (CommonHelper.IsInnerNet())
            {
                dbTime = TimeHelper.Minute;
            }
            self.Timer = TimerComponent.Instance.NewRepeatedTimer(dbTime, TimerType.RankeTimer, self);
        }
    }


    [ObjectSystem]
    public class RankSceneComponentDestroySystem : DestroySystem<RankSceneComponent>
    {

        public override void Destroy(RankSceneComponent self)
        { 
            TimerComponent.Instance.Remove(ref self.Timer);
        }
    }

    public static class RankSceneComponentSystem
    {
        private static void AddRankMailRewardItems(MailInfo mailInfo, string reward, string getWay, Dictionary<string, List<RewardItem>> rewardCache)
        {
            if (string.IsNullOrEmpty(reward))
            {
                return;
            }

            if (!rewardCache.TryGetValue(reward, out List<RewardItem> rewardItems))
            {
                rewardItems = ItemNewHelper.GetRewardItemsAtSemicolon(reward);
                rewardCache.Add(reward, rewardItems);
            }

            for (int i = 0; i < rewardItems.Count; i++)
            {
                RewardItem rewardItem = rewardItems[i];
                mailInfo.ItemList.Add(new BagInfo()
                {
                    ItemID = rewardItem.ItemID,
                    ItemNum = rewardItem.ItemNum,
                    GetWay = getWay
                });
            }
        }

        public static async ETTask InitServerInfo(this RankSceneComponent self)
        {
            await TimerComponent.Instance.WaitAsync(TimeHelper.Second);

            // 战区共享 Rank：不做单服开服天数/世界等级逻辑（无 ServerItem、无本服 FubenCenter）
            if (StartZoneConfigCategory.Instance.IsWarShareZone(self.DomainZone()))
            {
                DBServerInfo warDbInfo = await DBHelper.GetComponent<DBServerInfo>(self.DomainZone(), self.DomainZone());
                if (warDbInfo == null)
                {
                    warDbInfo = new DBServerInfo();
                    warDbInfo.Id = self.DomainZone();
                }
                self.DBServerInfo = warDbInfo;
                Log.Console($"[WarZoneRank] Init skip WorldLv/OpenDay, zone={self.DomainZone()}");
                return;
            }

            DBServerInfo dBServerInfo = await DBHelper.GetComponent<DBServerInfo>(self.DomainZone(), self.DomainZone());
            if (dBServerInfo == null)
            {
                dBServerInfo = new DBServerInfo();
                dBServerInfo.Id = self.DomainZone();
            }
            //初始化参数
            self.DBServerInfo = dBServerInfo;
            self.UpdateExchangeGold(DBHelper.GetOpenServerDay(self.DomainZone()));
            //上午重启不刷新世界等级
            DateTime dateTime = TimeHelper.DateTimeNow();
            if (self.DBServerInfo.ServerInfo.WorldLv == 0|| dateTime.Hour >= 12 || CommonHelper.IsInnerNet())
            {
                self.UpdateWorldLv();
            }
            self.BroadcastWorldLv(1).Coroutine();
        }

        public static void UpdateWorldLv(this RankSceneComponent self)
        {
            //第二天并且超过12点才刷新
            int openserverDay = DBHelper.GetOpenServerDay(self.DomainZone());
            int worldLv = WorldLvHelper.GetWorldLv(openserverDay);
            self.DBServerInfo.ServerInfo.WorldLv = worldLv;
            Log.Debug($"UpdateWorldLv: {self.DomainZone()} {worldLv}");
        }

        public static async ETTask BroadcastWorldLv(this RankSceneComponent self, int updatetype = 0)
        {           
            //延迟刷新，以免有些服务器还没启动
            await TimerComponent.Instance.WaitAsync(RandomHelper.RandomNumber(500, 1000));
            long fubenCenterId = DBHelper.GetFubenCenterId(self.DomainZone());
         
            foreach (StartProcessConfig listprogress in StartProcessConfigCategory.Instance.GetAll().Values)
            {
                List<StartSceneConfig> processScenes = StartSceneConfigCategory.Instance.GetByProcess(listprogress.Id);
                if (processScenes.Count == 0 || listprogress.Id == CommonConfig.RobotProgress)  //机器人进程
                {
                    continue;
                }

                StartSceneConfig startSceneConfig = processScenes[0];
                long mapInstanceId = StartSceneConfigCategory.Instance.GetBySceneName(startSceneConfig.Zone, startSceneConfig.Name).InstanceId;
                A2R_Broadcast createUnit = (A2R_Broadcast)await ActorMessageSenderComponent.Instance.Call(
                    mapInstanceId, new R2A_Broadcast() { LoadType = 2, LoadValue = self.DomainZone().ToString(), ServerInfo = self.DBServerInfo.ServerInfo });
            }
        }

        //3287042516137869312  半心心心
        //未来
        public static void ClearRankingTrialById(this RankSceneComponent self, long unitid)
        {
            DateTime dateTime = TimeHelper.DateTimeNow();
            for (int i = self.DBRankInfo.rankingTrial.Count - 1; i >= 0; i--)
            {
                if (self.DBRankInfo.rankingTrial[i].KeyId == unitid)
                {
                    self.DBRankInfo.rankingTrial.RemoveAt(i);
                }
            }
        }

        public static void ClearRankingTrial(this RankSceneComponent self)
        {
            DateTime dateTime = TimeHelper.DateTimeNow();
            if ((self.DomainZone() == 190) && dateTime.Year == 26 && dateTime.Month == 12 && dateTime.Day == 30)
            {
                self.DBRankInfo.rankingTrial.Clear();
                Log.Warning("self.DBRankInfo.rankingTrial.Clear");
            }
        }

        public static void OnHour12Update(this RankSceneComponent self)
        {
            DateTime dateTime = TimeHelper.DateTimeNow();
            
            self.UpdateWorldLv();
            self.BroadcastWorldLv().Coroutine();
        }

        public static void OnZeroClockUpdate(this RankSceneComponent self)
        {
            //Console.WriteLine($"RankSceneComponent.OnZeroClockUpdate:  {self.DomainZone()} {TimeInfo.Instance.ToDateTime(TimeHelper.ServerNow())}");

            //更新服务器拍卖行数据
            //TimeHelper. self.OpenServiceTime
            self.UpdateExchangeGold(DBHelper.GetOpenServerDay(self.DomainZone()));
            self.SendCombatReward().Coroutine();
            self.SendPetReward().Coroutine();
            self.SendTrialReward().Coroutine();
            self.SendSeasonTowerReward().Coroutine();
            self.BroadcastWorldLv(1).Coroutine();

            self.DBRankInfo.rankShowLie.Clear();
            self.DBRankInfo.rankUnionRace.Clear();
        }

        //更新兑换金币
        public static void UpdateExchangeGold(this RankSceneComponent self, int dayTime)
        {
            int duihuan_baseGold = 1500;       //基础兑换值
            float duihuanPro = 0.05f;
            //最多计算20天后的物价
            if (dayTime > 30)
            {
                dayTime = 30;
            }

            //计算物价
            int duihuanDay = dayTime;
            if (duihuanDay >= 30) {
                duihuanDay = 30;
            }
            duihuanPro = duihuanPro * duihuanDay;
            /*
            if (dayTime > 0 && dayTime <= 7)
            {
                duihuanPro = duihuanPro * dayTime;
            }

            //计算物价
            if (dayTime > 7 && dayTime <= 18)
            {
                duihuanPro = 7 * 0.05f + (dayTime - 7) * 0.1f;
            }

            //计算物价
            if (dayTime > 18)
            {
                duihuanPro = 7 * 0.15f + 11 * 0.1f + (dayTime - 18) * 0.05f;
            }
            */

            int nowDuiHuanGold = (int)(duihuan_baseGold + duihuan_baseGold * duihuanPro);

            //随机值5%浮动
            Random random = new Random();
            float duihuan_randomValue = random.Next(10);
            duihuan_randomValue = duihuan_randomValue / 100f;
            if (duihuan_randomValue >= 0.1f) {
                duihuan_randomValue = 0.1f;
            }
            int duihuan_nowGold = (int)((float)nowDuiHuanGold * (0.95f + duihuan_randomValue));

            Log.Info("今日货币兑换值:" + duihuan_nowGold + " dayTime = " + dayTime);
            //最低不能低于昨天的兑换值
            if (duihuan_nowGold >= self.DBServerInfo.ServerInfo.ExChangeGold)
            {
                if (duihuan_nowGold < 500)
                {
                    duihuan_nowGold = 500;
                }
                self.DBServerInfo.ServerInfo.ExChangeGold = duihuan_nowGold;
                Log.Info("更新货币兑换值:" + self.DBServerInfo.ServerInfo.ExChangeGold);
            }

            self.DBServerInfo.ServerInfo.ChouKaDropId = ActivityV1Config.ChouKaDropId[RandomHelper.RandomNumber(0, ActivityV1Config.ChouKaDropId.Count)];
        }

        public static async ETTask InitDBRankInfo(this RankSceneComponent self)
        {
            await TimerComponent.Instance.WaitAsync(TimeHelper.Second);
           
            DBRankInfo dbRankInfo = await DBHelper.GetComponent<DBRankInfo>(self.DomainZone(), self.DomainZone());
            if (dbRankInfo== null)
            {
                DBRankInfo dBRankInfo = new DBRankInfo();
                dBRankInfo.Id = self.DomainZone();
                self.DBRankInfo = dBRankInfo;
            }
            else
            {
                self.DBRankInfo = dbRankInfo;
            }

            self.UpdateRankPetList();
        }

        public static async ETTask UpdateCombat(this RankSceneComponent self)
        {
            Log.Debug($"UpdateCombatUpdateCombat: {self.DomainZone()}");
            self.DomainScene().RemoveComponent<UnitComponent>();
            self.DomainScene().AddComponent<UnitComponent>();
            List<RankingInfo> rankingInfoList = new List<RankingInfo>();
            for (int i = self.DBRankInfo.rankingInfos.Count - 1; i >=0; i--)
            {
                rankingInfoList.Add(self.DBRankInfo.rankingInfos[i]);
            }

            await ETTask.CompletedTask;
        }

        public static async ETTask SaveDB(this RankSceneComponent self)
        {
            await DBHelper.SaveComponent(self.DomainZone(), self.DBRankInfo.Id, self.DBRankInfo);
            if (self.DBServerInfo != null)
            {
                await DBHelper.SaveComponent(self.DomainZone(), self.DBServerInfo.Id, self.DBServerInfo);
            }
        }

        /// <summary>
        /// 通知所有排名变化的玩家
        /// </summary>
        /// <param name="self"></param>
        /// <param name="rankingInfo"></param>
        public static void UpdateRankList(this RankSceneComponent self, RankingInfo rankingInfo)
        {
          
            int oldRankIndex = -1;
            Dictionary<long, int> oldRankList = new Dictionary<long, int>();
            for (int i = 0; i < self.DBRankInfo.rankingInfos.Count; i++)
            {
                RankingInfo rankingInfoTemp = self.DBRankInfo.rankingInfos[i];
                if (!oldRankList.TryAdd(rankingInfoTemp.UserId, i))
                {
                    Log.Error($"oldRankList.ContainsKey(rankingInfoTemp.UserId): {rankingInfoTemp.UserId}");
                }

                if (rankingInfoTemp.UserId == rankingInfo.UserId)
                {
                    oldRankIndex = i;
                }
            }

            if (oldRankIndex == -1)
            {
                self.DBRankInfo.rankingInfos.Add(rankingInfo);
            }
            else
            {
                self.DBRankInfo.rankingInfos[oldRankIndex] = rankingInfo;   
            }

            self.DBRankInfo.rankingInfos.Sort(delegate (RankingInfo a, RankingInfo b)
            {
                return (int)b.Combat - (int)a.Combat;
            });

            if (self.DBRankInfo.rankingInfos.Count > 500)
            {
                self.DBRankInfo.rankingInfos.RemoveAt(self.DBRankInfo.rankingInfos.Count - 1);   
            }


            List<long> updateRankList = new List<long>();
            for (int i = 0; i < self.DBRankInfo.rankingInfos.Count; i++)
            {
                RankingInfo rankingInfoTemp = self.DBRankInfo.rankingInfos[i];
                if (!oldRankList.TryGetValue(rankingInfoTemp.UserId, out int oldIndex) || oldIndex != i)
                {
                    updateRankList.Add(rankingInfoTemp.UserId);
                }
            }

            for (int i = 0; i < updateRankList.Count; i++)
            {
                self.UpdateRankNo1(updateRankList[i], rankingInfo.Occ).Coroutine();
            }
        }


        /// <summary>
        /// 通知排行榜第一刷新
        /// </summary>
        public static async ETTask  UpdateRankNo1(this RankSceneComponent self, long userId, int occ)
        {
            int zone = self.DomainZone();
            // 战区 Rank 不走单服第一名推送（无 ServerItem / 本服 Gate）
            if (StartZoneConfigCategory.Instance.IsWarShareZone(zone))
            {
                return;
            }
            if (DBHelper.GetOpenServerDay(zone) < 3)
            {
                return;
            }
            int rankId = self.GetCombatRank(userId);
            if (rankId <= 0)
            {
                return;
            }
            await ETTask.CompletedTask;
            //通知玩家
            //long gateServerId = DBHelper.GetGateServerId(zone);
            //G2T_GateUnitInfoResponse g2M_UpdateUnitResponse = (G2T_GateUnitInfoResponse)await ActorMessageSenderComponent.Instance.Call
            //   (gateServerId, new T2G_GateUnitInfoRequest()
            //   {
            //       UserID = userId
            //   });
            //if (g2M_UpdateUnitResponse.PlayerState == (int)PlayerState.Game && g2M_UpdateUnitResponse.SessionInstanceId > 0)
            //{

                R2M_RankUpdateMessage r2M_RankUpdateMessage = new R2M_RankUpdateMessage();
                r2M_RankUpdateMessage.RankType = 1;
                r2M_RankUpdateMessage.RankId = rankId;
                r2M_RankUpdateMessage.OccRankId = self.GetOccCombatRank(userId, occ);
                //MessageHelper.SendToLocationActor(g2M_UpdateUnitResponse.UnitId, r2M_RankUpdateMessage);
                MessageHelper.SendToLocationActor(userId, r2M_RankUpdateMessage);
            //}
        }

        public static int GetTrialRank(this RankSceneComponent self, long userId)
        {
            for (int i = 0; i < self.DBRankInfo.rankingTrial.Count; i++)
            {
                if (self.DBRankInfo.rankingTrial[i].KeyId == userId)
                {
                    return i + 1;
                }

            }
            return 0;
        }

        public static void ClearSeasonTowerRankByUnitId(this RankSceneComponent self, long userId)
        {
            for (int i = self.DBRankInfo.rankSeasonTower.Count - 1; i >= 0; i--)
            {
                if (self.DBRankInfo.rankSeasonTower[i].KeyId == userId)
                {
                    self.DBRankInfo.rankSeasonTower.RemoveAt(i);    
                }
            }
        }

        public static int GetSeasonTowerRank(this RankSceneComponent self, long userId)
        {
            for (int i = 0; i < self.DBRankInfo.rankSeasonTower.Count; i++)
            {
                if (self.DBRankInfo.rankSeasonTower[i].KeyId == userId)
                {
                    return i + 1;
                }

            }
            return 0;
        }

        public static int GetPetRank(this RankSceneComponent self, long userId)
        {
            for (int i = 0; i < self.DBRankInfo.rankingPets.Count; i++)
            {
                RankPetInfo rankPetInfo = self.DBRankInfo.rankingPets[i];
                if (rankPetInfo.UserId == userId)
                {
                    return rankPetInfo.RankId;
                }

            }
            return 0;
        }

        public static int GetCombatRank(this RankSceneComponent self, long usrerId)
        {
            for (int i = 0; i < self.DBRankInfo.rankingInfos.Count; i++)
            {
                if (self.DBRankInfo.rankingInfos[i].UserId == usrerId)
                {
                    return i + 1;
                }
            }
            return 0;
        }

        public static int GetOccCombatRank(this RankSceneComponent self, long usrerId, int occ)
        {
            int ocRank = 0;
            for (int i = 0; i < self.DBRankInfo.rankingInfos.Count; i++)
            {
                RankingInfo rankingInfo = self.DBRankInfo.rankingInfos[i];
                if (rankingInfo.Occ == occ)
                {
                    ocRank++;
                }

                if (rankingInfo.UserId == usrerId)
                {
                    return ocRank;
                }
            }
            return 0;
        }

     
        public static void UpdateWorldLevel(this RankSceneComponent self, RankingInfo rankingInfo)
        {
            ServerInfo serverInfo = self.DBServerInfo.ServerInfo;
            //if (rankingInfo.PlayerLv < serverInfo.WorldLv)
            //{
            //    return;
            //}
            if (serverInfo.RankingInfo == null)
            {
                serverInfo.RankingInfo = rankingInfo;
                self.BroadcastWorldLv().Coroutine();
                return;
            }
            if (serverInfo.RankingInfo.PlayerLv < rankingInfo.PlayerLv)
            {
                serverInfo.RankingInfo = rankingInfo;
                self.BroadcastWorldLv().Coroutine();
            }
        }

        public static void OnRecvRankUpdate(this RankSceneComponent self, int campId, RankingInfo rankingInfo)
        {
            if (!StartZoneConfigCategory.Instance.IsWarShareZone(self.DomainZone()))
            {
                self.UpdateWorldLevel(rankingInfo);
            }
            self.UpdateRankList(rankingInfo);
            self.UpdateCampRankList(campId, rankingInfo);
        }

        public static void UpdateRankPetList(this RankSceneComponent self)
        {
            //读机器人配置表（战区榜不灌本服机器人）
            if (StartZoneConfigCategory.Instance.IsWarShareZone(self.DomainZone()))
            {
                return;
            }
            if (self.DBRankInfo.rankingPets.Count > 0)
            {
                return;
            }
            List<int> allPet = new List<int>() { 1000101, 1000201 , 1000301 , 1000401 , 1000501 ,1000601, 1000701};
            for (int i = 0; i < CommonConfig.PetRankNumber; i++)
            {
                int[] indexs = RandomHelper.GetRandoms(3, 0, allPet.Count);
                List<int> pets = new List<int>();
                for (int p = 0; p < indexs.Length; p++)
                {
                    pets.Add(allPet[p]);
                }
                self.DBRankInfo.rankingPets.Add(new RankPetInfo() { UserId = IdGenerater.Instance.GenerateId(), TeamName = "机器人:" + (i + 1) + "的队伍", RankId = i + 1, PlayerName = "机器人:" + (i + 1), PetUId = new List<long>() { 0, 0, 0 }, PetConfigId = pets });
            }
        }

        public static void OnRecvPetRank(this RankSceneComponent self, M2R_PetRankUpdateRequest m2R_PetRankUpdateRequest)
        {
            RankPetInfo enemyRankPetInfo = null;
            RankPetInfo selfRankPetInfo = null;

            for (int i = 0; i < self.DBRankInfo.rankingPets.Count; i++)
            {
                RankPetInfo rankPetInfo = self.DBRankInfo.rankingPets[i];
                if (rankPetInfo.UserId == m2R_PetRankUpdateRequest.RankPetInfo.UserId)
                {
                    selfRankPetInfo = rankPetInfo;
                }
                if (rankPetInfo.UserId == m2R_PetRankUpdateRequest.EnemyId)
                {
                    enemyRankPetInfo = rankPetInfo;
                }
            }
            //没找到对方或者高于对方排名，不更新排名
            if (enemyRankPetInfo != null)
            {
                if (selfRankPetInfo != null)
                {
                    if (selfRankPetInfo.RankId > enemyRankPetInfo.RankId)
                    {
                        int selfRank = selfRankPetInfo.RankId;
                        selfRankPetInfo.RankId = enemyRankPetInfo.RankId;
                        enemyRankPetInfo.RankId = selfRank;
                    }
                }
                else
                {
                    m2R_PetRankUpdateRequest.RankPetInfo.RankId = enemyRankPetInfo.RankId;
                    self.DBRankInfo.rankingPets.Remove(enemyRankPetInfo);
                    self.DBRankInfo.rankingPets.Add(m2R_PetRankUpdateRequest.RankPetInfo);
                }
            }

            self.DBRankInfo.rankingPets.Sort(delegate (RankPetInfo a, RankPetInfo b)
            {
                return a.RankId - b.RankId;
            });
            for (int i = 0; i < self.DBRankInfo.rankingPets.Count; i++)
            {
                self.DBRankInfo.rankingPets[i].RankId = i + 1;
            }
        }

        public static List<RankPetInfo> GetRankPetList(this RankSceneComponent self, int rankNumber)
        {
            List<RankPetInfo> rankPetInfos = new List<RankPetInfo>();
            HashSet<int> indexList = new HashSet<int>();

            //前四名只找1-10名
            if (rankNumber >= 1 && rankNumber <= 4)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (i != (rankNumber - 1))
                    {
                        indexList.Add(i);
                    }
                }
                indexList.Add(RandomHelper.RandomNumber( 3, 10)) ;
            }
            else
            {
                int randomNumber = 0;

                while (indexList.Count < 3)
                {
                    if (randomNumber > 200)
                    {
                        Log.Warning($"randomNumber > 200:  {randomNumber}");
                        Log.Console($"randomNumber > 200:  {randomNumber}");
                        break;
                    }

                    int index = 0;
                    if (rankNumber == 0)        //没上榜就找排行榜最后十名
                        index = RandomHelper.RandomNumber(self.DBRankInfo.rankingPets.Count - 10, self.DBRankInfo.rankingPets.Count);
                    else if (rankNumber > 10)       //大于10名找比自己排行前十名的
                        index = RandomHelper.RandomNumber(rankNumber - 11, rankNumber - 1);
                    else // 4<rankNumber<=10 ,前面取俩个，后面取一个
                    {
                        if (indexList.Count < 2)
                        {
                            index = RandomHelper.RandomNumber(0, rankNumber - 1);
                        }
                        else
                        {
                            index = RandomHelper.RandomNumber(rankNumber, 11);
                        }
                    }

                    indexList.Add(index);

                    randomNumber++;
                }
            }
            List<int> sortedIndexes = new List<int>(indexList);
            sortedIndexes.Sort();

            for (int i = 0; i < sortedIndexes.Count; i++)
            {
                rankPetInfos.Add(self.DBRankInfo.rankingPets[sortedIndexes[i]]);
            }

            return rankPetInfos;
        }

        public static void OnDeleteRole(this RankSceneComponent self, List<RankPetInfo> rankingInfos, long userId)
        {
            for (int i = rankingInfos.Count - 1; i >= 0; i--)
            {
                if (rankingInfos[i].UserId == userId)
                {
                    rankingInfos.RemoveAt(i);
                    break;
                }
            }
        }

        public static void  OnDeleteRole(this RankSceneComponent self, List<RankingInfo> rankingInfos, long userId)
        {
            for (int i = rankingInfos.Count - 1; i >= 0; i-- )
            {
                if (rankingInfos[i].UserId == userId)
                {
                    rankingInfos.RemoveAt(i);
                    break;
                }
            }
        }

        public static  void OnShowLieBegin(this RankSceneComponent self)
        {
            self.DBRankInfo.rankShowLie.Clear();
            self.BroadcastShowLie("1").Coroutine();
        }

        public static async ETTask BroadcastShowLie(this RankSceneComponent self, string loadvalue)
        {
            List<ServerItem> serverItems = ServerHelper.GetServerList();
            //Console.WriteLine($"BroadcastShowLie: ServerItems {serverItems.Count}");

            int firstserver = 0;
            for (int i = 0; i < serverItems.Count; i++)
            {
                if (serverItems[i].Show == 1)
                {
                    firstserver = serverItems[i].ServerId;
                    break;
                }
            }

            if (firstserver == self.DomainZone())
            {
                Log.Debug($"BroadcastShowLie:  {self.DomainZone()}");
                Log.Console($"BroadcastShowLie value:  {self.DomainZone()} {loadvalue}");
                foreach (StartProcessConfig listprogress in StartProcessConfigCategory.Instance.GetAll().Values)
                {
                    List<StartSceneConfig> processScenes = StartSceneConfigCategory.Instance.GetByProcess(listprogress.Id);
                    if (processScenes.Count == 0 || listprogress.Id == CommonConfig.RobotProgress)  //机器人进程
                    {
                        continue;
                    }

                    StartSceneConfig startSceneConfig = processScenes[0];
                    long mapInstanceId = StartSceneConfigCategory.Instance.GetBySceneName(startSceneConfig.Zone, startSceneConfig.Name).InstanceId;
                    A2R_Broadcast createUnit = (A2R_Broadcast)await ActorMessageSenderComponent.Instance.Call(
                        mapInstanceId, new R2A_Broadcast() { LoadType = 1, LoadValue = loadvalue });
                }
            }
        }

        public static async ETTask OnDemonOver(this RankSceneComponent self)
        {
            await ETTask.CompletedTask;
            int zone = self.DomainZone();

            Log.Warning($"发放恶魔排行榜奖励： {zone}");
            long serverTime = TimeHelper.ServerNow();
            List<RankingInfo> rankingInfos = self.DBRankInfo.rankingDemon;
            long mailServerId = DBHelper.GetMailServerId( self.DomainZone() );
            Dictionary<string, List<RewardItem>> rewardCache = new Dictionary<string, List<RewardItem>>();
            for (int i = 0; i < rankingInfos.Count; i++)
            {
                LDRankList rankRewardConfig = RankHelper.GetRankReward(i + 1, 5);
                if (rankRewardConfig == null)
                {
                    continue;
                }
                MailInfo mailInfo = new MailInfo();

                Log.Warning($"发放恶魔排行榜奖励2： {rankingInfos[i].UserId}");

                mailInfo.Status = 0;
                mailInfo.Context = $"恭喜您获得恶魔排行榜第{i + 1}名奖励";
                mailInfo.Title = "恶魔排行榜奖励";
                mailInfo.MailId = IdGenerater.Instance.GenerateId();

                AddRankMailRewardItems(mailInfo, rankRewardConfig.Reward, $"{ItemGetWay.Demon}_{serverTime}", rewardCache);
            
            }
        }

        //家族战结束
        public static async ETTask OnUnionRaceOver(this RankSceneComponent self)
        {
            await ETTask.CompletedTask;
            int zone = self.DomainZone();
          
            Log.Warning($"发放家族战排行榜奖励： {zone}");
            long serverTime = TimeHelper.ServerNow();
            List<RankShouLieInfo> rankingInfos = self.DBRankInfo.rankUnionRace;
            long mailServerId = StartSceneConfigCategory.Instance.GetBySceneName(self.DomainZone(), Enum.GetName(SceneType.Mail)).InstanceId;
            Dictionary<string, List<RewardItem>> rewardCache = new Dictionary<string, List<RewardItem>>();
            for (int i = 0; i < rankingInfos.Count; i++)
            {
                LDRankList rankRewardConfig = RankHelper.GetRankReward(i + 1, 4);
                if (rankRewardConfig == null)
                {
                    continue;
                }
                MailInfo mailInfo = new MailInfo();

                Log.Warning($"发放家族战排行榜奖励2： {rankingInfos[i].UnitID}");

                mailInfo.Status = 0;
                mailInfo.Context = $"恭喜您获得家族战排行榜第{i + 1}名奖励";
                mailInfo.Title = "家族战排行榜奖励";
                mailInfo.MailId = IdGenerater.Instance.GenerateId();

                AddRankMailRewardItems(mailInfo, rankRewardConfig.Reward, $"{ItemGetWay.ShowLie}_{serverTime}", rewardCache);
                //E2M_EMailSendResponse g_EMailSendResponse = (E2M_EMailSendResponse)await ActorMessageSenderComponent.Instance.Call
                //      (mailServerId, new M2E_EMailSendRequest()
                //      {
                //          Id = rankingInfos[i].UnitID,
                //          MailInfo = mailInfo
                //      });
            }
        }

        //发送狩猎排行奖励
        public static async ETTask OnShowLieOver(this RankSceneComponent self)
        {
            await ETTask.CompletedTask;
            int zone = self.DomainZone();
            self.BroadcastShowLie("0").Coroutine();

            //Log.Console($"发放狩猎排行榜奖励： {zone}");
            Log.Debug($"发放狩猎排行榜奖励： {zone}");
            long serverTime = TimeHelper.ServerNow();
            List<RankShouLieInfo> rankingInfos = self.DBRankInfo.rankShowLie;
            long mailServerId = StartSceneConfigCategory.Instance.GetBySceneName(self.DomainZone(), Enum.GetName(SceneType.Mail)).InstanceId;
            Dictionary<string, List<RewardItem>> rewardCache = new Dictionary<string, List<RewardItem>>();
            for (int i = 0; i < rankingInfos.Count; i++)
            {
                LDRankList rankRewardConfig = RankHelper.GetRankReward(i + 1, 3);
                if (rankRewardConfig == null)
                {
                    continue;
                }
                MailInfo mailInfo = new MailInfo();

                mailInfo.Status = 0;
                mailInfo.Context = $"恭喜您获得狩猎排行榜第{i + 1}名奖励";
                mailInfo.Title = "狩猎排行榜奖励";
                mailInfo.MailId = IdGenerater.Instance.GenerateId();
                Log.Debug($"发放狩猎排行榜奖励：zone. {zone} rankid.{i + 1}  unitid.{rankingInfos[i].UnitID}  {rankingInfos[i].PlayerName}  {rankingInfos[i].KillNumber}");
                AddRankMailRewardItems(mailInfo, rankRewardConfig.Reward, $"{ItemGetWay.RankReward}_{serverTime}", rewardCache);
                //E2M_EMailSendResponse g_EMailSendResponse = (E2M_EMailSendResponse)await ActorMessageSenderComponent.Instance.Call
                //      (mailServerId, new M2E_EMailSendRequest()
                //      {
                //          Id = rankingInfos[i].UnitID,
                //          MailInfo = mailInfo
                //      });
            }
        }

        /// <summary>
        /// 发送试炼副本奖励
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static async ETTask SendTrialReward(this RankSceneComponent self)
        {
            int zone = self.DomainZone();
            await TimerComponent.Instance.WaitAsync(TimeHelper.Second * 10);
            DateTime dateTime = TimeHelper.DateTimeNow();
            if ((int)dateTime.DayOfWeek != 1)
            {
                return;
            }
            Log.Debug($"发放试炼排行榜奖励： {zone}");
            //Log.Console($"发放试炼排行榜奖励： {zone}");
            long serverTime = TimeHelper.ServerNow();
            List<KeyValuePairLong> rankingInfos = self.DBRankInfo.rankingTrial;
            long mailServerId = StartSceneConfigCategory.Instance.GetBySceneName(self.DomainZone(), Enum.GetName(SceneType.Mail)).InstanceId;
            Dictionary<string, List<RewardItem>> rewardCache = new Dictionary<string, List<RewardItem>>();
            for (int i = 0; i < rankingInfos.Count; i++)
            {
                LDRankList rankRewardConfig = RankHelper.GetRankReward(i + 1, 6);
                if (rankRewardConfig == null)
                {
                    continue;
                }
                MailInfo mailInfo = new MailInfo();

                mailInfo.Status = 0;
                mailInfo.Context = $"恭喜您获得试炼排行榜第{i + 1}名奖励";
                mailInfo.Title = "排行榜奖励";
                mailInfo.MailId = IdGenerater.Instance.GenerateId();

                if (i <= 10)
                {
                    Log.Warning($"试炼奖励: {self.DomainZone()} {rankingInfos[i].KeyId}");
                }
                AddRankMailRewardItems(mailInfo, rankRewardConfig.Reward, $"{ItemGetWay.RankReward}_{serverTime}", rewardCache);
                //E2M_EMailSendResponse g_EMailSendResponse = (E2M_EMailSendResponse)await ActorMessageSenderComponent.Instance.Call
                //      (mailServerId, new M2E_EMailSendRequest()
                //      {
                //          Id = rankingInfos[i].KeyId,
                //          MailInfo = mailInfo
                //      });
            }

            self.DBRankInfo.rankingTrial.Clear();
        }

        public static async ETTask SendSeasonTowerReward(this RankSceneComponent self)
        {
            int zone = self.DomainZone();
            await TimerComponent.Instance.WaitAsync(RandomHelper.RandomNumber(5000, 10000));
            DateTime dateTime = TimeHelper.DateTimeNow();
            if (dateTime.DayOfWeek != DayOfWeek.Monday)
            {
                return;
            }

            Log.Debug($"发放赛季之塔排行榜奖励： {zone}");
            //Console.WriteLine($"发放赛季之塔排行榜奖励： {zone}");

            long serverTime = TimeHelper.ServerNow();
            List<KeyValuePairLong> rankingInfos = self.DBRankInfo.rankSeasonTower;
            long mailServerId = StartSceneConfigCategory.Instance.GetBySceneName(self.DomainZone(), Enum.GetName(SceneType.Mail)).InstanceId;
            Dictionary<string, List<RewardItem>> rewardCache = new Dictionary<string, List<RewardItem>>();
            for (int i = 0; i < rankingInfos.Count; i++)
            {
                LDRankList rankRewardConfig = RankHelper.GetRankReward(i + 1, 7);
                if (rankRewardConfig == null)
                {
                    continue;
                }
                MailInfo mailInfo = new MailInfo();

                mailInfo.Status = 0;
                mailInfo.Context = $"恭喜您获得赛季之塔第{i + 1}名奖励";
                mailInfo.Title = "赛季之塔奖励";
                mailInfo.MailId = IdGenerater.Instance.GenerateId();

                if (i <= 10)
                {
                    Log.Warning($"赛季之塔奖励: {self.DomainZone()} {rankingInfos[i].KeyId}");
                }
                AddRankMailRewardItems(mailInfo, rankRewardConfig.Reward, $"{ItemGetWay.RankReward}_{serverTime}", rewardCache);
                //E2M_EMailSendResponse g_EMailSendResponse = (E2M_EMailSendResponse)await ActorMessageSenderComponent.Instance.Call
                //      (mailServerId, new M2E_EMailSendRequest()
                //      {
                //          Id = rankingInfos[i].KeyId,
                //          MailInfo = mailInfo
                //      });
            }

            self.DBRankInfo.rankSeasonTower.Clear();
            await ETTask.CompletedTask;
        }

        /// <summary>
        /// 发送战力排行奖励
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static async ETTask SendCombatReward(this RankSceneComponent self)
        {
            int zone = self.DomainZone();
            await TimerComponent.Instance.WaitAsync(RandomHelper.RandomNumber(5000, 10000));
            DateTime dateTime = TimeHelper.DateTimeNow();
            if (!RankHelper.HaveReward(1, (int)dateTime.DayOfWeek))
            {
                return;
            }
            Log.Debug($"发放战力排行榜奖励： {zone}");
            long serverTime = TimeHelper.ServerNow();
            List<RankingInfo> rankingInfos = self.DBRankInfo.rankingInfos;
            long mailServerId = StartSceneConfigCategory.Instance.GetBySceneName(self.DomainZone(), Enum.GetName(SceneType.Mail)).InstanceId;
            Dictionary<string, List<RewardItem>> rewardCache = new Dictionary<string, List<RewardItem>>();
            for (int i = 0; i < rankingInfos.Count; i++)
            {
                LDRankList rankRewardConfig = RankHelper.GetRankReward(i+1, 1);
                if (rankRewardConfig == null)
                {
                    continue;
                }
                MailInfo mailInfo = new MailInfo();

                mailInfo.Status = 0;
                mailInfo.Context = $"恭喜您获得排行榜第{i + 1}名奖励";
                mailInfo.Title = "排行榜奖励";
                mailInfo.MailId = IdGenerater.Instance.GenerateId();

                if (i <= 10)
                {
                    Log.Warning($"战力奖励: {self.DomainZone()} {rankingInfos[i].UserId}   {i}");
                }
                AddRankMailRewardItems(mailInfo, rankRewardConfig.Reward, $"{ItemGetWay.RankReward}_{serverTime}", rewardCache);
                //E2M_EMailSendResponse g_EMailSendResponse = (E2M_EMailSendResponse)await ActorMessageSenderComponent.Instance.Call
                //      (mailServerId, new M2E_EMailSendRequest() 
                //      { 
                //          Id = rankingInfos[i].UserId,
                //          MailInfo = mailInfo });
            }
        }

        public static async ETTask SendPetReward(this RankSceneComponent self)
        {
            int zone = self.DomainZone();
            await TimerComponent.Instance.WaitAsync(RandomHelper.RandomNumber(1000, 10000));
            DateTime dateTime = TimeHelper.DateTimeNow();
            if (!RankHelper.HaveReward(2, (int)dateTime.DayOfWeek))
            {
                return;
            }
            Log.Debug($"发放宠物排行榜奖励： {zone}");
            long serverTime = TimeHelper.ServerNow();
            List<RankPetInfo> rankingInfos = self.DBRankInfo.rankingPets;
            long mailServerId = StartSceneConfigCategory.Instance.GetBySceneName(self.DomainZone(), Enum.GetName(SceneType.Mail)).InstanceId;
            Dictionary<string, List<RewardItem>> rewardCache = new Dictionary<string, List<RewardItem>>();
            for (int i = 0; i < rankingInfos.Count; i++)
            {
                bool havePetUId = false;
                for (int k = 0; k < rankingInfos[i].PetUId.Count; k++)
                {
                    if (rankingInfos[i].PetUId[k] > 0)
                    {
                        havePetUId = true;
                        break;
                    }
                }
                if (!havePetUId)
                {
                    continue;
                }

                LDRankList rankRewardConfig = RankHelper.GetRankReward(i + 1, 2);
                if (rankRewardConfig == null)
                {
                    continue;
                }

                MailInfo mailInfo = new MailInfo();

                mailInfo.Status = 0;
                mailInfo.Context = $"恭喜您获得排行榜第{i + 1}名奖励";
                mailInfo.Title = "排行榜奖励";
                mailInfo.MailId = IdGenerater.Instance.GenerateId();

                AddRankMailRewardItems(mailInfo, rankRewardConfig.Reward, $"{ItemGetWay.RankReward}_{serverTime}", rewardCache);
                //E2M_EMailSendResponse g_EMailSendResponse = (E2M_EMailSendResponse)await ActorMessageSenderComponent.Instance.Call
                //      (mailServerId, new M2E_EMailSendRequest()
                //      {
                //          Id = rankingInfos[i].UserId,
                //          MailInfo = mailInfo
                //      });
            }
        }
    }
}
