using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2R_RankListHandler : AMActorRpcHandler<Scene, C2R_RankListRequest, R2C_RankListResponse>
    {
        protected override async ETTask Run(Scene scene, C2R_RankListRequest request, R2C_RankListResponse response, Action reply)
        {
            if (scene.SceneType == SceneType.WZRank)
            {
                WZRankSceneComponent wzRank = scene.GetComponent<WZRankSceneComponent>();
                response.RankList = wzRank.GetRankList();
                reply();
                await ETTask.CompletedTask;
                return;
            }

            RankSceneComponent rankComponent = scene.GetComponent<RankSceneComponent>();
            List<RankingInfo> all = rankComponent.DBRankInfo.rankingInfos;
            List<RankingInfo> list = all.GetRange(0, all.Count > CommonConfig.RankNumber ? CommonConfig.RankNumber : all.Count);
            response.RankList = list;

            reply();
            await ETTask.CompletedTask;
        }
    }
}
