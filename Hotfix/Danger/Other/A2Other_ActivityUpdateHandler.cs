using AlibabaCloud.SDK.Sample;
using NLog.Fluent;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{

    /// <summary>
    /// 活动开启/关闭（FunctionId + FunctionType）。日清走 A2Other_DailyResetRequest。
    /// </summary>
    [ActorMessageHandler]
    public class A2Other_ActivityUpdateHandler : AMActorRpcHandler<Scene, A2Other_ActivityUpdateRequest, Other2A_ActivityUpdateResponse>
    {

        protected override async ETTask Run(Scene scene, A2Other_ActivityUpdateRequest request, Other2A_ActivityUpdateResponse response, Action reply)
        {
            switch (scene.SceneType)
            {
                case SceneType.Rank:
                    LogHelper.LogWarning($"排行榜活动刷新: {scene.DomainZone()} FunctionId: {request.FunctionId}", true);
                    if (request.FunctionId == 1052 && request.FunctionType == 1)
                    {
                        Log.Warning("OnShowLieBegin");
                        scene.GetComponent<RankSceneComponent>().OnShowLieBegin();
                    }
                    break;
                case SceneType.FubenWork:
                    LogHelper.LogWarning($"Arena活动刷新: {scene.DomainZone()} FunctionId: {request.FunctionId}", true);
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
                    LogHelper.LogWarning($"Union活动刷新: {scene.DomainZone()} FunctionId: {request.FunctionId}", true);
                    if (request.FunctionId == 1043 && request.FunctionType == 1)
                    {
                        scene.GetComponent<UnionSceneComponent>().OnUnionBoss();
                    }
                    if (request.FunctionId == 1044 && request.FunctionType == 1)
                    {
                        scene.GetComponent<UnionSceneComponent>().OnUnionRaceBegin().Coroutine();
                    }
                    if (request.FunctionId == 1044 && request.FunctionType == 2)
                    {
                        //scene.GetComponent<UnionSceneComponent>().OnUnionRaceOver().Coroutine();
                    }
                    break;
                case SceneType.FubenCenter:
                    if (request.FunctionId > 0 && request.FunctionType == 1)
                    {
                        FubenCenterComponent fubenCenter = scene.GetComponent<FubenCenterComponent>();
                        fubenCenter.OnActivityOpen(request.FunctionId);
                    }
                    if (request.FunctionId > 0 && request.FunctionType == 2)
                    {
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
                default:
                    break;
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
