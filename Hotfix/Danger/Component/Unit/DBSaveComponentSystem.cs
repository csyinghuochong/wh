using System;

namespace ET
{
    [Timer(TimerType.DBSaveTimer)]
    public class DBSaveTimer : ATimer<DBSaveComponent>
    {
        public override void Run(DBSaveComponent self)
        {
            try
            {
                self.SceondCheck();
            }
            catch (Exception e)
            {
                Log.Error($"move timer error: {self.Id}\n{e}");
            }
        }
    }

    [ObjectSystem]
    public class DBSaveComponentAwakeSystem : AwakeSystem<DBSaveComponent>
    {
        public override void Awake(DBSaveComponent self)
        {
            self.DBInterval = -1;
            self.NoFindPath = 0;
            self.EntityChangeTypeSet.Clear();
        }
    }

    [ObjectSystem]
    public class DBSaveComponentDestroySystem : DestroySystem<DBSaveComponent>
    {
        public override void Destroy(DBSaveComponent self)
        {
            TimerComponent.Instance?.Remove(ref self.Timer);
        }
    }

    [ObjectSystem]
    public class UnitGetComponentSystem : GetComponentSystem<Unit>
    {
        public override void GetComponent(Unit unit, Entity componet)
        {
            Type type = componet.GetType();
            
            if (!typeof(IUnitCache).IsAssignableFrom(type))
            {
                return;
            }
            unit.GetComponent<DBSaveComponent>()?.AddChange(type);
        }
    }

    public static class DBSaveComponentSystem
    {
       
        public static void AddChange(this DBSaveComponent self, Type t)
        {
            self.EntityChangeTypeSet.Add(t);
        }

        public static void UpdateCacheDB(this DBSaveComponent self)
        {
            try
            {
                if (self.EntityChangeTypeSet.Count <= 0)
                {
                    return;
                }
                Unit unit = self.GetParent<Unit>();
                if (unit.IsRobot())
                {
                    return;
                }

                long dbCacheId = DBHelper.GetUnitCacheConfig(unit.Id);
                M2D_SaveUnit message = new M2D_SaveUnit() { UnitId = unit.Id };
                
                message.EntityTypes.Add(unit.GetType().FullName);
                message.EntityBytes.Add(MongoHelper.ToBson(unit));
                
                foreach (Type type in self.EntityChangeTypeSet)
                {
                    Entity entity = unit.GetComponent(type);
                    if (entity == null || entity.IsDisposed)
                    {
                        continue;
                    }
                    message.EntityTypes.Add(type.FullName);
                    message.EntityBytes.Add(MongoHelper.ToBson(entity));
                }
             
                self.EntityChangeTypeSet.Clear();
                MessageHelper.CallActor(dbCacheId, message).Coroutine();
            }
            catch (Exception ex)
            {
                Log.Error("更新缓存服Unit数据出错: " + ex.ToString());
            }
        }

        public static void OnRelogin(this DBSaveComponent self, long gateSessionId)
        {
            Unit unit = self.GetParent<Unit>();
            RoleInfoComponentServer roleInfo = unit.GetComponent<RoleInfoComponentServer>();
            string offLineInfo = $"{unit.DomainZone()}区： " +
               $"unit.id: {roleInfo.Id} : " +
               $" {roleInfo.RoleInfo.Name} : " +
               $"{TimeHelper.DateTimeNow().ToString()}   二次登陆";

            if (!unit.IsRobot())
            {
                LogHelper.LoginInfo(offLineInfo);
                //需要通知其他服务器吗？
                Log.Debug(offLineInfo);
            }
            UnitGateComponent unitGateComponent = unit.GetComponent<UnitGateComponent>();
            unitGateComponent.PlayerState = PlayerState.Game;
        }

        public static  void OnOffLine(this DBSaveComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            string offLineInfo = $"{unit.DomainZone()}区： " +
               $"unit.id: {roleInfoComponentServer.Id} : " +
               $" {roleInfoComponentServer.RoleInfo.Name} : " +
               $"{TimeHelper.DateTimeNow().ToString()}   离线";

            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            long stallId = numericComponent.GetAsLong(NumericType.Now_Stall);
            if (stallId > 0)
            {
                Unit unitstall = unit.GetParent<UnitComponent>().Get(stallId);
                if (unitstall != null)
                {
                    unitstall.AddComponent<DeathTimeComponent, long>(TimeHelper.Hour * 6);
                }
            }

            DataCollationComponent dataCollationComponent = unit.GetComponent<DataCollationComponent>();
            string oaid = dataCollationComponent.OAID;
            string lastgametime =   TimeHelper.DateTimeNow().ToString();
            numericComponent.ApplyValue(NumericType.LastGameTime, TimeHelper.ServerNow(), false);
            roleInfoComponentServer.OnOffLine();
            dataCollationComponent.OnOffLine(lastgametime);
            UnitGateComponent unitGateComponent = unit.GetComponent<UnitGateComponent>();
            unitGateComponent.PlayerState = PlayerState.None;
            if (!unit.IsRobot())
            {
                LogHelper.LoginInfo(offLineInfo);
                Log.Warning(offLineInfo);
                self.UpdateCacheDB();
                DBHelper.UpdateLastGameTime(oaid, 
                    lastgametime,
                    roleInfoComponentServer.RoleInfo.AccInfoID,
                    roleInfoComponentServer.RemoteAddress,
                    roleInfoComponentServer.RoleInfo.Lv,
                    self.OnLineTime).Coroutine();
            }
        }

