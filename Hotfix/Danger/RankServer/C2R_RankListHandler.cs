using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2R_RankListHandler : AMActorRpcHandler<Scene, C2R_RankListRequest, R2C_RankListResponse>
    {
        protected override async ETTask Run(Scene scene, C2R_RankListRequest request, R2C_RankListResponse response, Action reply)
        {
            if (request.BelongId == BelongReddot.Belong_10902)
            {
                if (request.Scope != 0 && request.Scope != 1)
                {
                    response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                    reply();
                    return;
                }

                await FillUnionRankList(scene, response);
                reply();
                return;
            }

            if (request.BelongId != 10901)
            {
                reply();
                await ETTask.CompletedTask;
                return;
            }

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

        /// <summary>本服读取当前区公会；战区读取配置的成员区公会，全部成功后统一排序并截取榜单。</summary>
        private static async ETTask FillUnionRankList(Scene scene, R2C_RankListResponse response)
        {
            List<int> zones = scene.SceneType == SceneType.WZRank
                    ? StartZoneConfigCategory.Instance.GetWarZoneMembers(scene.DomainZone())
                    : new List<int> { scene.DomainZone() };
            List<UnionListItem> unions = new List<UnionListItem>();
            HashSet<long> unionIds = new HashSet<long>();
            for (int i = 0; i < zones.Count; i++)
            {
                U2C_UnionListResponse unionResponse = (U2C_UnionListResponse)await ActorMessageSenderComponent.Instance.Call(
                        DBHelper.GetUnionServerId(zones[i]), new C2U_UnionListRequest());
                if (unionResponse.Error != ErrorCode.ERR_Success)
                {
                    response.Error = unionResponse.Error;
                    return;
                }

                foreach (UnionListItem union in unionResponse.UnionList)
                {
                    if (!unionIds.Add(union.UnionId))
                    {
                        continue;
                    }

                    unions.Add(union);
                }
            }

            unions.Sort(CompareUnionRanks);
            response.UnionRankList = unions.GetRange(0, Math.Min(unions.Count, CommonConfig.RankNumber));
        }

        /// <summary>等级高者优先，其次人数多者优先，再按创建时间从早到晚；同时间用唯一 ID 稳定排序。</summary>
        private static int CompareUnionRanks(UnionListItem a, UnionListItem b)
        {
            int result = b.UnionLevel.CompareTo(a.UnionLevel);
            if (result != 0)
            {
                return result;
            }

            result = b.PlayerNumber.CompareTo(a.PlayerNumber);
            if (result != 0)
            {
                return result;
            }

            result = a.CreateTime.CompareTo(b.CreateTime);
            return result != 0 ? result : a.UnionId.CompareTo(b.UnionId);
        }
    }
}
