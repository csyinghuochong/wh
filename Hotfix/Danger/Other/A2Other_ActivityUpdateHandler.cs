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

        private async ETTask TestSmss(Scene scene)
        {
            if (scene.DomainZone() == 3)
            {
                for (int i = 0; i < 2; i++)
                {

                    Console.WriteLine("SendSmsVerifyCode.Send 18319670288");
                    SendSmsVerifyCode.Send_2("18319670288", 1, 1);
                    await TimerComponent.Instance.WaitAsync(TimeHelper.Second * 20);
                    SendSmsVerifyCode.Send_2("18319670288", 1, 2);
                    await TimerComponent.Instance.WaitAsync(TimeHelper.Second * 20);
                    SendSmsVerifyCode.Send_2("18319670288", 2, 1);
                    await TimerComponent.Instance.WaitAsync(TimeHelper.Second * 20);
                    SendSmsVerifyCode.Send_2("18319670288", 2, 2);
                    await TimerComponent.Instance.WaitAsync(TimeHelper.Second * 20);

                    Console.WriteLine("SendSmsVerifyCode.Send 18652422521");
                    SendSmsVerifyCode.Send_2("18652422521", 1, 1);
                    await TimerComponent.Instance.WaitAsync(TimeHelper.Second * 20);
                    SendSmsVerifyCode.Send_2("18652422521", 1, 2);
                    await TimerComponent.Instance.WaitAsync(TimeHelper.Second * 20);
                    SendSmsVerifyCode.Send_2("18652422521", 2, 1);
                    await TimerComponent.Instance.WaitAsync(TimeHelper.Second * 20);
                    SendSmsVerifyCode.Send_2("18652422521", 2, 2);

                    await TimerComponent.Instance.WaitAsync(TimeHelper.Second * 20);
                    Console.WriteLine("SendSmsVerifyCode.Send 15172796169");
                    SendSmsVerifyCode.Send_2("15172796169", 1, 1);
                    await TimerComponent.Instance.WaitAsync(TimeHelper.Second * 20);
                    SendSmsVerifyCode.Send_2("15172796169", 1, 2);
                    await TimerComponent.Instance.WaitAsync(TimeHelper.Second * 20);
                    SendSmsVerifyCode.Send_2("15172796169", 2, 1);
                    await TimerComponent.Instance.WaitAsync(TimeHelper.Second * 20);
                    SendSmsVerifyCode.Send_2("15172796169", 2, 2);
                }
            }
        }

        private async ETTask TestSmssNew(Scene scene)
        {
            if (scene.DomainZone() == 3)
            {
                for (int i = 0; i < 2; i++)
                {
                    //移动
                    Console.WriteLine("SendSmsVerifyCode.Send 18319670288");
                    Sample.Send("18319670288", 1, 1);
                    await TimerComponent.Instance.WaitAsync(TimeHelper.Minute * 2);
                    Sample.Send("18319670288", 1, 2);
                    await TimerComponent.Instance.WaitAsync(TimeHelper.Minute * 2);
                }
            }

        }

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
                    
                    if (CommonHelper.IsInnerNet())
                    {
                        //TestSmss(scene).Coroutine();
                        //TestSmssNew(scene).Coroutine();
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

                    if (request.Hour == 20 && scene.DomainZone() == 3)
                    {
                        //Console.WriteLine("gongzuoshi3 0");
                        //Game.EventSystem.Publish(new EventType.GMCommonRequest() { Context = "gongzuoshi3 0" });
                    }

                    if (request.Hour == 23 && scene.DomainZone() == 3)
                    {
                        //打印所有拍卖大于特定值
                        string command = "paimai2 0 50000000";
                        Console.WriteLine(command);
                        Game.EventSystem.Publish(new EventType.GMCommonRequest() { Context = command });

                        //清理今日拍卖
                        string filePath = "../Logs/WJ_PaiMai.txt";
                        LogHelper.WriteLogList(new List<string>() { "" }, filePath, false);

                        //清理聊天记录
                        //string filePath_2 = "../Logs/WJ_Chat.txt";
                        //LogHelper.WriteLogList(new List<string>() { "" }, filePath_2, false);
                    }
                    if (request.Hour == 10 && scene.DomainZone() == 3)
                    {
                        //打印拍卖
                    }

                    break;
                case SceneType.Map:
                    //Log.Console($"{scene.Name}  {scene.DomainZone()}  request.FunctionType: {request.FunctionId} {request.FunctionType}");
                    if (request.FunctionId == 1057 && request.FunctionType == 1)
                    {
                        
                    }
                    if (request.FunctionId == 1057 && request.FunctionType == 2)
                    {
                        List<Unit> units = UnitHelper.GetUnitList(scene, UnitType.Npc);
                        for (int i = units.Count - 1; i >= 0; i--)
                        {
                            if (units[i].ConfigId >= 20099007 && units[i].ConfigId <= 20099010)
                            {
                                scene.GetComponent<UnitComponent>().Remove(units[i].Id);
                            }
                        }
                    }
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
                    if (request.FunctionId == 1052 && request.FunctionType == 2)
                    {
                        //Log.Console("OnShowLieOver");
                        Log.Warning("OnShowLieOver");
                        scene.GetComponent<RankSceneComponent>().OnShowLieOver().Coroutine();
                    }
                    if (request.FunctionId == 1044 && request.FunctionType == 2)
                    {
                        //Log.Console("RankSceneComponent.OnUnionRaceOver");
                        scene.GetComponent<RankSceneComponent>().OnUnionRaceOver().Coroutine();
                    }
                    if (request.FunctionId == 1059 && request.FunctionType == 2)
                    {
                        Log.Warning("RankSceneComponent.OnDemonOver");
                        scene.GetComponent<RankSceneComponent>().OnDemonOver().Coroutine();
                    }
                    break;
                case SceneType.FubenWork:
                    //Log.Console($"Arena定时刷新: {scene.DomainZone()} {hour}");
                    LogHelper.LogWarning($"Arena定时刷新: {scene.DomainZone()} {hour}", true);
                    if (hour == 0)
                    {
                        scene.GetComponent<ArenaSceneComponent>().OnZeroClockUpdate();
                    }
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
                            item.Value.GetComponent<YeWaiRefreshComponent>().OnZeroClockUpdate(request.OpenDay);
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

                    if (hour == 0)
                    {
                        scene.GetComponent<BattleSceneComponent>().OnZeroClockUpdate();
                    }
                    if (request.FunctionId == 1025 && request.FunctionType == 1)
                    {
                        //Log.Console("OnBattleOpen");scene
                      
                        scene.GetComponent<FubenCenterComponent>().OnActivityOpen(request.FunctionId);  

                        //scene.GetComponent<BattleSceneComponent>().OnBattleOpen();
                    }
                    if (request.FunctionId == 1025 && request.FunctionType == 2)
                    {
                        // Log.Console("OnBattleOver");
                        //scene.GetComponent<BattleSceneComponent>().OnBattleOver().Coroutine();
                    }
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

                    /*if (hour == 0 && self.DomainZone() == 3) //通知中心服
                    {
                        Console.WriteLine($"通知中心服:  {hour}");
                        long centerid = DBHelper.GetRealmCenter();
                        A2A_ActivityUpdateResponse m2m_TrasferUnitResponse = (A2A_ActivityUpdateResponse)await ActorMessageSenderComponent.Instance.Call
                                (centerid, new A2A_ActivityUpdateRequest() { Hour = hour });
                    }
                    if ((hour == 0 || hour == 21) && self.DomainZone() == 3) //通知账号中心服
                    {
                        Console.WriteLine($"通知账号中心服:  {hour}");
                        long centerid = DBHelper.GetRealmCenter();
                        A2A_ActivityUpdateResponse m2m_TrasferUnitResponse = (A2A_ActivityUpdateResponse)await ActorMessageSenderComponent.Instance.Call
                                (centerid, new A2A_ActivityUpdateRequest() { Hour = hour });
                    }*/
                    
                    ///可以移动到CenterServerComponent
                    //if (hour == 0)
                   
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