        public static void OnLogin(this DBSaveComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            RoleInfoComponentServer roleInfo = unit.GetComponent<RoleInfoComponentServer>();
            string offLineInfo = $"{unit.DomainZone()}区： " +
               $"unit.id: {roleInfo.Id} : " +
               $" {roleInfo.RoleInfo.Name} : " +
               $"{  TimeHelper.DateTimeNow().ToString()}   登录";
            if (!unit.IsRobot())
            {
                LogHelper.LoginInfo(offLineInfo);
                Log.Warning(offLineInfo);
                self.LogTest();
            }
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            //if (numericComponent.GetAsLong(NumericType.LastGameTime) == 0)
            //{
            //    numericComponent.ApplyValue(NumericType.LastGameTime, TimeHelper.ServerNow(), false);
            //}
            numericComponent.ApplyValue(NumericType.LastGameTime, TimeHelper.ServerNow(), false);
            UnitGateComponent unitGateComponent = unit.GetComponent<UnitGateComponent>();
            unitGateComponent.PlayerState = PlayerState.Game;
        }

        public static void LogTest(this DBSaveComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            if (unit.IsRobot())
            {
                return;
            }
            ActivityComponentServer activityComponentServer = unit.GetComponent<ActivityComponentServer>();
            LogHelper.LogDebug($"活动领取： {activityComponentServer.ActivityReceiveIds.Count}  {activityComponentServer.QuTokenRecvive.Count}");
        }

        public static int OnDisconnect(this DBSaveComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            RoleInfoComponentServer roleInfo = unit.GetComponent<RoleInfoComponentServer>();
            string offLineInfo = $"{unit.DomainZone()}区： " +
              $"unit.id: {roleInfo.Id} : " +
              $" {roleInfo.RoleInfo.Name} : " +
              $"{  TimeHelper.DateTimeNow().ToString()}  移除";

            Scene scene = unit.DomainScene();
            int sceneTypeEnum = scene.GetComponent<MapComponent>().MapTypeEnum;
            if (sceneTypeEnum == MapTypeEnum.MainCityScene)
            {
                unit.RecordPostion(sceneTypeEnum, CommonHelper.MainCityID());
            }

            TransferHelper.BeforeTransfer(unit, 2);
            if (!unit.IsRobot())
            {
                self.LogTest();
                self.UpdateCacheDB();
                LogHelper.LoginInfo(offLineInfo);
                LogHelper.LogDebug(offLineInfo);
            }

            long unitId = unit.Id;
            unit.GetParent<UnitComponent>().Remove(unitId);

            Game.EventSystem.Publish(new EventType.PlayerDisconnect() { DomainScene = scene, UnitId = unitId });
           
            return ErrorCode.ERR_Success;
        }

        /// <summary>
        /// 异常退出
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static async ETTask OnKickPlayer(this Unit unit, bool other)
        {
            await unit.RemoveLocation();

            if (other)
            {
                //通知Chat服
                await ServerMessageHelper.SendServerMessage(DBHelper.GetChatServerId(unit), NoticeType.PlayerExit, unit.Id.ToString());
                //通知其他服
            }

            DBSaveComponent dBSaveComponent = unit.GetComponent<DBSaveComponent>();
            if (dBSaveComponent != null)
            {
                dBSaveComponent.OnDisconnect();
            }
            else
            {
                unit.GetParent<UnitComponent>().Remove(unit.Id);
            }
        }

        public static void Activeted(this DBSaveComponent self)
        {
            TimerComponent.Instance?.Remove(ref self.Timer);
            self.Timer = TimerComponent.Instance.NewRepeatedTimer(TimeHelper.Second, TimerType.DBSaveTimer, self);

            //Console.WriteLine($" self.SceondIndex: {self.SceondIndex} ");
        }

        //public static void Check_2(this DBSaveComponent self)
        //{
        //    if (self.LastDBTime == 0)
        //    {
        //        return;
        //    }
        //    if (TimeHelper.ServerNow() - self.LastDBTime >= TimeHelper.Minute)
        //    {
        //        self.Check();
        //    }
        //}

        public static void SceondCheck(this DBSaveComponent self)
        {
            self.SceondIndex++;
            if (self.SceondIndex >= 60)
            { 
                self.SceondIndex = 0;
                self.MinuteCheck();
            }

            Unit unit = self.GetParent<Unit>();
            UnitGateComponent unitGateComponent = unit.GetComponent<UnitGateComponent>();
            if (unitGateComponent.PlayerState!= PlayerState.None)
            {
                unit.GetComponent<ActivityComponentServer>().Check();
            }
        }

        public static bool MinuteCheck(this DBSaveComponent self)
        {
            //self.LastDBTime = TimeHelper.ServerNow();
            Unit unit = self.GetParent<Unit>();
            /*if (self.NoFindPath >= 60)
            {
                self.NoFindPath = 0;
                M2C_KickPlayerMessage m2C_KickPlayer = new M2C_KickPlayerMessage();
                MessageHelper.SendToClient(unit, m2C_KickPlayer);

                Log.Debug($"MinuteCheck:  {unit.DomainZone()} {unit.Id}");
                unit.OnKickPlayer(false).Coroutine();
            }
            self.NoFindPath++;*/
            int saveInterval = RandomHelper.RandomNumber(20, 30);
            if (self.DBInterval == -1 || self.DBInterval >= saveInterval)
            {
                self.DBInterval = 0;
                self.UpdateCacheDB();
            }
            self.DBInterval++;
            self.OnLineTime++;
            TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            DataCollationComponent dataCollationComponent = unit.GetComponent<DataCollationComponent>();
            TitleComponentServer titleComponentServer = unit.GetComponent<TitleComponentServer>();
            taskComponentServer.Check();
            roleInfoComponentServer.Check();
            dataCollationComponent.Check();
            titleComponentServer.OnCheckTitle(true);
            return false;
        }
    }
}
