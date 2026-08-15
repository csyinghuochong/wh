using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{
    [ObjectSystem]
    public class FubenCenterComponentAwakeSystem : AwakeSystem<FubenCenterComponent>
    {
        public override void Awake(FubenCenterComponent self)
        {
            self.FubenInstanceList.Clear();
            self.YeWaiFubenList.Clear();
            self.BattleInfos.Clear();
            self.BattleOpen = true;

            self.InitYeWaiScene().Coroutine();
        }
    }

    public static class FubenCenterComponentSystem
    {
        public static int GetScenePlayer(this FubenCenterComponent self, long instanced)
        { 
            foreach((long id, Entity Entity) in self.Children)
            {
                if (Entity.InstanceId != instanced)
                {
                    continue;
                }
                return UnitHelper.GetUnitList(Entity as Scene, UnitType.Player).Count;
            }
            return 0;
        }

        public static void OnActivityOpen(this FubenCenterComponent self, int functionId)
        {
            if (functionId == 1025)
            {
                //self.OnBattleOpen();
                return;
            }
            
            //Log.Console($"OnActivityOpen: {functionId}");
        }

        public static void OnActivityClose(this FubenCenterComponent self, int functionId)
        {
            //if (functionId == 1025)
            //{
            //    //self.OnBattleOver().Coroutine();
            //    return;
            //}
            
            self.DisposeFuben(functionId).Coroutine();
            //Log.Console($"OnActivityClose: {functionId}");
        }

        public static long GetFunctionFubenId(this FubenCenterComponent self, int functionId, long unitId)
        {
            Dictionary<long, List<long>> playerList = null;
            if (playerList == null)
            {
                return 0;
            }

            foreach ((long id, List<long> players) in playerList)
            {
                Scene scene = self.GetChild<Scene>(id);
                if (scene == null)
                {
                    Log.Error("scene == null");
                    continue;
                }

                if (players.Contains(unitId))
                {
                    return scene.InstanceId;
                }

                if (players.Count < 20)
                {
                    players.Add(unitId);
                    return scene.InstanceId;
                }
            }

            //动态创建副本.....RecastPathComponent.awake寻路
            int sceneid = 0;
            if (functionId == 1058)
            {
                sceneid = BattleHelper.GetSceneIdByType(MapTypeEnum.RunRace);
            }
            if (sceneid == 0)
            {
                return 0;
            }

            LDScene ldScene = LDSceneCategory.Instance.Get(sceneid);
            long fubenid = IdGenerater.Instance.GenerateId();
            long fubenInstanceId = IdGenerater.Instance.GenerateInstanceId();
            Log.Warning($"GenarateFuben2.{fubenInstanceId}");

            self.FubenInstanceList.Add(fubenInstanceId);
            //self.YeWaiFubenList.Add(sceneConfig.Id, fubenInstanceId);  可能有多个不能这样搞

            Scene fubnescene = SceneFactory.Create(self, fubenid, fubenInstanceId, self.DomainZone(), "Fuben" + ldScene.Id.ToString(), SceneType.Map);
            MapComponent mapComponent = fubnescene.GetComponent<MapComponent>();
            mapComponent.SetMapInfo(ldScene.Scene_Type, ldScene.Id, 0);
            mapComponent.NavMeshId = ldScene.GetNavMeshId();
            Game.Scene.GetComponent<RecastPathComponent>().Update(mapComponent.NavMeshId);
            YeWaiRefreshComponent yeWaiRefreshComponen = fubnescene.AddComponent<YeWaiRefreshComponent>();
            yeWaiRefreshComponen.SceneId = ldScene.Id;

            switch (ldScene.Scene_Type)
            {
                case MapTypeEnum.RunRace:
                    RunRaceDungeonComponent runRaceDungeon = fubnescene.AddComponent<RunRaceDungeonComponent>();
                    runRaceDungeon.OnBegin();
                    break;
                default:
                    break;
            }

            //FubenHelp.CreateMonsterList(fubnescene, ldScene.CreateMonster);
            //FubenHelp.CreateMonsterList(fubnescene, ldScene.CreateMonsterPosi);

            playerList.Add( fubenid, new List<long>() { unitId } );

            return fubenInstanceId;
        }


        /// <summary>
        /// 活动关闭 ，一段时间后销毁副本
        /// </summary>
        /// <param name="self"></param>
        /// <param name="functionId"></param>
        /// <returns></returns>
        public static async ETTask DisposeFuben(this FubenCenterComponent self, int functionId)
        {
            long waitDisposeTime = 0;


            await TimerComponent.Instance.WaitAsync(waitDisposeTime);

            //foreach ( (long id, Entity Entity) in self.Children)
            //{
            //    if (Entity.GetComponent<MapComponent>()== null)
            //    {
            //        continue;
            //    }
               
            //    if (!playerList.Remove(Entity.Id))
            //    {
            //        continue;
            //    }

            //    Log.Warning($"DisposeFubenId; {functionId} {Entity.Id}");

            //    long instanceid = Entity.InstanceId;
            //    if (self.FubenInstanceList.Remove(instanceid))
            //    {
            //        Log.Warning($"DisposeFubenInstance; {functionId}  {instanceid}");
            //    }
              
            //    Scene scene = Entity as Scene;
            //    C2M_TransferRequest actor_Transfer = new C2M_TransferRequest()
            //    {
            //        SceneType = MapTypeEnum.MainCityScene,
            //    };
            //    List<Unit> units = scene.GetComponent<UnitComponent>().GetAll();
            //    for (int i = 0; i < units.Count; i++)
            //    {
            //        if (units[i].Type != UnitType.Player)
            //        {
            //            continue;
            //        }
            //        if (units[i].IsDisposed || units[i].IsRobot())
            //        {
            //            continue;
            //        }
            //        TransferHelper.TransferUnit(units[i], actor_Transfer).Coroutine();
            //    }

            //    await TimerComponent.Instance.WaitAsync(60000 + RandomHelper.RandomNumber(0, 1000));
            //    scene.Dispose();
            //    break;
            //}
        }

        public static void OnBattleOpen(this FubenCenterComponent self)
        {
            self.BattleOpen = true;
            LogHelper.LogWarning($"OnBattleOpen : {self.DomainZone()}", true);
            //if (DBHelper.GetOpenServerDay(self.DomainZone()) > 0)
            //{
            //    long robotSceneId = DBHelper.GetRobotServerId();
            //    MessageHelper.SendActor(robotSceneId, new G2Robot_MessageRequest() { Zone = self.DomainZone(), MessageType = NoticeType.BattleOpen });
            //}
        }

        public static async ETTask OnBattleOver(this FubenCenterComponent self)
        {
            self.BattleOpen = false;
            LogHelper.LogDebug($"OnBattleOver : {self.DomainZone()}");
            long robotSceneId = DBHelper.GetRobotServerId();
            MessageHelper.SendActor(robotSceneId, new G2Robot_MessageRequest() { Zone = self.DomainZone(), MessageType = NoticeType.BattleOver });

            for (int i = 0; i < self.BattleInfos.Count; i++)
            {
                BattleInfo battleInfo = self.BattleInfos[i];
                try
                {
                    FubenWork2M_ExitResponse exitResponse = (FubenWork2M_ExitResponse)await ActorMessageSenderComponent.Instance.Call(
                          battleInfo.ProgressId, new M2FubenWork_ExitRequest()
                          {
                              SceneType = MapTypeEnum.Battle,
                              FubenId = battleInfo.FubenId,
                              Camp1Player = battleInfo.Camp1Player,
                              Camp2Player = battleInfo.Camp2Player,
                          });
                    if (exitResponse.Error != ErrorCode.ERR_Success)
                    {
                        Log.Error($"OnBattleOver ExitError: {battleInfo.FubenId} {exitResponse.Error}");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
                battleInfo.Dispose();
            }
            self.BattleInfos.Clear();
        }

        static int GetBattlePlayerLimit(int sceneId)
        {
            if (!LDSceneCategory.Instance.Contain(sceneId))
            {
                return 40;
            }
            int limit = LDSceneCategory.Instance.Get(sceneId).Limit_Player;
            return limit > 0 ? limit : 40;
        }

        static int AddBattlePlayer(BattleInfo battleInfo, long unitid)
        {
            battleInfo.PlayerNumber++;
            int camp = battleInfo.PlayerNumber % 2 + 1;
            if (camp == 1)
            {
                battleInfo.Camp1Player.Add(unitid);
            }
            else
            {
                battleInfo.Camp2Player.Add(unitid);
            }
            return camp;
        }

        public static KeyValuePairInt GetBattleInstanceId(this FubenCenterComponent self, long unitid, int sceneId)
        {
            if (!self.BattleOpen)
            {
                return new KeyValuePairInt() { KeyId = 0, Value = 0 };
            }

            int playerLimit = GetBattlePlayerLimit(sceneId);
            BattleInfo assignable = null;
            for (int i = 0; i < self.BattleInfos.Count; i++)
            {
                BattleInfo battleInfo = self.BattleInfos[i];
                if (battleInfo.SceneId != sceneId)
                {
                    continue;
                }
                if (battleInfo.Camp1Player.Contains(unitid))
                {
                    return new KeyValuePairInt() { KeyId = 1, Value = battleInfo.FubenInstanceId };
                }
                if (battleInfo.Camp2Player.Contains(unitid))
                {
                    return new KeyValuePairInt() { KeyId = 2, Value = battleInfo.FubenInstanceId };
                }
                if (assignable == null && battleInfo.PlayerNumber < playerLimit)
                {
                    assignable = battleInfo;
                }
            }

            if (assignable != null)
            {
                int camp = AddBattlePlayer(assignable, unitid);
                return new KeyValuePairInt() { KeyId = camp, Value = assignable.FubenInstanceId };
            }

            return null;
        }

        public static async ETTask<KeyValuePairInt> GenerateBattleInstanceId(this FubenCenterComponent self, long unitid, int sceneId)
        {
            if (!self.BattleOpen)
            {
                return null;
            }

            StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.GetRandomFubenWork(self.DomainZone());
            FubenWork2M_EnterResponse createUnit = (FubenWork2M_EnterResponse)await ActorMessageSenderComponent.Instance.Call(
                      startSceneConfig.InstanceId, new M2FubenWork_EnterRequest()
                      {
                          UserID = unitid,
                          SceneType = MapTypeEnum.Battle,
                          SceneId = sceneId,
                          TransferId = 0,
                          Difficulty = 0
                      });

            if (createUnit.Error != ErrorCode.ERR_Success || createUnit.FubenInstanceId == 0)
            {
                return null;
            }

            BattleInfo battleInfo = self.AddChild<BattleInfo>();
            battleInfo.ProgressId = startSceneConfig.InstanceId;
            battleInfo.FubenId = createUnit.FubenId;
            battleInfo.PlayerNumber = 0;
            battleInfo.FubenInstanceId = createUnit.FubenInstanceId;
            battleInfo.SceneId = sceneId;
            int camp = AddBattlePlayer(battleInfo, unitid);
            self.BattleInfos.Add(battleInfo);
            return new KeyValuePairInt() { KeyId = camp, Value = battleInfo.FubenInstanceId };
        }

        public static async ETTask  InitYeWaiScene(this FubenCenterComponent self)
        {
            await TimerComponent.Instance.WaitAsync(RandomHelper.RandomNumber(0, 1000));
           
            foreach (LDScene sceneConfig in LDSceneCategory.Instance.GetAll().Values)
            {
                if (sceneConfig.Scene_Type != MapTypeEnum.BaoZangZhiDi 
                && sceneConfig.Scene_Type != MapTypeEnum.MiJing )
                {
                    continue;
                }

                //动态创建副本.....RecastPathComponent.awake寻路
                long fubenid = IdGenerater.Instance.GenerateId();
                long fubenInstanceId = IdGenerater.Instance.GenerateInstanceId();

                self.FubenInstanceList.Add(fubenInstanceId);
                self.YeWaiFubenList.Add(sceneConfig.Id, fubenInstanceId);

                Scene fubnescene = SceneFactory.Create(self, fubenid, fubenInstanceId, self.DomainZone(), "YeWai" + sceneConfig.Id.ToString(), SceneType.Map);
                MapComponent mapComponent = fubnescene.GetComponent<MapComponent>();
                mapComponent.SetMapInfo(sceneConfig.Scene_Type, sceneConfig.Id, 0);
                mapComponent.NavMeshId = sceneConfig.GetNavMeshId(); 
                YeWaiRefreshComponent yeWaiRefreshComponen = fubnescene.AddComponent<YeWaiRefreshComponent>();
                yeWaiRefreshComponen.SceneId = sceneConfig.Id;
                
                switch (sceneConfig.Scene_Type)
                {
                    case MapTypeEnum.MiJing:
                        fubnescene.AddComponent<MiJingComponent>();
                        break;
                    default:
                        break;
                }

                //FubenHelp.CreateMonsterList(fubnescene, sceneConfigs[i].CreateMonster);
                //FubenHelp.CreateMonsterList(fubnescene, sceneConfigs[i].CreateMonsterPosi);

                int openDay = DBHelper.GetOpenServerDay(self.DomainZone());
                yeWaiRefreshComponen.OnZeroClockUpdate(openDay);
            }
        }
    }
}
