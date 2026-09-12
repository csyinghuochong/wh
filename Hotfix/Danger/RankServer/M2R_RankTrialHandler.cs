using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class M2R_RankTrialHandler : AMActorRpcHandler<Scene, M2R_RankTrialRequest, R2M_RankTrialResponse>
    {
        protected override async ETTask Run(Scene scene, M2R_RankTrialRequest request, R2M_RankTrialResponse response, Action reply)
        {
            RankSceneComponent rankSceneComponent = scene.GetComponent<RankSceneComponent>();
            List<LongLongPair> rankTrial = rankSceneComponent.DBRankInfo.rankingTrial;

            bool have = false;
            for (int i = 0; i < rankTrial.Count; i++)
            {
                if (rankTrial[i].KeyId != request.RankingInfo.KeyId)
                {
                    continue;
                }
                if (rankTrial[i].Value2 > request.RankingInfo.Value2)
                {
                    continue;
                }

                if (rankTrial[i].Value2 < request.RankingInfo.Value2)
                {
                    rankTrial[i].Value = request.RankingInfo.Value;
                    rankTrial[i].Value2 = request.RankingInfo.Value2;
                }
                else
                {
                    rankTrial[i].Value = Math.Max(rankTrial[i].Value, request.RankingInfo.Value);
                }
                have = true;
            }

            if (!have)
            {
                rankTrial.Add(request.RankingInfo);
            }

            ///试炼之塔排行先按照层树排序,层序一样按照秒伤 试炼排行榜得秒伤处也显示层数和秒伤,比如40层50000秒伤 显示格式为: 40层(50000/秒)
            rankTrial.Sort(delegate (LongLongPair a, LongLongPair b)
            {
                if (b.Value2 == a.Value2)
                {
                    return (int)b.Value - (int)a.Value;
                }
                else
                {
                    return (int)b.Value2 - (int)a.Value2;
                }
            });

            int maxnumber = Math.Min(rankTrial.Count, CommonConfig.RankNumber);
            rankSceneComponent.DBRankInfo.rankingTrial = rankTrial.GetRange(0, maxnumber);
            response.RankId = rankSceneComponent.GetTrialRank(request.RankingInfo.KeyId);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
