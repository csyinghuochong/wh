using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{

    [Timer(TimerType.ActivitySceneTimer)]
    public class ActivitySceneTimer : ATimer<ActivitySceneComponent>
    {
        public override void Run(ActivitySceneComponent self)
        {
            try
            {
                self.OnCheck();
            }
            catch (Exception e)
            {
                Log.Error($"move timer error: {self.Id}\n{e}");
            }
        }
    }

    [Timer(TimerType.ActivityTipTimer)]
    public class ActivityTipTimer : ATimer<ActivitySceneComponent>
    {
        public override void Run(ActivitySceneComponent self)
        {
            try
            {
                self.OnCheckFuntionButton().Coroutine();
            }
            catch (Exception e)
            {
                Log.Error($"move timer error: {self.Id}\n{e}");
            }
        }
    }

    /// <summary>
    /// 定时刷新的暂时都作为活动来处理
    /// </summary>
    [ObjectSystem]
    public class ActivitySceneComponentAwakeSystem : AwakeSystem<ActivitySceneComponent>
    {
        public override void Awake(ActivitySceneComponent self)
        {
            self.MapIdList.Clear();
            self.MapIdList.Add(DBHelper.GetGateServerId(self.DomainZone()));
            self.MapIdList.Add(DBHelper.GetPaiMaiServerId(self.DomainZone()));
            self.MapIdList.Add(DBHelper.GetRankServerId(self.DomainZone()));
            self.MapIdList.Add(DBHelper.GetFubenCenterId(self.DomainZone()));
            //self.MapIdList.Add(DBHelper.GetArenaServerId(self.DomainZone()));
            //self.MapIdList.Add(DBHelper.GetBattleServerId(self.DomainZone()));
            //self.MapIdList.Add(DBHelper.GetUnionServerId(self.DomainZone()));
            //self.MapIdList.Add(DBHelper.GetSoloServerId(self.DomainZone()));
            self.MapIdList.Add(DBHelper.GetDbCacheId(self.DomainZone()));
            self.InitDayActivity().Coroutine();
            self.InitFunctionButton();
        }
    }

    [ObjectSystem]
    public class ActivitySceneComponentDestroySystem : DestroySystem<ActivitySceneComponent>
    {

        public override void Destroy(ActivitySceneComponent self)
        {
            TimerComponent.Instance.Remove(ref self.Timer);
        }
    }

    public static class ActivitySceneComponentSystem
    {
        public static void OnCheck(this ActivitySceneComponent self)
        {
            DateTime dateTime = TimeHelper.DateTimeNow();

            if (self.DBDayActivityInfo.LastHour != dateTime.Hour)
            {
                self.DBDayActivityInfo.LastHour = dateTime.Hour;
                self.NoticeActivityUpdate_Hour(dateTime).Coroutine();
            }

            self.CheckIndex += 1000;
            if (self.CheckIndex >= TimeHelper.Minute * 5)
            {
                self.CheckPetMine();
                self.SaveDB();
                self.CheckIndex = 0;
            }

            //self.TeamUpdateHandler().Coroutine();
        }

        public static void InitPetMineExtend(this ActivitySceneComponent self)
        {
#if false // TODO: migrate to LD config
            List<MineBattleConfig> mineBattleConfig = MineBattleConfigCategory.Instance.GetAll().Values.ToList();
            for (int i = 0; i < mineBattleConfig.Count; i++)
            {
                int totalNumber = CommonConfig.PetMiningList[mineBattleConfig[i].Id].Count;

                int hexinNumber = 1;

                if (mineBattleConfig[i].Id == 10001)
                {
                    hexinNumber = 1;
                }
                if (mineBattleConfig[i].Id == 10002)
                {
                    hexinNumber = 2;
                }
                if (mineBattleConfig[i].Id == 10003)
                {
                    hexinNumber = 5;
                }

                int[] index = RandomHelper.GetRandoms(hexinNumber, 0, totalNumber);
                List<int> hexinlist = new List<int>(index);

                for (int hexin = 0; hexin < hexinlist.Count; hexin++)
                {
                    self.DBDayActivityInfo.PetMingHexinList.Add(new IntLongPair()
                    {
                        KeyId = mineBattleConfig[i].Id,
                        Value = hexinlist[hexin]
                    });
                }
            }
#endif
        }

        public static void CheckPetMine(this ActivitySceneComponent self)
        {
#if false // TODO: migrate to LD config
            {
                int openDay = ServerHelper.GetOpenServerDay(false, self.DomainZone());

                List<PetMingPlayerInfo> petMingPlayers = self.DBDayActivityInfo.PetMingList;

                Dictionary<long, long> playerLimitList = new Dictionary<long, long>();
                for (int i = 0; i < petMingPlayers.Count; i++)
                {
                    MineBattleConfig mineBattleConfig = MineBattleConfigCategory.Instance.Get(petMingPlayers[i].MineType);
                    int chanchu = mineBattleConfig.ChanChuLimit;

                    if (!playerLimitList.ContainsKey(petMingPlayers[i].UnitId))
                    {
                        playerLimitList.Add(petMingPlayers[i].UnitId, 0);
                    }
                    playerLimitList[petMingPlayers[i].UnitId] += chanchu;
                }

                for (int i = 0; i < petMingPlayers.Count; i++)
                {
                    long playerLimit = playerLimitList[petMingPlayers[i].UnitId];

                    float coffi = CommonHelper.GetMineCoefficient(openDay, petMingPlayers[i].MineType, petMingPlayers[i].Postion, self.DBDayActivityInfo.PetMingHexinList);

                    MineBattleConfig mineBattleConfig = MineBattleConfigCategory.Instance.Get(petMingPlayers[i].MineType);
                    int chanchu = (int)(mineBattleConfig.GoldOutPut * coffi * (self.CheckIndex * 1f/ TimeHelper.Hour));

                    if (!self.DBDayActivityInfo.PetMingChanChu.ContainsKey(petMingPlayers[i].UnitId))
                    {
                        self.DBDayActivityInfo.PetMingChanChu.Add(petMingPlayers[i].UnitId, chanchu);
                    }
                    else
                    {
                        long oldValue = self.DBDayActivityInfo.PetMingChanChu[petMingPlayers[i].UnitId];
                        oldValue += chanchu;
                        oldValue = Math.Min(oldValue, playerLimit);

                        self.DBDayActivityInfo.PetMingChanChu[petMingPlayers[i].UnitId] = oldValue;
                    }
                }
            }
#endif
        }

        public static async ETTask TeamUpdateHandler(this ActivitySceneComponent self)
        {
            DateTime dateTime = TimeHelper.DateTimeNow();

            //if (dateTime.Year == 24 && dateTime.Month == 1 && dateTime.Day == 5 && dateTime.Hour == 15 && dateTime.Minute == 19)
            //{

            //    A2A_ActivityUpdateResponse m2m_TrasferUnitResponse = (A2A_ActivityUpdateResponse)await ActorMessageSenderComponent.Instance.Call
            //             (DBHelper.GetRankServerId(self.DomainZone()), new A2A_ActivityUpdateRequest() { Hour = 16, OpenDay = 1 });
            //}
            await ETTask.CompletedTask;
        }

        public static async ETTask InitDayActivity(this ActivitySceneComponent self)
        {
            int zone = self.DomainZone();
            long dbCacheId = DBHelper.GetDbCacheId(zone);
            await TimerComponent.Instance.WaitAsync(RandomHelper.RandomNumber(1000, 2000));
        
            DBDayActivityInfo dbDayActivityInfo = await DBHelper.GetComponent<DBDayActivityInfo>(self.DomainZone(), self.DomainZone());
            if (dbDayActivityInfo == null)
            {
                self.DBDayActivityInfo = self.AddChildWithId<DBDayActivityInfo>((long)self.DomainZone());
            }
            else
            {
                self.AddChild(dbDayActivityInfo);
                self.DBDayActivityInfo = dbDayActivityInfo;
            }
            int openServerDay = DBHelper.GetOpenServerDay(zone);
            LogHelper.LogDebug($"InitDayActivity: {zone}  {openServerDay}");
          
            if (self.DBDayActivityInfo.GlobalRandomShops == null || self.DBDayActivityInfo.GlobalRandomShops.Count == 0)
            {
                self.InitGlobalRandomShop();
            }

            if (self.DBDayActivityInfo.PetMingHexinList.Count == 0)
            {
                self.InitPetMineExtend();
            }
            self.SaveDB();
            self.CreateRobot(openServerDay).Coroutine();     

            //每日活动
            self.Timer = TimerComponent.Instance.NewRepeatedTimer(TimeHelper.Second, TimerType.ActivitySceneTimer, self);
        }

        public static void InitGlobalRandomShop(this ActivitySceneComponent self)
        {
            self.DBDayActivityInfo.GlobalRandomShops = RandomShopHelper.InitGlobalRandomShops();
        }

        public static List<ShopGoodsItem> GetGlobalRandomShopList(this ActivitySceneComponent self, int shopId)
        {
            if (self.DBDayActivityInfo.GlobalRandomShops == null)
            {
                self.DBDayActivityInfo.GlobalRandomShops = new Dictionary<int, List<ShopGoodsItem>>();
            }

            if (self.DBDayActivityInfo.GlobalRandomShops.Count == 0)
            {
                self.InitGlobalRandomShop();
            }

            if (self.DBDayActivityInfo.GlobalRandomShops.TryGetValue(shopId, out List<ShopGoodsItem> list))
            {
                return list;
            }

            return new List<ShopGoodsItem>();
        }

        public static async ETTask OnCheckFuntionButton(this ActivitySceneComponent self)
        {
            long serverTime = TimeHelper.ServerNow();
            DateTime dateTime = TimeInfo.Instance.ToDateTime(serverTime);

            if (self.ActivityTimerList.Count > 0)
            {
                int functionId = self.ActivityTimerList[0].FunctionId;
                bool todayopen = FunctionHelp.IsFunctionDayOpen((int)dateTime.DayOfWeek, functionId);
                Log.Warning($"OnCheckFuntionButton: {functionId} {self.ActivityTimerList[0].FunctionType}");

                long sceneserverid = 0;
                switch (functionId)
                {
                    case 1025://战场
                        sceneserverid = DBHelper.GetFubenCenterId(self.DomainZone());
                        break;
                    case 1043: //家族Boss
                        sceneserverid = DBHelper.GetUnionServerId(self.DomainZone());
                        break;
                    case 1044:  //家族争霸
                        sceneserverid = DBHelper.GetUnionServerId(self.DomainZone());
                        break;
                    case 1045:
                        sceneserverid = DBHelper.GetSoloServerId(self.DomainZone());
                        break;
                    case 1052://狩猎活动
                        sceneserverid = DBHelper.GetRankServerId(self.DomainZone());
                        break;
                    case 1055:
                        //喜从天降
                        sceneserverid = DBHelper.GetHappyServerId(self.DomainZone());
                        break;
                    case 1057: //小龟大赛
                        sceneserverid = DBHelper.MapCityServerId(self.DomainZone());
                        break;
                    case 1058://奔跑比赛
                    case 1059://恶魔活动
                        sceneserverid = DBHelper.GetFubenCenterId(self.DomainZone());
                        break;
                    case 2000:
                        sceneserverid = DBHelper.GetGateServerId(self.DomainZone());
                        break;
                    default:
                        break;
                }

                if (todayopen)
                {
                    Other2A_ActivityUpdateResponse m2m_TrasferUnitResponse = (Other2A_ActivityUpdateResponse)await ActorMessageSenderComponent.Instance.Call
                                    (sceneserverid, new A2Other_ActivityUpdateRequest() { Hour = -1, FunctionId = functionId, FunctionType = self.ActivityTimerList[0].FunctionType });
                }
                if (todayopen && functionId == 1044 && self.ActivityTimerList[0].FunctionType == 2)
                {
                    //1044
                    long rankserverid = DBHelper.GetRankServerId(self.DomainZone());
                    ////家族战结束. 发送奖励
                    Other2A_ActivityUpdateResponse m2m_TrasferUnitResponse = (Other2A_ActivityUpdateResponse)await ActorMessageSenderComponent.Instance.Call
                                 (rankserverid, new A2Other_ActivityUpdateRequest() { Hour = -1, FunctionId = functionId, FunctionType = 2 });
                }

                self.ActivityTimerList.RemoveAt(0);
            }

            TimerComponent.Instance.Remove(ref self.ActivityTimer);
            if (self.ActivityTimerList.Count > 0)
            {
                self.ActivityTimer = TimerComponent.Instance.NewOnceTimer(self.ActivityTimerList[0].BeginTime, TimerType.ActivityTipTimer, self);
            }
        }

        public static void InitFunctionButton(this ActivitySceneComponent self)
        {
            self.ActivityTimerList.Clear();
            Log.Warning("InitFunctionButton");
            long serverTime = TimeHelper.ServerNow();
            DateTime dateTime = TimeInfo.Instance.ToDateTime(serverTime);
            long curTime = (dateTime.Hour * 60 + dateTime.Minute) * 60 + dateTime.Second;
            TimerComponent.Instance.Remove(ref self.ActivityTimer);
            ///1025 战场 1043家族boss 1044家族争霸  1045竞技 1052狩猎活动  1055喜从天降  1057小龟大赛  1058奔跑比赛 1059恶魔活动
            List<int> functonIds = ConfigData.FunctionOpenIds;
            for (int i = 0; i < functonIds.Count; i++)
            {
                long startTime = FunctionHelp.GetOpenTime(functonIds[i]);
                long endTime = FunctionHelp.GetCloseTime(functonIds[i]);
                bool functionopne = FunctionHelp.IsFunctionDayOpen((int)dateTime.DayOfWeek, functonIds[i]);

                if (functonIds[i] == 2000)
                {
                    if (CommonHelper.IsInnerNet())
                    {
                        continue;
                    }
                    startTime = curTime + RandomHelper.NextLong(TimeHelper.Second * 5, TimeHelper.Hour * 10);
                }

                if (curTime < startTime)
                {
                    long sTime = serverTime + (startTime - curTime) * 1000;
                    self.ActivityTimerList.Add(new ActivityTimer() { FunctionId = functonIds[i], BeginTime = sTime, FunctionType = 1 });
                }
                if (curTime < endTime)
                {
                    long sTime = serverTime + (endTime - curTime) * 1000;
                    self.ActivityTimerList.Add(new ActivityTimer() { FunctionId = functonIds[i], BeginTime = sTime, FunctionType = 2 });
                }
                bool inTime = functionopne && curTime >= startTime && curTime <= endTime;
                if (inTime)
                {
                    self.ActivityTimerList.Add(new ActivityTimer() { FunctionId = functonIds[i], BeginTime = serverTime, FunctionType = 1 });
                }
            }

            if (self.ActivityTimerList.Count > 0)
            {
                self.ActivityTimerList.Sort(delegate (ActivityTimer a, ActivityTimer b)
                {
                    return (int)(a.BeginTime - b.BeginTime);
                });

                self.ActivityTimer = TimerComponent.Instance.NewOnceTimer(self.ActivityTimerList[0].BeginTime, TimerType.ActivityTipTimer, self);
            }
        }

        public static void SaveDB(this ActivitySceneComponent self)
        {
            DBHelper.SaveComponent(self.DomainZone(), self.DomainZone(), self.DBDayActivityInfo).Coroutine();
        }

        public static int OnGlobalShopBuyRequest(this ActivitySceneComponent self, int shopId, ShopGoodsItem mysteryInfo)
        {
            if (mysteryInfo == null || self.DBDayActivityInfo.GlobalRandomShops == null)
            {
                return ErrorCode.ERR_ItemNotEnoughError;
            }

            if (shopId > 0
                && self.DBDayActivityInfo.GlobalRandomShops.TryGetValue(shopId, out List<ShopGoodsItem> shopList))
            {
                return TryDeductGlobalShopStock(shopList, mysteryInfo);
            }

            foreach (List<ShopGoodsItem> list in self.DBDayActivityInfo.GlobalRandomShops.Values)
            {
                int error = TryDeductGlobalShopStock(list, mysteryInfo);
                if (error == ErrorCode.ERR_Success)
                {
                    return ErrorCode.ERR_Success;
                }
            }
            return ErrorCode.ERR_ItemNotEnoughError;
        }

        private static int TryDeductGlobalShopStock(List<ShopGoodsItem> shopList, ShopGoodsItem mysteryInfo)
        {
            for (int i = 0; i < shopList.Count; i++)
            {
                ShopGoodsItem mysteryItemInfo1 = shopList[i];
                if (mysteryItemInfo1.ShopGoodId != mysteryInfo.ShopGoodId)
                {
                    continue;
                }
                // ItemNumber>0 时扣全服库存；0 表示不限
                if (mysteryItemInfo1.ItemNumber > 0)
                {
                    if (mysteryItemInfo1.ItemNumber < mysteryInfo.ItemNumber)
                    {
                        return ErrorCode.ERR_ItemNotEnoughError;
                    }
                    mysteryItemInfo1.ItemNumber -= mysteryInfo.ItemNumber;
                }
                return ErrorCode.ERR_Success;
            }
            return ErrorCode.ERR_ItemNotEnoughError;
        }

        private static async ETTask CreateRobot(this ActivitySceneComponent self, int openServerDay)
        {
            await TimerComponent.Instance.WaitAsync(TimeHelper.Second * 10 + self.DomainZone());

            int createRobotNumber = 10;
            long robotSceneId = DBHelper.GetRobotServerId();
            MessageHelper.SendActor(robotSceneId, new G2Robot_MessageRequest() { Zone = self.DomainZone(), MessageType = NoticeType.CreateRobot, Message = $"1#{createRobotNumber}" });
        }

        public static async ETTask NoticeActivityUpdate_Hour(this ActivitySceneComponent self, DateTime dateTime)
        {
            DayOfWeek dayOfWeek = dateTime.DayOfWeek;
            int hour = dateTime.Hour;
            int openServerDay = DBHelper.GetOpenServerDay(self.DomainZone());
            LogHelper.LogWarning($"NoticeActivityUpdate_Hour: zone: {self.DomainZone()} openday: {openServerDay}  {hour}", true);

            for (int i = 0; i < self.MapIdList.Count; i++)
            {
                Other2A_ActivityUpdateResponse m2m_TrasferUnitResponse = (Other2A_ActivityUpdateResponse)await ActorMessageSenderComponent.Instance.Call
                        (self.MapIdList[i], new A2Other_ActivityUpdateRequest() { Hour = hour, OpenDay = openServerDay });
            }

            if (hour == 0)
            {
                LogHelper.LogWarning($"全服随机商店刷新: {self.DomainZone()}", true);
                self.InitGlobalRandomShop();
                self.DBDayActivityInfo.PetMingHexinList.Clear();

                self.InitPetMineExtend();
                self.InitFunctionButton();
            }
            
            if (!CommonHelper.IsInnerNet() && self.DomainZone() != 3 && hour == 6)
            {
                Log.Warning($"刷新机器人: {self.DomainZone()}");
                self.CreateRobot(openServerDay).Coroutine();
            }

            if (hour == 0 && dayOfWeek == DayOfWeek.Monday)
            {
                Console.WriteLine($"限时活动清空");
                self.DBDayActivityInfo.FeedPlayerList.Clear();
                self.DBDayActivityInfo.FeedRewardKey = 0;
                self.DBDayActivityInfo.BaoShiDu = 0;
                self.DBDayActivityInfo.OpenGuessIds.Clear();
            }

        }
    }
}
