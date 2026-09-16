using AlibabaCloud.SDK.Sample;
using NLog.Fluent;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{

    [ActorMessageHandler]
    public class A2Other_ActivityUpdateHandler : AMActorRpcHandler<Scene, A2Other_ActivityUpdateRequest, Other2A_ActivityUpdateResponse>
    {

        protected override async ETTask Run(Scene scene, A2Other_ActivityUpdateRequest request, Other2A_ActivityUpdateResponse response, Action reply)
        {
            int hour = request.Hour;
            switch (scene.SceneType)
            {
                case SceneType.Gate:
                    LogHelper.LogWarning($"Gate定时刷新: {scene.DomainZone()} {hour} ", true);
                    if (hour == 0)
                    {
                        PrintAllEntity();
                    }
                 
                    Player[] players = scene.GetComponent<PlayerComponent>().GetAll();
                    for (int i = 0; i < players.Length; i++)
                    {
                        if (players[i].PlayerState != PlayerState.Game)
                        {
                            continue;
                        }
                        ActorLocationSenderComponent.Instance.Send(players[i].UnitId, new G2M_ActivityUpdate() { ActivityType = hour });
                    }

                    break;
                case SceneType.Map:
                   
                    break;
                case SceneType.Rank:
                    //Log.Console($"排行榜定时刷新: {scene.DomainZone()} {hour}");
                    LogHelper.LogWarning($"排行榜定时刷新: {scene.DomainZone()} {hour}", true);
                    if (hour == 0)
                    {
                        scene.GetComponent<RankSceneComponent>().OnZeroClockUpdate();
                    }
                    if (hour == 12)
                    {
                        scene.GetComponent<RankSceneComponent>().OnHour12Update();
                    }
                    if (request.FunctionId == 1052 && request.FunctionType == 1)
                    {
                        //Log.Console("OnShowLieBegin");
                        Log.Warning("OnShowLieBegin");
                        scene.GetComponent<RankSceneComponent>().OnShowLieBegin();
                    }
                    break;
                case SceneType.FubenWork:
                    //Log.Console($"Arena定时刷新: {scene.DomainZone()} {hour}");
                    LogHelper.LogWarning($"Arena定时刷新: {scene.DomainZone()} {hour}", true);
                    if (request.FunctionId == 1055 && request.FunctionType == 1)
                    {
                        scene.GetComponent<HappySceneComponent>().OnHappyBegin();
                    }
                    if (request.FunctionId == 1055 && request.FunctionType == 2)
                    {
                        scene.GetComponent<HappySceneComponent>().OnHappyOver();
                    }
                    break;
                case SceneType.Union:
                    //Log.Console($"Union定时刷新: {scene.DomainZone()} {hour}");
                    LogHelper.LogWarning($"Union定时刷新: {scene.DomainZone()} {hour}", true);
                    if (hour == 0)
                    {
                        scene.GetComponent<UnionSceneComponent>().OnZeroClockUpdate();
                    }
                    if (request.FunctionId == 1043 && request.FunctionType == 1)
                    {
                        //Log.Console("OnUnionBoss");
                        scene.GetComponent<UnionSceneComponent>().OnUnionBoss();
                    }
                    if (request.FunctionId == 1044 && request.FunctionType == 1)
                    {
                        //Log.Console("OnUnionRaceBegin");
                        scene.GetComponent<UnionSceneComponent>().OnUnionRaceBegin().Coroutine();
                    }
                    if (request.FunctionId == 1044 && request.FunctionType == 2)
                    {
                        //Log.Console("UnionSceneComponent.OnUnionRaceOver");
                        //scene.GetComponent<UnionSceneComponent>().OnUnionRaceOver().Coroutine();
                    }
                    break;
                case SceneType.Consign:
                    //更新快捷购买列表价格
                    //Log.Console($"PaiMai定时刷新: {scene.DomainZone()} {hour}");
                    LogHelper.LogWarning($"PaiMai定时刷新: {scene.DomainZone()} {hour}", true);
                    if (hour == 0)
                    {
                        scene.GetComponent<ConsignSceneComponent>().OnZeroClockUpdate();
                    }
                    break;
                case SceneType.DBCache:
                    //if (!ComHelp.IsInnerNet())
                    //{
                    //    scene.GetComponent<DBCacheComponent>().CheckUnitCacheList();
                    //}
                    break;
                case SceneType.FubenCenter:
                    if (hour == 0)
                    {
                        //Log.Console($"FubenCenter定时刷新: {scene.DomainZone()} {hour}");
                        LogHelper.LogWarning($"FubenCenter定时刷新: {scene.DomainZone()} {hour}", true);
                        FubenCenterComponent fubenCenter = scene.GetComponent<FubenCenterComponent>();
                        foreach (var item in fubenCenter.Children)
                        {
                            YeWaiRefreshComponent yeWaiRefresh = item.Value.GetComponent<YeWaiRefreshComponent>();
                            if (yeWaiRefresh == null)
                            {
                                continue;
                            }
                            yeWaiRefresh.OnZeroClockUpdate(request.OpenDay);
                        }
                    }
                    if (request.FunctionId > 0 && request.FunctionType == 1)
                    {
                        //Log.Console($"GenarateFuben.{request.FunctionId}");
                        FubenCenterComponent fubenCenter = scene.GetComponent<FubenCenterComponent>();
                        fubenCenter.OnActivityOpen(request.FunctionId);
                    }
                    if (request.FunctionId > 0 && request.FunctionType == 2)
                    {
                        //Log.Console($"DisposeFuben.{request.FunctionId}");
                        FubenCenterComponent fubenCenter = scene.GetComponent<FubenCenterComponent>();
                        fubenCenter.OnActivityClose(request.FunctionId);
                    }

                    Log.Error($"FubenCenterComponent:  {request.FunctionId}");

                    if (request.FunctionId == 1045 && request.FunctionType == 1)
                    {
                        //scene.GetComponent<SoloSceneComponent>().OnSoloBegin().Coroutine();
                    }
                    if (request.FunctionId == 1045 && request.FunctionType == 2)
                    {
                        //scene.GetComponent<SoloSceneComponent>().OnSoloOver().Coroutine();
                    }
                    break;
                case SceneType.Realm:
                    Log.Error($"SceneType.Realm -1");
                    break;
                default:
                    break;
            }

            reply();
            await ETTask.CompletedTask;
        }

        private void PrintAllEntity()
        {
            Log.Info("PrintAllEntity");
            Log.Info(EventSystem.Instance.ToString());
            Log.Info(ObjectPool.Instance.ToString());
        }
    }
}
