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
            PlayerSessionLifecycleHelper.OnRelogin(self, gateSessionId);
        }

        public static  void OnOffLine(this DBSaveComponent self)
        {
            PlayerSessionLifecycleHelper.OnOffLine(self);
        }

        public static void OnLogin(this DBSaveComponent self)
        {
            PlayerSessionLifecycleHelper.OnLogin(self);
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
            return PlayerSessionLifecycleHelper.OnDisconnect(self);
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

            PlayerTickOrchestrator.RunSecondTick(self.GetParent<Unit>());
        }

        public static bool MinuteCheck(this DBSaveComponent self)
        {
            PlayerTickOrchestrator.RunMinuteTick(self.GetParent<Unit>(), self);
            return false;
        }
    }
}
