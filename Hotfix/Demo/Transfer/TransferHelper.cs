using NLog.Fluent;
using System;
using System.Collections.Generic;

namespace ET
{
    public static class TransferHelper
    {
        public static async ETTask<int> TransferUnit(Unit unit, C2M_TransferRequest request)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Transfer, unit.Id))
            {
                if (unit.IsDisposed)
                {
                    return ErrorCode.ERR_RequestRepeatedly;
                }
                int oldScene = unit.DomainScene().GetComponent<MapComponent>().MapTypeEnum;
                if (!SceneConfigHelper.CanTransfer(oldScene, request.SceneType))
                {
                    Log.Debug($"LoginTest1  Actor_Transfer unitId{unit.Id} oldScene:{oldScene}  requestscene{request.SceneType}");
                    return ErrorCode.ERR_RequestRepeatedly;
                }
                RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
                if (SceneConfigHelper.UseSceneConfig(request.SceneType) && request.SceneId > 0)
                {
                    if (!LDSceneCategory.Instance.Contain(request.SceneId))
                    {
                        return ErrorCode.ERR_TimesIsNot;
                    }

                    LDScene ldScene = LDSceneCategory.Instance.Get(request.SceneId);
                    /*if (ldScene.DayEnterNum > 0 && ldScene.DayEnterNum <= roleInfoComponent.GetSceneFubenTimes(request.SceneId))
                    {
                        return ErrorCode.ERR_TimesIsNot;
                    }
                    if (ldScene.EnterLv > roleInfoComponent.RoleInfo.Level)
                    {
                        return ErrorCode.ERR_LevelIsNot;
                    }*/
                    roleInfoComponentServer.AddSceneFubenTimes(request.SceneId);
                }
                if (oldScene == MapTypeEnum.MainCityScene && request.SceneType > MapTypeEnum.MainCityScene)
                {
                    unit.RecordPostion(request.SceneType, request.SceneId);
                }

                switch (request.SceneType)
                {
                    case MapTypeEnum.MainCityScene:
                        await TransferHelper.MainCityTransfer(unit);
                        break;
                    case (int)MapTypeEnum.CellDungeon:
                        break;
                    //宠物闯关
                    case (int)MapTypeEnum.PetDungeon:
                        int petfubenid = int.Parse(request.paramInfo);
                        
                        Scene oldscene = unit.DomainScene();
                        MapComponent mapComponent = oldscene.GetComponent<MapComponent>();
                        int sceneTypeEnum = mapComponent.MapTypeEnum;
                        long fubenid = IdGenerater.Instance.GenerateId();
                        long fubenInstanceId = IdGenerater.Instance.GenerateInstanceId();
                        Scene fubnescene = SceneFactory.Create(Game.Scene, fubenid, fubenInstanceId, unit.DomainZone(), "PetFuben" + fubenid.ToString(), SceneType.Map);
                        fubnescene.AddComponent<PetFubenSceneComponent>();
                        fubnescene.GetComponent<MapComponent>().SetMapInfo((int)MapTypeEnum.PetDungeon, request.SceneId, int.Parse(request.paramInfo));
                        TransferHelper.BeforeTransfer(unit);
                        await TransferHelper.Transfer(unit, fubenInstanceId, (int)MapTypeEnum.PetDungeon, request.SceneId, FubenDifficulty.None, request.paramInfo);
                        TransferHelper.NoticeFubenCenter(fubnescene, 1).Coroutine();
                        if (SceneConfigHelper.IsSingleFuben(sceneTypeEnum))
                        {
                            TransferHelper.NoticeFubenCenter(oldscene, 2).Coroutine();
                            oldscene.Dispose();
                        }
                        break;
                    case (int)MapTypeEnum.TrialDungeon:
                        int requestTowerId = int.Parse(request.paramInfo);
                        int passId = unit.GetComponent<NumericComponent>().GetAsInt(NumericType.TrialDungeonId);
                        if (passId == 0 && requestTowerId != 20001)
                        {
                            Log.Error($"试炼之地作弊3:{unit.DomainZone()} {unit.Id} {requestTowerId}   {passId}");
                            return ErrorCode.ERR_ModifyData;
                        }
                        if (passId != 0 && requestTowerId > passId + 1 )
                        {
                            Log.Error($"试炼之地作弊4:{unit.DomainZone()} {unit.Id} {requestTowerId}   {passId}");
                            return ErrorCode.ERR_ModifyData;
                        }
                        fubenid = IdGenerater.Instance.GenerateId();
                        fubenInstanceId = IdGenerater.Instance.GenerateInstanceId();
                        fubnescene = SceneFactory.Create(Game.Scene, fubenid, fubenInstanceId, unit.DomainZone(), "TrialDungeon" + fubenid.ToString(), SceneType.Map);
                        fubnescene.AddComponent<TrialDungeonComponent>();
                        mapComponent = fubnescene.GetComponent<MapComponent>();
                        mapComponent.SetMapInfo((int)MapTypeEnum.TrialDungeon, request.SceneId, int.Parse(request.paramInfo));
                        mapComponent.NavMeshId = LDSceneCategory.Instance.Get(request.SceneId).GetNavMeshId();
                        TransferHelper.BeforeTransfer(unit);
                        await TransferHelper.Transfer(unit, fubenInstanceId, (int)MapTypeEnum.TrialDungeon, request.SceneId, FubenDifficulty.None, request.paramInfo);
                        TransferHelper.NoticeFubenCenter(fubnescene, 1).Coroutine();
                        break;
                    case MapTypeEnum.SeasonTower:

                        //计算赛季之塔下一关
                        int seasonTowerid = unit.GetComponent<NumericComponent>().GetAsInt(NumericType.SeasonTowerId);
                        if (seasonTowerid == 0)
                        {
                            request.paramInfo = TowerHelper.GetFirstTowerIdByScene(MapTypeEnum.SeasonTower).ToString();
                        }
                        else
                        {
                            request.paramInfo = (seasonTowerid + 1).ToString();
                        }

                        fubenid = IdGenerater.Instance.GenerateId();
                        fubenInstanceId = IdGenerater.Instance.GenerateInstanceId();
                        fubnescene = SceneFactory.Create(Game.Scene, fubenid, fubenInstanceId, unit.DomainZone(), "SeasonTower" + fubenid.ToString(), SceneType.Map);
                        fubnescene.AddComponent<SeasonTowerComponent>();
                        mapComponent = fubnescene.GetComponent<MapComponent>();
                        mapComponent.SetMapInfo((int)MapTypeEnum.SeasonTower, request.SceneId, int.Parse(request.paramInfo));
                        mapComponent.NavMeshId = LDSceneCategory.Instance.Get(request.SceneId).GetNavMeshId();
                        TransferHelper.BeforeTransfer(unit);
                        await TransferHelper.Transfer(unit, fubenInstanceId, (int)MapTypeEnum.SeasonTower, request.SceneId, FubenDifficulty.None, request.paramInfo);
                        TransferHelper.NoticeFubenCenter(fubnescene, 1).Coroutine();
                        break;
                    case MapTypeEnum.TowerOfSeal:
                        int finished = unit.GetComponent<NumericComponent>().GetAsInt(NumericType.TowerOfSealFinished);
                        // 服务端再判断是否已经通关塔顶
                        if (finished >= 100)
                        {
                            return ErrorCode.ERR_TowerOfSealReachTop;
                        }

                        fubenid = IdGenerater.Instance.GenerateId();
                        fubenInstanceId = IdGenerater.Instance.GenerateInstanceId();
                        fubnescene = SceneFactory.Create(Game.Scene, fubenid, fubenInstanceId, unit.DomainZone(), "TowerOfSeal" + fubenid.ToString(), SceneType.Map);
                        fubnescene.AddComponent<TowerOfSealComponent>();
                        mapComponent = fubnescene.GetComponent<MapComponent>();
                        mapComponent.SetMapInfo((int)MapTypeEnum.TowerOfSeal, request.SceneId, int.Parse(request.paramInfo));
                        mapComponent.NavMeshId = LDSceneCategory.Instance.Get(request.SceneId).GetNavMeshId();
                        TransferHelper.BeforeTransfer(unit);
                        await TransferHelper.Transfer(unit, fubenInstanceId, (int)MapTypeEnum.TowerOfSeal, request.SceneId, FubenDifficulty.None, request.paramInfo);
                        TransferHelper.NoticeFubenCenter(fubnescene, 1).Coroutine();
                        break;
                    case (int)MapTypeEnum.RandomTower:
                        //2200001
                        fubenid = IdGenerater.Instance.GenerateId();
                        fubenInstanceId = IdGenerater.Instance.GenerateInstanceId();
                        fubnescene = SceneFactory.Create(Game.Scene, fubenid, fubenInstanceId, unit.DomainZone(), "RandomTower" + fubenid.ToString(), SceneType.Map);
                        fubnescene.AddComponent<RandomTowerComponent>();
                        mapComponent = fubnescene.GetComponent<MapComponent>();
                        mapComponent.SetMapInfo((int)MapTypeEnum.RandomTower, request.SceneId, 0);
                        mapComponent.NavMeshId = LDSceneCategory.Instance.Get(request.SceneId).GetNavMeshId();
                        TransferHelper.BeforeTransfer(unit);
                        await TransferHelper.Transfer(unit, fubenInstanceId, (int)MapTypeEnum.RandomTower, request.SceneId, 0, "0");
                        TransferHelper.NoticeFubenCenter(fubnescene, 1).Coroutine();
                        break;
                    case (int)MapTypeEnum.Union:
                        long unionid = unit.GetComponent<NumericComponent>().GetAsLong(NumericType.UnionId_0);
                        if (unionid == 0)
                        {
                            return ErrorCode.ERR_Union_Not_Exist;
                        }
                        long mapInstanceId = DBHelper.GetUnionServerId(unit.DomainZone());
                        U2M_UnionEnterResponse responseUnionEnter = (U2M_UnionEnterResponse)await ActorMessageSenderComponent.Instance.Call(
                        mapInstanceId, new M2U_UnionEnterRequest() { UnionId = unionid, UnitId = unit.Id, SceneId = request.SceneId });
                        TransferHelper.BeforeTransfer(unit);
                        await TransferHelper.Transfer(unit, responseUnionEnter.FubenInstanceId, (int)MapTypeEnum.Union, request.SceneId, request.Difficulty, "0");
                        break;
                    case (int)MapTypeEnum.JiaYuan:
                        //动态创建副本
                        Scene scene = unit.DomainScene();
                        mapInstanceId = DBHelper.GetJiaYuanServerId(unit.DomainZone());
                        ///进入之前先刷新一下
                        if (long.Parse(request.paramInfo) == unit.Id)
                        {
                            JiaYuanComponentServer jiaYuanComponentServer = unit.GetComponent<JiaYuanComponentServer>();
                            jiaYuanComponentServer.OnBeforEnter();
                            await DBHelper.SaveComponentCache(unit.DomainZone(), unit.Id, jiaYuanComponentServer);
                        }
                        J2M_JiaYuanEnterResponse j2M_JianYuanEnterResponse = (J2M_JiaYuanEnterResponse)await ActorMessageSenderComponent.Instance.Call(
                        mapInstanceId, new M2J_JiaYuanEnterRequest() { MasterId = long.Parse(request.paramInfo), UnitId = unit.Id, SceneId = request.SceneId });
                        TransferHelper.BeforeTransfer(unit);
                        await TransferHelper.Transfer(unit, j2M_JianYuanEnterResponse.FubenInstanceId, (int)MapTypeEnum.JiaYuan, request.SceneId, request.Difficulty, "0");

                        if (oldScene == MapTypeEnum.JiaYuan)
                        {
                            JiaYuanSceneComponent jiayuanSceneComponent = scene.GetParent<JiaYuanSceneComponent>();
                            jiayuanSceneComponent.OnUnitLeave(scene);
                        }
                        break;
                    case (int)MapTypeEnum.TowerDungeon:
                        //动态创建副本
                        fubenid = IdGenerater.Instance.GenerateId();
                        fubenInstanceId = IdGenerater.Instance.GenerateInstanceId();
                        fubnescene = SceneFactory.Create(Game.Scene, fubenid, fubenInstanceId, unit.DomainZone(), "Tower" + fubenid.ToString(), SceneType.Map);
                        fubnescene.AddComponent<TowerComponent>().FubenDifficulty = request.Difficulty;
                        mapComponent = fubnescene.GetComponent<MapComponent>();
                        mapComponent.SetMapInfo((int)MapTypeEnum.TowerDungeon, request.SceneId, 0);
                        mapComponent.NavMeshId = LDSceneCategory.Instance.Get(request.SceneId).GetNavMeshId();
                        TransferHelper.BeforeTransfer(unit);
                        await TransferHelper.Transfer(unit, fubenInstanceId, (int)MapTypeEnum.TowerDungeon, request.SceneId, request.Difficulty, "0");
                        TransferHelper.NoticeFubenCenter(fubnescene, 1).Coroutine();
                        break;
                    case MapTypeEnum.OneChallenge:
                        fubenid = long.Parse(request.paramInfo);
                        fubnescene = Game.Scene.Get(fubenid);
                        bool newdungeon = false;
                        if (fubnescene == null)
                        {
                            newdungeon = true;
                            fubenInstanceId = IdGenerater.Instance.GenerateInstanceId();
                            fubnescene = SceneFactory.Create(Game.Scene, fubenid, fubenInstanceId, unit.DomainZone(), "OneChallenge" + fubenid.ToString(), SceneType.Map);
                            mapComponent = fubnescene.GetComponent<MapComponent>();
                            mapComponent.SetMapInfo((int)MapTypeEnum.OneChallenge, request.SceneId, 0);
                            mapComponent.NavMeshId = LDSceneCategory.Instance.Get(request.SceneId).GetNavMeshId();
                            Game.Scene.GetComponent<RecastPathComponent>().Update(fubnescene.GetComponent<MapComponent>().NavMeshId);
                        }
                        fubenInstanceId = fubnescene.InstanceId;
                        TransferHelper.BeforeTransfer(unit);
                        await TransferHelper.Transfer(unit, fubenInstanceId, (int)MapTypeEnum.OneChallenge, request.SceneId, request.Difficulty, "0");
                        if (newdungeon)
                        {
                            TransferHelper.NoticeFubenCenter(fubnescene, 1).Coroutine();
                        }
                        break;
                    case (int)MapTypeEnum.PetMing:
                        long cdTime = unit.GetComponent<NumericComponent>().GetAsLong(NumericType.PetMineCDTime);
                        if (cdTime > TimeHelper.ServerNow())
                        {
                            return ErrorCode.ERR_InMakeCD;
                        }

                        string[] praminfos = request.paramInfo.Split('_');
                        fubenid = IdGenerater.Instance.GenerateId();
                        fubenInstanceId = IdGenerater.Instance.GenerateInstanceId();
                        fubnescene = SceneFactory.Create(Game.Scene, fubenid, fubenInstanceId, unit.DomainZone(), "Fuben" + fubenid.ToString(), SceneType.Map);
                        PetMingDungeonComponent petMingDungeon = fubnescene.AddComponent<PetMingDungeonComponent>();
                        petMingDungeon.MineType = request.Difficulty;
                        petMingDungeon.Position = int.Parse(praminfos[0]);
                        petMingDungeon.TeamId = int.Parse(praminfos[1]);
                        fubnescene.GetComponent<MapComponent>().SetMapInfo((int)MapTypeEnum.PetMing, request.SceneId, 0);
                        TransferHelper.BeforeTransfer(unit);
                        await TransferHelper.Transfer(unit, fubenInstanceId, (int)MapTypeEnum.PetMing, request.SceneId, request.Difficulty, praminfos[0]);
                        TransferHelper.NoticeFubenCenter(fubnescene, 1).Coroutine();
                        break;
                    case (int)MapTypeEnum.PetTianTi:
                        ////动态创建副本
                        long enemyId = long.Parse(request.paramInfo);
                        fubenid = IdGenerater.Instance.GenerateId();
                        fubenInstanceId = IdGenerater.Instance.GenerateInstanceId();
                        fubnescene = SceneFactory.Create(Game.Scene, fubenid, fubenInstanceId, unit.DomainZone(), "Fuben" + fubenid.ToString(), SceneType.Map);
                        fubnescene.AddComponent<PetTianTiComponent>().EnemyId = enemyId;
                        fubnescene.GetComponent<MapComponent>().SetMapInfo((int)MapTypeEnum.PetTianTi, request.SceneId, 0);
                        TransferHelper.BeforeTransfer(unit);
                        await TransferHelper.Transfer(unit, fubenInstanceId, (int)MapTypeEnum.PetTianTi, request.SceneId, 0, "0");
                        TransferHelper.NoticeFubenCenter(fubnescene, 1).Coroutine();
                        break;
                    case (int)MapTypeEnum.LocalDungeon:
                        if (request.Difficulty < 1 || request.Difficulty > 3)
                        {
                            request.Difficulty = 1;
                        }
                        
                        LocalDungeonComponent localDungeon = unit.DomainScene().GetComponent<LocalDungeonComponent>();

                        if (localDungeon != null && localDungeon.FubenDifficulty != request.Difficulty)
                        {
                            //int diff = Math.Max(localDungeon.FubenDifficulty, request.Difficulty);
                            request.Difficulty = localDungeon.FubenDifficulty;
                            //Console.WriteLine($"FubenDifficulty != request.Difficulty: {unit.Id}  {localDungeon.FubenDifficulty} {request.Difficulty}");
                        }
                        else
                        {
                            request.Difficulty = localDungeon != null ? localDungeon.FubenDifficulty : request.Difficulty;
                        }

                        unit.GetComponent<SkillManagerComponent>()?.OnFinish(false);
                        int errorCode = await TransferHelper.LocalDungeonTransfer(unit, request.SceneId, int.Parse(request.paramInfo), request.Difficulty);
                        if (errorCode != ErrorCode.ERR_Success)
                        {
                            return errorCode;
                        }
                        //if (unit.IsRobot() )
                        //{
                        //    await TransferHelper.LocalDungeonTransfer(unit, request.SceneId, int.Parse(request.paramInfo), request.Difficulty);
                        //}
                        //else
                        //{
                        //    await TransferHelper.LocalDungeonTransfer_Old(unit, request.SceneId, int.Parse(request.paramInfo), request.Difficulty);
                        //}
                        break;
                    case MapTypeEnum.BaoZangZhiDi:
                    case MapTypeEnum.MiJing:
                        F2M_YeWaiSceneIdResponse f2M_YeWaiSceneIdResponse = (F2M_YeWaiSceneIdResponse)await ActorMessageSenderComponent.Instance.Call(
                        DBHelper.GetFubenCenterId(unit.DomainZone()), new M2F_YeWaiSceneIdRequest() { SceneId = request.SceneId });
                        if (f2M_YeWaiSceneIdResponse.FubenInstanceId == 0)
                        {
                            return ErrorCode.ERR_MapLimit;
                        }

                        LDScene ldScene = LDSceneCategory.Instance.Get(request.SceneId);
                        int curPlayerNum = int.Parse(f2M_YeWaiSceneIdResponse.Message); // UnitHelper.GetUnitList(unit.DomainScene(), UnitType.Player).Count;
                        /*if (ldScene.PlayerLimit > 0 && ldScene.PlayerLimit <= curPlayerNum)
                        {
                            return ErrorCode.ERR_MapLimit;
                        }*/
                        TransferHelper.BeforeTransfer(unit);
                        await TransferHelper.Transfer(unit, f2M_YeWaiSceneIdResponse.FubenInstanceId, ldScene.Scene_Type, request.SceneId, 0, "0");
                        break;
                    case MapTypeEnum.RunRace:
                    case MapTypeEnum.Demon:
                        f2M_YeWaiSceneIdResponse = (F2M_YeWaiSceneIdResponse)await ActorMessageSenderComponent.Instance.Call(
                        DBHelper.GetFubenCenterId(unit.DomainZone()), new M2F_YeWaiSceneIdRequest() { SceneId = request.SceneId,UnitId = unit.Id  });
                        if (f2M_YeWaiSceneIdResponse.FubenInstanceId == 0)
                        {
                            return ErrorCode.ERR_AlreadyFinish;
                        }
                        ldScene = LDSceneCategory.Instance.Get(request.SceneId);
                        TransferHelper.BeforeTransfer(unit);
                        await TransferHelper.Transfer(unit, f2M_YeWaiSceneIdResponse.FubenInstanceId, ldScene.Scene_Type, request.SceneId, 0, "0");
                        break;
                    case MapTypeEnum.Solo:
                        long soloServerId = DBHelper.GetSoloServerId(unit.DomainZone());
                        S2M_SoloEnterResponse d2GGetUnit = (S2M_SoloEnterResponse)await ActorMessageSenderComponent.Instance.Call(soloServerId, new M2S_SoloEnterRequest()
                        {
                            FubenId = long.Parse(request.paramInfo)
                        });

                        if (d2GGetUnit.Error != ErrorCode.ERR_Success)
                        {
                            return d2GGetUnit.Error;
                        }
                        if (d2GGetUnit.FubenInstanceId == 0)
                        {
                            return ErrorCode.ERR_AlreadyFinish;
                        }
                        if ( !FunctionHelp.IsInTime(1045))
                        {
                            return ErrorCode.ERR_AlreadyFinish;
                        }
                        oldscene = unit.DomainScene();
                        mapComponent = oldscene.GetComponent<MapComponent>();
                        sceneTypeEnum = mapComponent.MapTypeEnum;
                        TransferHelper.BeforeTransfer(unit);
                        await TransferHelper.Transfer(unit, d2GGetUnit.FubenInstanceId, MapTypeEnum.Solo, request.SceneId, 0, "0");
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
                            return ErrorCode.ERR_Union_Not_Exist;
                        }
                        if (!FunctionHelp.IsInUnionRaceTime())
                        {
                            return ErrorCode.ERR_AlreadyFinish;
                        }
                        mapInstanceId = DBHelper.GetUnionServerId(unit.DomainZone());
                        responseUnionEnter = (U2M_UnionEnterResponse)await ActorMessageSenderComponent.Instance.Call(
                        mapInstanceId, new M2U_UnionEnterRequest() { OperateType = 1, UnionId = unionid, UnitId = unit.Id, SceneId = request.SceneId });
                        if (responseUnionEnter.FubenInstanceId == 0)
                        {
                            return ErrorCode.ERR_AlreadyFinish;
                        }
                        TransferHelper.BeforeTransfer(unit);
                        await TransferHelper.Transfer(unit, responseUnionEnter.FubenInstanceId, MapTypeEnum.UnionRace, request.SceneId, 0, "0");
                        break;
                    case MapTypeEnum.Happy:
                        mapInstanceId = DBHelper.GetHappyServerId(unit.DomainZone());
                        H2M_HapplyEnterResponse happyEnter = (H2M_HapplyEnterResponse)await ActorMessageSenderComponent.Instance.Call(
                        mapInstanceId, new M2H_HapplyEnterRequest() { UnitId = unit.Id, SceneId = request.SceneId });
                        if (happyEnter.FubenInstanceId == 0)
                        {
                            return ErrorCode.ERR_AlreadyFinish;
                        }
                        TransferHelper.BeforeTransfer(unit);
                        await TransferHelper.Transfer(unit, happyEnter.FubenInstanceId, (int)MapTypeEnum.Happy, request.SceneId, FubenDifficulty.Normal, happyEnter.Position.ToString());
                        break;
                    case MapTypeEnum.Battle:
                        mapInstanceId = DBHelper.GetFubenCenterId(unit.DomainZone());
                        FubenCenter2M_BattleEnterResponse battleEnter = (FubenCenter2M_BattleEnterResponse)await ActorMessageSenderComponent.Instance.Call(
                        mapInstanceId, new M2FubenCenter_BattleEnterRequest() { UserID = unit.Id, SceneId = request.SceneId });
                        if (battleEnter.FubenInstanceId == 0)
                        {
                            return ErrorCode.ERR_AlreadyFinish;
                        }

                        TransferHelper.BeforeTransfer(unit);
                        await TransferHelper.Transfer(unit, battleEnter.FubenInstanceId, (int)MapTypeEnum.Battle, request.SceneId, FubenDifficulty.Normal, battleEnter.Camp.ToString());
                        break;
                    case MapTypeEnum.Arena:
                        roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
                        ldScene = LDSceneCategory.Instance.Get(request.SceneId);
                        /*if (roleInfoComponent.RoleInfo.Level < ldScene.EnterLv)
                        {
                            return ErrorCode.ERR_LevelIsNot;
                        }*/

                        mapInstanceId = DBHelper.GetArenaServerId(unit.DomainZone());
                        Arena2M_ArenaEnterResponse areneEnter = (Arena2M_ArenaEnterResponse)await ActorMessageSenderComponent.Instance.Call(
                        mapInstanceId, new M2Arena_ArenaEnterRequest() { UserID = unit.Id, SceneId = request.SceneId });
                        if (areneEnter.Error != ErrorCode.ERR_Success || areneEnter.FubenInstanceId == 0)
                        {
                            return ErrorCode.ERR_AlreadyFinish;
                        }
                        TransferHelper.BeforeTransfer(unit);
                        await TransferHelper.Transfer(unit, areneEnter.FubenInstanceId, (int)MapTypeEnum.Arena, request.SceneId, FubenDifficulty.Normal, "0");
                        break;
                    case (int)MapTypeEnum.TeamDungeon:
                        oldscene = unit.DomainScene();
                        mapComponent = oldscene.GetComponent<MapComponent>();
                        sceneTypeEnum = mapComponent.MapTypeEnum;
                        mapInstanceId = StartSceneConfigCategory.Instance.GetBySceneName(unit.DomainZone(), Enum.GetName(SceneType.Team)).InstanceId;
                        //[创建副本Scene]
                        T2M_TeamDungeonEnterResponse createUnit = (T2M_TeamDungeonEnterResponse)await ActorMessageSenderComponent.Instance.Call(
                        mapInstanceId, new M2T_TeamDungeonEnterRequest() { UserID = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.UserId });
                        if (createUnit.Error != ErrorCode.ERR_Success)
                        {
                            return ErrorCode.ERR_TransferFailError;
                        }
                        TransferHelper.BeforeTransfer(unit);
                        await TransferHelper.Transfer(unit, createUnit.FubenInstanceId, (int)MapTypeEnum.TeamDungeon, createUnit.FubenId, createUnit.FubenType, "0");
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

        public static async ETTask MainCityTransfer(Unit unit)
        {
            MapComponent mapComponent = unit.DomainScene().GetComponent<MapComponent>();
            int sceneTypeEnum = mapComponent.MapTypeEnum;
            long userId = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.UserId;
            unit.GetComponent<UnitInfoComponent>().LastDungeonId = 0;
            //传送回主场景
            long mapInstanceId = DBHelper.GetMainCityServerId(unit.DomainZone());
            //动态删除副本
            Scene scene = unit.DomainScene();
            TransferHelper.BeforeTransfer(unit);
            await TransferHelper.Transfer(unit, mapInstanceId, (int)MapTypeEnum.MainCityScene, CommonHelper.MainCityID(), 0, "0");

            Game.EventSystem.Publish(new EventType.ReturnMainCity() { DomainScene = scene, UnitId = userId });
        }

        public static async ETTask<int> LocalDungeonTransfer(Unit unit, int sceneId, int transferId, int difficulty)
        {
            if (transferId != 0 && !LDScene_TeleportCategory.Instance.Contain(transferId))
            {
                return ErrorCode.ERR_ModifyData;
            }
            
            long oldsceneid = unit.DomainScene().Id;
            List<StartSceneConfig> zonelocaldungeons = StartSceneConfigCategory.Instance.FuBenWorkScens[unit.DomainZone()];
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
                            UserID = unit.Id, SceneType = MapTypeEnum.LocalDungeon, SceneId = sceneId, TransferId = transferId, Difficulty = difficulty
                        });

            if (createUnit.Error != ErrorCode.ERR_Success)
            {
                return createUnit.Error;
            }

            TransferHelper.BeforeTransfer(unit);
            await TransferHelper.Transfer(unit, createUnit.FubenInstanceId, (int)MapTypeEnum.LocalDungeon, sceneId, difficulty, transferId.ToString());

            //移除旧scene
            Scene scene = Game.Scene.Get(oldsceneid);
            if (scene.GetComponent<LocalDungeonComponent>() != null)
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
            if (LDSectionCategory.Instance.MysteryDungeonList.Contains(sceneId))
            {
                unit.GetComponent<UnitInfoComponent>().LastDungeonId = unit.DomainScene().GetComponent<MapComponent>().SceneId;
                unit.GetComponent<UnitInfoComponent>().LastDungeonPosition = unit.Position;
            }

            long oldsceneid = unit.DomainScene().Id;
            long fubenid = IdGenerater.Instance.GenerateId();
            long fubenInstanceId = IdGenerater.Instance.GenerateInstanceId();
            Scene fubnescene = SceneFactory.Create(Game.Scene, fubenid, fubenInstanceId, unit.DomainZone(), "LocalDungeon" + fubenid.ToString(), SceneType.Map);
            fubnescene.AddComponent<YeWaiRefreshComponent>();
            LocalDungeonComponent localDungeon = fubnescene.AddComponent<LocalDungeonComponent>();
            localDungeon.FubenDifficulty = difficulty;
            sceneId = transferId != 0 ? LDScene_TeleportCategory.Instance.Get(transferId).Id : sceneId;
            fubnescene.GetComponent<MapComponent>().SetMapInfo((int)MapTypeEnum.LocalDungeon, sceneId, 0);

            TransferHelper.BeforeTransfer(unit);
            await TransferHelper.Transfer(unit, fubenInstanceId, (int)MapTypeEnum.LocalDungeon, sceneId, difficulty, transferId.ToString());
            TransferHelper.NoticeFubenCenter(fubnescene, 1).Coroutine();

            Scene scene = Game.Scene.Get(oldsceneid);
            if (scene.GetComponent<LocalDungeonComponent>()!=null)
            {
                //动态删除副本
                TransferHelper.NoticeFubenCenter(scene, 2).Coroutine();
                scene.Dispose();
            }
        }

        public static async ETTask<int> TransferComponent(Unit unit, long sceneInstanceId, string component)
        {
            M2M_UnitTransfer_0_Request request_0 = new M2M_UnitTransfer_0_Request();
            request_0.Unit = unit;
            foreach ((Type key, Entity entity) in unit.Components)
            {
                if (!(entity is ITransfer))
                {
                    continue;
                }

                //request.Entitys.Add(entity);
                if (key.Name.Equals(component))
                {
                    request_0.EntityBytes.Add(MongoHelper.ToBson(entity));
                }
            }
            request_0.ParamInfo = component;
            M2M_UnitTransfer_0_Response response_0 = await ActorMessageSenderComponent.Instance.Call(sceneInstanceId, request_0) as M2M_UnitTransfer_0_Response;
            return response_0.Error;
        }

        /// <summary>
        /// 必须等待返回才能执行销毁场景的操作
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="sceneInstanceId"></param>
        /// <param name="sceneType"></param>
        /// <param name="sceneId"></param>
        /// <param name="paramInfo"> 
        /// SceneTypeEnum.PetDungeon SceneTypeEnum.TrialDungeon是副本id  SceneTypeEnum.TowerOfSeal是副本id 
        /// SceneTypeEnum.LocalDungeon 是传送idSceneTypeEnum.Happy是位置 SceneTypeEnum.Battle是阵营
        /// <returns></returns>
        public static async ETTask Transfer(Unit unit, long sceneInstanceId, int sceneType, int sceneId, int fubenDifficulty,  string paramInfo)
        {
            // 删除Mailbox,让发给Unit的ActorLocation消息重发
            unit.RemoveComponent<MailBoxComponent>();

            // 通知客户端开始切场景
            M2C_StartSceneChange m2CStartSceneChange = new M2C_StartSceneChange() {SceneInstanceId = sceneInstanceId, SceneType = sceneType, ChapterId = sceneId, Difficulty = fubenDifficulty, ParamInfo = paramInfo };
            MessageHelper.SendToClient(unit, m2CStartSceneChange);

            await TimerComponent.Instance.WaitFrameAsync();
            await TransferComponent(unit, sceneInstanceId, DBHelper.BagComponentServer);
            await TransferComponent(unit, sceneInstanceId, DBHelper.ChengJiuComponent);

            M2M_UnitTransferRequest request = new M2M_UnitTransferRequest();
            request.Unit = unit;
            foreach ((Type key, Entity entity) in unit.Components)
            {
                if (!(entity is ITransfer))
                {
                    continue;
                }
                if (key.Name.Equals(DBHelper.BagComponentServer)
                 || key.Name.Equals(DBHelper.ChengJiuComponent))
                {
                    continue;
                }
                //request.Entitys.Add(entity);
                request.EntityBytes.Add(MongoHelper.ToBson(entity));
            }
            request.SceneType = sceneType;
            request.ChapterId = sceneId;
            request.Difficulty = fubenDifficulty;
            request.ParamInfo = paramInfo;
          
            // location加锁
            
            //移到上面
            UnitComponent unitComponent = unit.GetParent<UnitComponent>();
            long oldInstanceId = unit.InstanceId;
            if (oldInstanceId == unit.InstanceId)
            {
                unitComponent.Remove(unit.Id);
            }
            //unit.Dispose();
            
            await LocationProxyComponent.Instance.Lock(unit.Id, oldInstanceId);
            M2M_UnitTransferResponse response = await ActorMessageSenderComponent.Instance.Call(sceneInstanceId, request) as M2M_UnitTransferResponse;
            await LocationProxyComponent.Instance.UnLock(unit.Id, oldInstanceId, response.NewInstanceId);
            
            /*long oldInstanceId = unit.InstanceId;
            if (oldInstanceId == unit.InstanceId)
            {
                unitComponent.Remove(unit.Id);
            }*/
            //unit.Dispose();
        }

        public static void AfterTransfer(Unit unit)
        {
            RolePetInfo fightId = unit.GetComponent<PetComponentServer>().GetFightPet();
            if (fightId != null)
            {
                unit.GetComponent<PetComponentServer>().UpdatePetAttribute(fightId, false);
                UnitFactory.CreatePet(unit, fightId);
            }
            int jinglingid  = unit.GetComponent<ChengJiuComponentServer>().JingLingId;
            if (jinglingid != 0)
            {
                long JingLingUnitId = UnitFactory.CreateJingLing(unit, jinglingid).Id;
                unit.GetComponent<ChengJiuComponentServer>().JingLingUnitId = JingLingUnitId;
            }
        }



        public static void BeforeTransfer(Unit unit,  int transfer = 1)
        {
            //删除unit,让其它进程发送过来的消息找不到actor，重发
            //Game.EventSystem.Remove(unitId);
            // 删除Mailbox,让发给Unit的ActorLocation消息重发

            if (ConfigData.CleanSkill)
            {
                unit.RemoveComponent<MailBoxComponent>();
                unit.GetComponent<DataCollationComponent>()?.UpdateData();
                unit.GetComponent<SkillPassiveComponent>()?.Stop();
                unit.GetComponent<BuffManagerComponent>().BeforeTransfer(transfer);
                unit.GetComponent<HeroDataComponent>().OnKillZhaoHuan(null);
                RemovePetAndJingLing(unit);
            }
        }

        public static void RemoveStall(Unit unit)
        {
            List<Unit> stallList = UnitHelper.GetUnitList( unit.DomainScene(), UnitType.Stall );
            for (int i = stallList.Count - 1; i>= 0; i--)
            {
                if (stallList[i].MasterId == unit.Id)
                {
                    unit.GetParent<UnitComponent>().Remove(stallList[i].Id);
                }
            }
        }

        public static void RemovePetAndJingLing(Unit unit)
        {
            UnitComponent unitComponent = unit.DomainScene().GetComponent<UnitComponent>();
            RolePetInfo fightId = unit.GetComponent<PetComponentServer>().GetFightPet();
            if (fightId != null)
            {
                unitComponent.Remove(fightId.Id);
            }
            long jinglingUnitId = unit.GetComponent<ChengJiuComponentServer>().JingLingUnitId;
            if (jinglingUnitId != 0 && unitComponent.Get(jinglingUnitId) != null)
            {
                unitComponent.Remove(jinglingUnitId);
            }
            unit.GetComponent<ChengJiuComponentServer>().JingLingUnitId = 0;
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
            if (scene!=null && scene.GetComponent<MapComponent>()!=null)
            {
                sceneType = scene.GetComponent<MapComponent>().MapTypeEnum;
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