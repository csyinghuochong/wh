using NLog.Fluent;
using System;
using System.Collections.Generic;

namespace ET
{
    public static class TransferHelper
    {
        public static async ETTask<int> TransferUnit(Unit unit, TransferArgs args)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Transfer, unit.Id, 0))
            {
                if (unit.IsDisposed)
                {
                    return ErrorCode.ERR_RequestRepeatedly;
                }
                Scene unitScene = unit.DomainScene();
                MapComponent unitMapComponent = unitScene.GetComponent<MapComponent>();
                int oldScene = unitMapComponent.MapTypeEnum;
                if (!SceneConfigHelper.CanTransfer(oldScene, args.SceneType))
                {
                    Log.Debug($"LoginTest1  Actor_Transfer unitId{unit.Id} oldScene:{oldScene}  requestscene{args.SceneType}");
                    return ErrorCode.ERR_RequestRepeatedly;
                }
                // 跨服旅游中只允许回本服主城；其它玩法须先回本服
                if (UnitZoneHelper.IsTouring(unit) && args.SceneType != MapTypeEnum.MainCityScene)
                {
                    Log.Warning($"[WarZoneTour] 旅游中禁止传送 unit={unit.Id} home={UnitZoneHelper.GetHomeZone(unit)} cur={UnitZoneHelper.GetCurrentZone(unit)} to={args.SceneType}");
                    return ErrorCode.ERR_RequestRepeatedly;
                }
                RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
                RoleDailyDataComponentServer dailyData = unit.GetComponent<RoleDailyDataComponentServer>();
                int recordSceneId = 0;
                if (SceneConfigHelper.UseSceneConfig(args.SceneType) && args.SceneId > 0 && !LDSceneCategory.Instance.Contain(args.SceneId))
                {
                    return ErrorCode.ERR_TimesIsNot;
                }

                int targetSceneId = SceneConfigHelper.ResolveTransferSceneId(args.SceneType, args.SceneId, args.ParamInfo);
                // 组队副本按实际实例校验，在到达副本后记录首次进入次数。
                if (args.SceneType != MapTypeEnum.TeamDungeon
                    && SceneConfigHelper.UseSceneConfig(args.SceneType) && targetSceneId > 0 && LDSceneCategory.Instance.Contain(targetSceneId))
                {
                    LDScene ldScene = LDSceneCategory.Instance.Get(targetSceneId);
                    int enterError = ldScene.CheckSceneEnter(
                        roleInfoComponentServer.RoleInfo.Lv,
                        dailyData.GetSceneFubenTimes(targetSceneId),
                        dailyData.GetSceneWeekFubenTimes(targetSceneId));
                    if (enterError != ErrorCode.ERR_Success)
                    {
                        return enterError;
                    }

                    if (dailyData.AddSceneEnterTimes(targetSceneId))
                    {
                        recordSceneId = targetSceneId;
                    }
                }

                //进入失败 回退次数
                int Fail(int error)
                {
                    if (recordSceneId > 0)
                    {
                        dailyData.RollbackSceneEnterTimes(recordSceneId);
                        recordSceneId = 0;
                    }

                    return error;
                }
                if (oldScene == MapTypeEnum.MainCityScene && args.SceneType > MapTypeEnum.MainCityScene)
                {
                    unit.RecordPostion(args.SceneType, args.SceneId);
                }

                int transferError;
                switch (args.SceneType)
                {
                    case MapTypeEnum.MainCityScene:
                        int mainCityError = await TransferHelper.MainCityTransfer(unit);
                        if (mainCityError != ErrorCode.ERR_Success)
                        {
                            return Fail(mainCityError);
                        }
                        break;
                 
                    //宠物闯关
                    case (int)MapTypeEnum.PetDungeon:
                        int petfubenid = int.Parse(args.ParamInfo);
                        
                        Scene oldscene = unitScene;
                        MapComponent mapComponent = unitMapComponent;
                        int sceneTypeEnum = mapComponent.MapTypeEnum;
                        long fubenid = IdGenerater.Instance.GenerateId();
                        long fubenInstanceId = IdGenerater.Instance.GenerateInstanceId();
                        Scene fubnescene = SceneFactory.Create(Game.Scene, fubenid, fubenInstanceId, UnitZoneHelper.GetHomeZone(unit), "PetFuben" + fubenid.ToString(), SceneType.Map);
                        fubnescene.AddComponent<PetFubenSceneComponent>();
                        fubnescene.GetComponent<MapComponent>().SetMapInfo((int)MapTypeEnum.PetDungeon, args.SceneId, petfubenid);
                        TransferHelper.BeforeTransfer(unit);
                        transferError = await TransferHelper.Transfer(unit, fubenInstanceId,
                            new TransferArgs(sceneType: (int)MapTypeEnum.PetDungeon, sceneId: args.SceneId,
                                difficulty: FubenDifficulty.None, paramInfo: args.ParamInfo));
                        if (transferError != ErrorCode.ERR_Success)
                        {
                            fubnescene.Dispose();
                            return Fail(transferError);
                        }
                        TransferHelper.NoticeFubenCenter(fubnescene, 1).Coroutine();
                        if (SceneConfigHelper.IsSingleFuben(sceneTypeEnum))
                        {
                            TransferHelper.NoticeFubenCenter(oldscene, 2).Coroutine();
                            oldscene.Dispose();
                        }
                        break;
                    case (int)MapTypeEnum.TrialDungeon:
                        fubenid = IdGenerater.Instance.GenerateId();
                        fubenInstanceId = IdGenerater.Instance.GenerateInstanceId();
                        fubnescene = SceneFactory.Create(Game.Scene, fubenid, fubenInstanceId, UnitZoneHelper.GetHomeZone(unit), "TrialDungeon" + fubenid.ToString(), SceneType.Map);
                        fubnescene.AddComponent<TrialDungeonComponent>();
                        mapComponent = fubnescene.GetComponent<MapComponent>();
                        mapComponent.SetMapInfo((int)MapTypeEnum.TrialDungeon, args.SceneId, int.Parse(args.ParamInfo));
                        mapComponent.NavMeshId = LDSceneCategory.Instance.Get(args.SceneId).GetNavMeshId();
                        TransferHelper.BeforeTransfer(unit);
                        transferError = await TransferHelper.Transfer(unit, fubenInstanceId,
                            new TransferArgs(sceneType: (int)MapTypeEnum.TrialDungeon, sceneId: args.SceneId,
                                difficulty: FubenDifficulty.None, paramInfo: args.ParamInfo));
                        if (transferError != ErrorCode.ERR_Success)
                        {
                            fubnescene.Dispose();
                            return Fail(transferError);
                        }
                        TransferHelper.NoticeFubenCenter(fubnescene, 1).Coroutine();
                        break;

                    case (int)MapTypeEnum.Union:
                        long unionid = unit.GetComponent<NumericComponent>().GetAsLong(NumericType.UnionId_0);
                        if (unionid == 0)
                        {
                            return Fail(ErrorCode.ERR_Union_Not_Exist);
                        }
                        long mapInstanceId = DBHelper.GetUnionServerId(unit);
                        U2M_UnionEnterResponse responseUnionEnter = (U2M_UnionEnterResponse)await ActorMessageSenderComponent.Instance.Call(
                        mapInstanceId, new M2U_UnionEnterRequest() { UnionId = unionid, UnitId = unit.Id, SceneId = args.SceneId });
                        TransferHelper.BeforeTransfer(unit);
                        transferError = await TransferHelper.Transfer(unit, responseUnionEnter.FubenInstanceId,
                            new TransferArgs(sceneType: (int)MapTypeEnum.Union, sceneId: args.SceneId,
                                difficulty: args.Difficulty, paramInfo: "0"));
                        if (transferError != ErrorCode.ERR_Success)
                        {
                            return Fail(transferError);
                        }
                        break;
                    case (int)MapTypeEnum.Home:
                        //动态创建副本
                        Scene scene = unit.DomainScene();
                        mapInstanceId = DBHelper.GetHomeServerId(unit);
                        ///进入之前先刷新一下
                        if (long.Parse(args.ParamInfo) == unit.Id)
                        {
                            HomeComponentServer homeComponentServer = unit.GetComponent<HomeComponentServer>();
                            homeComponentServer.OnBeforEnter();
                            await DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(unit), unit.Id, homeComponentServer);
                        }
                        Home2M_EnterResponse j2M_JianYuanEnterResponse = (Home2M_EnterResponse)await ActorMessageSenderComponent.Instance.Call(
                        mapInstanceId, new M2Home_EnterRequest() { MasterId = long.Parse(args.ParamInfo), UnitId = unit.Id, SceneId = args.SceneId });
                        TransferHelper.BeforeTransfer(unit);
                        transferError = await TransferHelper.Transfer(unit, j2M_JianYuanEnterResponse.FubenInstanceId,
                            new TransferArgs(sceneType: (int)MapTypeEnum.Home, sceneId: args.SceneId,
                                difficulty: args.Difficulty, paramInfo: "0"));
                        if (transferError != ErrorCode.ERR_Success)
                        {
                            return Fail(transferError);
                        }

                        if (oldScene == MapTypeEnum.Home && scene.InstanceId != j2M_JianYuanEnterResponse.FubenInstanceId)
                        {
                            HomeSceneComponent homeSceneComponent = scene.GetParent<HomeSceneComponent>();
                            homeSceneComponent.OnUnitLeave(scene);
                        }
                        break;
                    case (int)MapTypeEnum.TowerDungeon:
                        //动态创建副本
                        fubenid = IdGenerater.Instance.GenerateId();
                        fubenInstanceId = IdGenerater.Instance.GenerateInstanceId();
                        fubnescene = SceneFactory.Create(Game.Scene, fubenid, fubenInstanceId, UnitZoneHelper.GetHomeZone(unit), "Tower" + fubenid.ToString(), SceneType.Map);
                        fubnescene.AddComponent<TowerComponent>().FubenDifficulty = args.Difficulty;
                        mapComponent = fubnescene.GetComponent<MapComponent>();
                        mapComponent.SetMapInfo((int)MapTypeEnum.TowerDungeon, args.SceneId, 0);
                        mapComponent.NavMeshId = LDSceneCategory.Instance.Get(args.SceneId).GetNavMeshId();
                        TransferHelper.BeforeTransfer(unit);
                        transferError = await TransferHelper.Transfer(unit, fubenInstanceId,
                            new TransferArgs(sceneType: (int)MapTypeEnum.TowerDungeon, sceneId: args.SceneId,
                                difficulty: args.Difficulty, paramInfo: "0"));
                        if (transferError != ErrorCode.ERR_Success)
                        {
                            fubnescene.Dispose();
                            return Fail(transferError);
                        }
                        TransferHelper.NoticeFubenCenter(fubnescene, 1).Coroutine();
                        break;
                    case MapTypeEnum.OneChallenge:
                        fubenid = long.Parse(args.ParamInfo);
                        fubnescene = Game.Scene.Get(fubenid);
                        bool newdungeon = false;
                        if (fubnescene == null)
                        {
                            newdungeon = true;
                            fubenInstanceId = IdGenerater.Instance.GenerateInstanceId();
                            fubnescene = SceneFactory.Create(Game.Scene, fubenid, fubenInstanceId, UnitZoneHelper.GetHomeZone(unit), "OneChallenge" + fubenid.ToString(), SceneType.Map);
                            mapComponent = fubnescene.GetComponent<MapComponent>();
                            mapComponent.SetMapInfo((int)MapTypeEnum.OneChallenge, args.SceneId, 0);
                            mapComponent.NavMeshId = LDSceneCategory.Instance.Get(args.SceneId).GetNavMeshId();
                            Game.Scene.GetComponent<RecastPathComponent>().Update(mapComponent.NavMeshId);
                        }
                        fubenInstanceId = fubnescene.InstanceId;
                        TransferHelper.BeforeTransfer(unit);
                        transferError = await TransferHelper.Transfer(unit, fubenInstanceId,
                            new TransferArgs(sceneType: (int)MapTypeEnum.OneChallenge, sceneId: args.SceneId,
                                difficulty: args.Difficulty, paramInfo: "0"));
                        if (transferError != ErrorCode.ERR_Success)
                        {
                            return Fail(transferError);
                        }
                        if (newdungeon)
                        {
                            TransferHelper.NoticeFubenCenter(fubnescene, 1).Coroutine();
                        }
                        break;
                    case (int)MapTypeEnum.PetTianTi:
                        ////动态创建副本
                        long enemyId = long.Parse(args.ParamInfo);
                        fubenid = IdGenerater.Instance.GenerateId();
                        fubenInstanceId = IdGenerater.Instance.GenerateInstanceId();
                        fubnescene = SceneFactory.Create(Game.Scene, fubenid, fubenInstanceId, UnitZoneHelper.GetHomeZone(unit), "Fuben" + fubenid.ToString(), SceneType.Map);
                        fubnescene.AddComponent<PetTianTiComponent>().EnemyId = enemyId;
                        fubnescene.GetComponent<MapComponent>().SetMapInfo((int)MapTypeEnum.PetTianTi, args.SceneId, 0);
                        TransferHelper.BeforeTransfer(unit);
                        transferError = await TransferHelper.Transfer(unit, fubenInstanceId,
                            new TransferArgs(sceneType: (int)MapTypeEnum.PetTianTi, sceneId: args.SceneId,
                                difficulty: 0, paramInfo: "0"));
                        if (transferError != ErrorCode.ERR_Success)
                        {
                            fubnescene.Dispose();
                            return Fail(transferError);
                        }
                        TransferHelper.NoticeFubenCenter(fubnescene, 1).Coroutine();
                        break;
                    case (int)MapTypeEnum.LocalDungeon:
                        int difficulty = args.Difficulty >= 1 && args.Difficulty <= 3 ? args.Difficulty : 1;
                        LocalDungeonComponent localDungeon = unit.DomainScene().GetComponent<LocalDungeonComponent>();
                        if (localDungeon != null)
                        {
                            difficulty = localDungeon.FubenDifficulty;
                        }

                        unit.GetComponent<SkillManagerComponent>()?.OnFinish(false);
                        int errorCode = await TransferHelper.LocalDungeonTransfer(unit,
                            new TransferArgs(sceneType: args.SceneType, sceneId: args.SceneId,
                                difficulty: difficulty, paramInfo: args.ParamInfo));
                        if (errorCode != ErrorCode.ERR_Success)
                        {
                            return Fail(errorCode);
                        }
                        
                        break;
                    case MapTypeEnum.Solo:
                        long soloServerId = DBHelper.GetSoloServerId(unit);
                        S2M_SoloEnterResponse d2GGetUnit = (S2M_SoloEnterResponse)await ActorMessageSenderComponent.Instance.Call(soloServerId, new M2S_SoloEnterRequest()
                        {
                            FubenId = long.Parse(args.ParamInfo)
                        });

                        if (d2GGetUnit.Error != ErrorCode.ERR_Success)
                        {
                            return Fail(d2GGetUnit.Error);
                        }
                        if (d2GGetUnit.FubenInstanceId == 0)
                        {
                            return Fail(ErrorCode.ERR_AlreadyFinish);
                        }
                        if ( !FunctionHelp.IsInTime(1045))
                        {
                            return Fail(ErrorCode.ERR_AlreadyFinish);
                        }
                        oldscene = unit.DomainScene();
                        mapComponent = oldscene.GetComponent<MapComponent>();
                        sceneTypeEnum = mapComponent.MapTypeEnum;
                        TransferHelper.BeforeTransfer(unit);
                        transferError = await TransferHelper.Transfer(unit, d2GGetUnit.FubenInstanceId,
                            new TransferArgs(sceneType: MapTypeEnum.Solo, sceneId: args.SceneId,
                                difficulty: 0, paramInfo: "0"));
                        if (transferError != ErrorCode.ERR_Success)
                        {
                            return Fail(transferError);
                        }
                        if (SceneConfigHelper.IsSingleFuben(sceneTypeEnum))
                        {
                            TransferHelper.NoticeFubenCenter(oldscene, 2).Coroutine();
                            oldscene.Dispose();
                        }
                        break;
                    case MapTypeEnum.UnionRace:
                        unionid = unit.GetComponent<NumericComponent>().GetAsLong(NumericType.UnionId_0);
                        if (unionid == 0)
                        {
                            return Fail(ErrorCode.ERR_Union_Not_Exist);
                        }
                        if (!FunctionHelp.IsInUnionRaceTime())
                        {
                            return Fail(ErrorCode.ERR_AlreadyFinish);
                        }
                        mapInstanceId = DBHelper.GetUnionServerId(unit);
                        responseUnionEnter = (U2M_UnionEnterResponse)await ActorMessageSenderComponent.Instance.Call(
                        mapInstanceId, new M2U_UnionEnterRequest() { OperateType = 1, UnionId = unionid, UnitId = unit.Id, SceneId = args.SceneId });
                        if (responseUnionEnter.FubenInstanceId == 0)
                        {
                            return Fail(ErrorCode.ERR_AlreadyFinish);
                        }
                        TransferHelper.BeforeTransfer(unit);
                        transferError = await TransferHelper.Transfer(unit, responseUnionEnter.FubenInstanceId,
                            new TransferArgs(sceneType: MapTypeEnum.UnionRace, sceneId: args.SceneId,
                                difficulty: 0, paramInfo: "0"));
                        if (transferError != ErrorCode.ERR_Success)
                        {
                            return Fail(transferError);
                        }
                        break;
                    case MapTypeEnum.Happy:
                        mapInstanceId = DBHelper.GetHappyServerId(unit);
                        H2M_HapplyEnterResponse happyEnter = (H2M_HapplyEnterResponse)await ActorMessageSenderComponent.Instance.Call(
                        mapInstanceId, new M2H_HapplyEnterRequest() { UnitId = unit.Id, SceneId = args.SceneId });
                        if (happyEnter.FubenInstanceId == 0)
                        {
                            return Fail(ErrorCode.ERR_AlreadyFinish);
                        }
                        TransferHelper.BeforeTransfer(unit);
                        transferError = await TransferHelper.Transfer(unit, happyEnter.FubenInstanceId,
                            new TransferArgs(sceneType: (int)MapTypeEnum.Happy, sceneId: args.SceneId,
                                difficulty: FubenDifficulty.Normal, paramInfo: happyEnter.Position.ToString()));
                        if (transferError != ErrorCode.ERR_Success)
                        {
                            return Fail(transferError);
                        }
                        break;
                    case MapTypeEnum.Battle:
                        mapInstanceId = DBHelper.GetFubenCenterId(unit);
                        FubenCenter2M_BattleEnterResponse battleEnter = (FubenCenter2M_BattleEnterResponse)await ActorMessageSenderComponent.Instance.Call(
                        mapInstanceId, new M2FubenCenter_BattleEnterRequest() { UserID = unit.Id, SceneId = args.SceneId });
                        if (battleEnter.FubenInstanceId == 0)
                        {
                            return Fail(ErrorCode.ERR_AlreadyFinish);
                        }

                        TransferHelper.BeforeTransfer(unit);
                        transferError = await TransferHelper.Transfer(unit, battleEnter.FubenInstanceId,
                            new TransferArgs(sceneType: (int)MapTypeEnum.Battle, sceneId: args.SceneId,
                                difficulty: FubenDifficulty.Normal, paramInfo: battleEnter.Camp.ToString()));
                        if (transferError != ErrorCode.ERR_Success)
                        {
                            return Fail(transferError);
                        }
                        break;
                    case (int)MapTypeEnum.TeamDungeon:
                        oldscene = unit.DomainScene();
                        mapComponent = oldscene.GetComponent<MapComponent>();
                        sceneTypeEnum = mapComponent.MapTypeEnum;
                        mapInstanceId = DBHelper.GetTeamServerId(unit);
                        // 副本在申请等待结束时已创建，这里只取实例并传送。
                        T2M_TeamDungeonEnterResponse createUnit = (T2M_TeamDungeonEnterResponse)await ActorMessageSenderComponent.Instance.Call(
                        mapInstanceId, new M2T_TeamDungeonEnterRequest() { UserID = roleInfoComponentServer.RoleInfo.UserId });
                        if (createUnit.Error != ErrorCode.ERR_Success)
                        {
                            return Fail(createUnit.Error);
                        }
                        if (createUnit.FubenInstanceId == 0
                            || createUnit.FubenId <= 0 || !LDSceneCategory.Instance.Contain(createUnit.FubenId))
                        {
                            return Fail(ErrorCode.ERR_TransferFailError);
                        }

                        LDScene teamScene = LDSceneCategory.Instance.Get(createUnit.FubenId);
                        int teamEnterError = teamScene.CheckSceneEnter(
                            roleInfoComponentServer.RoleInfo.Lv,
                            createUnit.HasEntered ? 0 : dailyData.GetSceneFubenTimes(createUnit.FubenId),
                            createUnit.HasEntered ? 0 : dailyData.GetSceneWeekFubenTimes(createUnit.FubenId));
                        if (teamEnterError != ErrorCode.ERR_Success)
                        {
                            return Fail(teamEnterError);
                        }

                        TransferHelper.BeforeTransfer(unit);
                        int teamTransferError = await TransferHelper.Transfer(unit, createUnit.FubenInstanceId,
                            new TransferArgs(sceneType: (int)MapTypeEnum.TeamDungeon, sceneId: createUnit.FubenId,
                                difficulty: createUnit.FubenType, paramInfo: "0"));
                        if (teamTransferError != ErrorCode.ERR_Success)
                        {
                            return Fail(teamTransferError);
                        }
                        if (SceneConfigHelper.IsSingleFuben(sceneTypeEnum))
                        {
                            TransferHelper.NoticeFubenCenter(oldscene, 2).Coroutine();
                            oldscene.Dispose();
                        }
                        break;
                    default:
                        break;
                }
            }
            return ErrorCode.ERR_Success;
        }

        public static async ETTask<int> MainCityTransfer(Unit unit)
        {
            Scene scene = unit.DomainScene();
            MapComponent mapComponent = scene.GetComponent<MapComponent>();
            int sceneTypeEnum = mapComponent.MapTypeEnum;
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            long userId = roleInfoComponentServer.RoleInfo.UserId;
            // 跨服旅游后当前区是目标服，回城用归属服
            long mapInstanceId = DBHelper.GetMainCityServerId(UnitZoneHelper.GetHomeZone(unit));
            //动态删除副本
            TransferHelper.BeforeTransfer(unit);
            int error = await TransferHelper.Transfer(unit, mapInstanceId,
                new TransferArgs(sceneType: (int)MapTypeEnum.MainCityScene, sceneId: SceneConfigHelper.MainCityID(),
                    difficulty: 0, paramInfo: "0"));
            if (error != ErrorCode.ERR_Success)
            {
                return error;
            }

            Game.EventSystem.Publish(new EventType.ReturnMainCity() { DomainScene = scene, UnitId = userId });
            return ErrorCode.ERR_Success;
        }

        public static async ETTask<int> LocalDungeonTransfer(Unit unit, TransferArgs args)
        {
            int sceneId = args.SceneId;
            int transferId = int.Parse(args.ParamInfo);
            if (transferId != 0 && !LDScene_TeleportCategory.Instance.Contain(transferId))
            {
                return ErrorCode.ERR_ModifyData;
            }
            
            long oldsceneid = unit.DomainScene().Id;
            List<StartSceneConfig> zonelocaldungeons = StartSceneConfigCategory.Instance.FuBenWorkScens[UnitZoneHelper.GetHomeZone(unit)];
            int n = (int)( (unit.Id / 99) % zonelocaldungeons.Count);

            //if (ComHelp.IsInnerNet())
            //{
            //    n = 0;
            //}

            StartSceneConfig startSceneConfig =  zonelocaldungeons[n];
            sceneId = transferId != 0 ? LDScene_TeleportCategory.Instance.Get(transferId).Scene_Target : sceneId;
            if (sceneId == 0)
            {
                Log.Error($"zonelocaldungeonsb:  unitid: {unit.Id}  n: {n}  transferId: {transferId} sceneId: {sceneId} ");
                return ErrorCode.ERR_NotFindLevel;
            }

            //Log.Console($"zonelocaldungeonsb:  unitid: {unit.Id}  n: {n}  transferId: {transferId} sceneId: {sceneId} ");
            FubenWork2M_EnterResponse createUnit = (FubenWork2M_EnterResponse)await ActorMessageSenderComponent.Instance.Call(
                        startSceneConfig.InstanceId, new M2FubenWork_EnterRequest()
                        { 
                            UserID = unit.Id, SceneType = MapTypeEnum.LocalDungeon, SceneId = sceneId, TransferId = transferId, Difficulty = args.Difficulty
                        });

            if (createUnit.Error != ErrorCode.ERR_Success)
            {
                return createUnit.Error;
            }

            TransferHelper.BeforeTransfer(unit);
            int transferError = await TransferHelper.Transfer(unit, createUnit.FubenInstanceId,
                new TransferArgs(sceneType: (int)MapTypeEnum.LocalDungeon, sceneId: sceneId,
                    difficulty: args.Difficulty, paramInfo: transferId.ToString()));
            if (transferError != ErrorCode.ERR_Success)
            {
                return transferError;
            }

            //移除旧scene
            Scene scene = Game.Scene.Get(oldsceneid);
            if (scene.InstanceId != createUnit.FubenInstanceId && scene.GetComponent<LocalDungeonComponent>() != null)
            {
                //动态删除副本
                TransferHelper.NoticeFubenCenter(scene, 2).Coroutine();
                scene.Dispose();
            }
            return ErrorCode.ERR_Success;   
        }

        public static async ETTask LocalDungeonTransfer_Old(Unit unit, int sceneId, int transferId, int difficulty)
        {
            //前往神秘之门
          
            long oldsceneid = unit.DomainScene().Id;
            long fubenid = IdGenerater.Instance.GenerateId();
            long fubenInstanceId = IdGenerater.Instance.GenerateInstanceId();
            Scene fubnescene = SceneFactory.Create(Game.Scene, fubenid, fubenInstanceId, UnitZoneHelper.GetHomeZone(unit), "LocalDungeon" + fubenid.ToString(), SceneType.Map);
            fubnescene.AddComponent<YeWaiRefreshComponent>();
            LocalDungeonComponent localDungeon = fubnescene.AddComponent<LocalDungeonComponent>();
            localDungeon.FubenDifficulty = difficulty;
            sceneId = transferId != 0 ? LDScene_TeleportCategory.Instance.Get(transferId).Id : sceneId;
            fubnescene.GetComponent<MapComponent>().SetMapInfo((int)MapTypeEnum.LocalDungeon, sceneId, 0);

            TransferHelper.BeforeTransfer(unit);
            int transferError = await TransferHelper.Transfer(unit, fubenInstanceId,
                new TransferArgs(sceneType: (int)MapTypeEnum.LocalDungeon, sceneId: sceneId,
                    difficulty: difficulty, paramInfo: transferId.ToString()));
            if (transferError != ErrorCode.ERR_Success)
            {
                fubnescene.Dispose();
                return;
            }
            TransferHelper.NoticeFubenCenter(fubnescene, 1).Coroutine();

            Scene scene = Game.Scene.Get(oldsceneid);
            if (scene.GetComponent<LocalDungeonComponent>()!=null)
            {
                //动态删除副本
                TransferHelper.NoticeFubenCenter(scene, 2).Coroutine();
                scene.Dispose();
            }
        }

        /// <summary>按服务端确定的实例和业务参数传送；必须等待返回后才能销毁旧场景。</summary>
        public static async ETTask<int> Transfer(Unit unit, long sceneInstanceId, TransferArgs args)
        {
            if (unit.IsDisposed || sceneInstanceId == 0)
            {
                return ErrorCode.ERR_TransferFailError;
            }

            UnitTransferSource source = new UnitTransferSource(unit);
            long transferId = IdGenerater.Instance.GenerateId();
            bool locked = false;
            bool sceneChanged = false;
            bool submitted = false;
            bool committed = false;
            try
            {
                unit.RemoveComponent<MailBoxComponent>();
                byte[] bytes = UnitTransferHelper.Serialize(unit);
                if (bytes == null)
                {
                    return ErrorCode.ERR_TransferFailError;
                }

                // 先让目标场景收齐分片，成功后才通知客户端切场景。
                int transferError = await UnitTransferHelper.Send(source.UnitId, sceneInstanceId, transferId, bytes);
                if (transferError != ErrorCode.ERR_Success)
                {
                    return transferError;
                }
                if (unit.InstanceId != source.InstanceId)
                {
                    return ErrorCode.ERR_TransferFailError;
                }

                // 批次 ID 作为锁凭证，超时的旧请求不能提交下一次传送。
                await LocationProxyComponent.Instance.Lock(source.UnitId, transferId);
                locked = true;
                if (unit.InstanceId != source.InstanceId)
                {
                    return ErrorCode.ERR_TransferFailError;
                }

                sceneChanged = true;
                MessageHelper.SendToClient(unit, new M2C_StartSceneChange
                {
                    SceneInstanceId = sceneInstanceId, SceneType = args.SceneType, ChapterId = args.SceneId,
                    Difficulty = args.Difficulty, ParamInfo = args.ParamInfo,
                });
                // 让出服务端一帧，不代表客户端已经完成切场景。
                await TimerComponent.Instance.WaitFrameAsync();
                if (unit.InstanceId != source.InstanceId)
                {
                    return ErrorCode.ERR_TransferFailError;
                }

                // 同场景只暂存原角色，目标创建失败时仍可恢复同一个对象。
                if (source.SceneInstanceId == sceneInstanceId)
                {
                    source.Detach();
                }

                submitted = true;
                M2M_UnitTransferResponse response = (M2M_UnitTransferResponse)await ActorMessageSenderComponent.Instance.Call(
                    sceneInstanceId, new M2M_UnitTransferRequest
                    {
                        UnitId = source.UnitId, TransferId = transferId, SceneType = args.SceneType,
                        ChapterId = args.SceneId, Difficulty = args.Difficulty, ParamInfo = args.ParamInfo,
                    });
                committed = response.Error == ErrorCode.ERR_Success;
                return response.Error;
            }
            catch (Exception ex)
            {
                Log.Error($"Transfer exception unitId={source.UnitId} transferId={transferId}: {ex}");
                if (submitted)
                {
                    // 目标先提交 Location 再回包；回包丢失时以实际路由为准。
                    long instanceId = await UnitTransferHelper.GetLocation(source.UnitId);
                    committed = instanceId != 0 && instanceId != source.InstanceId;
                }
                return committed ? ErrorCode.ERR_Success : ErrorCode.ERR_TransferFailError;
            }
            finally
            {
                if (committed)
                {
                    source.Dispose();
                }
                else if (!locked || await UnitTransferHelper.Commit(source.UnitId, transferId, source.InstanceId))
                {
                    await source.Restore(sceneChanged);
                }
                else
                {
                    // 路由已归其它角色，不能重新启用旧角色。
                    source.Dispose();
                }
            }
        }

        public static void AfterTransfer(Unit unit)
        {
            PetComponentServer petComponent = unit.GetComponent<PetComponentServer>();
            PetInfo fightId = petComponent.GetFightPet();
            if (fightId != null)
            {
                petComponent.UpdatePetAttribute(fightId, false);
                UnitFactory.CreatePet(unit, fightId);
            }
        }



        public static void BeforeTransfer(Unit unit,  int transfer = 1)
        {
            //删除unit,让其它进程发送过来的消息找不到actor，重发
            //Game.EventSystem.Remove(unitId);
            // 删除Mailbox,让发给Unit的ActorLocation消息重发

            unit.RemoveComponent<MailBoxComponent>();
            unit.GetComponent<DataCollationComponent>()?.UpdateData();
            unit.GetComponent<SkillPassiveComponent>()?.Stop();
            unit.GetComponent<BuffManagerComponent>().BeforeTransfer(transfer);
            unit.GetComponent<UnitLifeComponent>()?.OnKillZhaoHuan(null);
            RemovePetAndJingLing(unit);
        }

        public static void RemoveStall(Unit unit)
        {
            
        }

        public static void RemovePetAndJingLing(Unit unit)
        {
            UnitComponent unitComponent = unit.DomainScene().GetComponent<UnitComponent>();
            PetComponentServer petComponent = unit.GetComponent<PetComponentServer>();
            PetInfo fightId = petComponent.GetFightPet();
            if (fightId != null)
            {
                unitComponent.Remove(fightId.Id);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="operateType">1创建副本 2销毁副本</param>
        /// <returns></returns>
        public static async ETTask NoticeFubenCenter(Scene scene, int operateType)
        {
            long fubencenterId = DBHelper.GetFubenCenterId(scene.DomainZone());
            int sceneType = 0;
            MapComponent mapComponent = scene?.GetComponent<MapComponent>();
            if (mapComponent != null)
            {
                sceneType = mapComponent.MapTypeEnum;
            }
            M2F_FubenCenterOperateRequest request = new M2F_FubenCenterOperateRequest()
            {
                SceneType = sceneType,
                OperateType = operateType,
                FubenInstanceId = scene.InstanceId
            };
            F2M_FubenCenterOpenResponse response = (F2M_FubenCenterOpenResponse)await ActorMessageSenderComponent.Instance.Call(fubencenterId, request);
            if (operateType == 1)
            { 
                //scene.GetComponent<ServerInfoComponent>().ServerInfo = response.ServerInfo;
            }
        }


    }
}
