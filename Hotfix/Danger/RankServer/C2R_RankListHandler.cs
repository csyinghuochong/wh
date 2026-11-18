using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ET
{
    [ActorMessageHandler]
    public class C2R_RankListHandler : AMActorRpcHandler<Scene, C2R_RankListRequest, R2C_RankListResponse>
    {
        protected override async ETTask Run(Scene scene, C2R_RankListRequest request, R2C_RankListResponse response, Action reply)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            Console.WriteLine($"[RankListTiming] time={DateTime.Now:HH:mm:ss.fff} process={Game.Options.Process} rpc={request.RpcId} stage=Rank.HandlerEnter scene={scene.SceneType} scope={request.Scope} belongId={request.BelongId}");
            if (request.BelongId != 10901)
            {
                Console.WriteLine($"[RankListTiming] time={DateTime.Now:HH:mm:ss.fff} process={Game.Options.Process} rpc={request.RpcId} stage=Rank.UnsupportedBelong elapsedMs={stopwatch.Elapsed.TotalMilliseconds:F3}");
                stopwatch.Restart();
                reply();
                Console.WriteLine($"[RankListTiming] time={DateTime.Now:HH:mm:ss.fff} process={Game.Options.Process} rpc={request.RpcId} stage=Rank.ReplyReturned elapsedMs={stopwatch.Elapsed.TotalMilliseconds:F3}");
                await ETTask.CompletedTask;
                return;
            }

            if (scene.SceneType == SceneType.WZRank)
            {
                stopwatch.Restart();
                WZRankSceneComponent wzRank = scene.GetComponent<WZRankSceneComponent>();
                response.RankList = wzRank.GetRankList();
                Console.WriteLine($"[RankListTiming] time={DateTime.Now:HH:mm:ss.fff} process={Game.Options.Process} rpc={request.RpcId} stage=Rank.BuildWarList count={response.RankList.Count} elapsedMs={stopwatch.Elapsed.TotalMilliseconds:F3}");
                stopwatch.Restart();
                reply();
                Console.WriteLine($"[RankListTiming] time={DateTime.Now:HH:mm:ss.fff} process={Game.Options.Process} rpc={request.RpcId} stage=Rank.ReplyReturned elapsedMs={stopwatch.Elapsed.TotalMilliseconds:F3}");
                await ETTask.CompletedTask;
                return;
            }

            stopwatch.Restart();
            RankSceneComponent rankComponent = scene.GetComponent<RankSceneComponent>();
            List<RankingInfo> all = rankComponent.DBRankInfo.rankingInfos;
            List<RankingInfo> list = all.GetRange(0, all.Count > CommonConfig.RankNumber ? CommonConfig.RankNumber : all.Count);
            response.RankList = list;

            Console.WriteLine($"[RankListTiming] time={DateTime.Now:HH:mm:ss.fff} process={Game.Options.Process} rpc={request.RpcId} stage=Rank.BuildLocalList totalCount={all.Count} count={list.Count} elapsedMs={stopwatch.Elapsed.TotalMilliseconds:F3}");
            stopwatch.Restart();
            reply();
            Console.WriteLine($"[RankListTiming] time={DateTime.Now:HH:mm:ss.fff} process={Game.Options.Process} rpc={request.RpcId} stage=Rank.ReplyReturned elapsedMs={stopwatch.Elapsed.TotalMilliseconds:F3}");
            await ETTask.CompletedTask;
        }
    }
}
