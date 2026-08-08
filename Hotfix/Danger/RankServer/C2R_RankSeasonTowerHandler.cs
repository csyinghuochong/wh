using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2R_RankSeasonTowerHandler : AMActorRpcHandler<Scene, C2R_RankSeasonTowerRequest, R2C_RankSeasonTowerResponse>
    {
        protected override async ETTask Run(Scene scene, C2R_RankSeasonTowerRequest request, R2C_RankSeasonTowerResponse response, Action reply)
        {
            long timeNow = TimeHelper.ServerNow();
            RankSceneComponent rankComponent = scene.GetComponent<RankSceneComponent>();

            //rankComponent.ClearSeasonTowerRankByUnitId(3096816747831951360);  //诗香
            //rankComponent.ClearSeasonTowerRankByUnitId(3089593540553015296);  //白血伤

            if (timeNow - rankComponent.RankSeasonTowerLastTime < TimeHelper.Second * 10)
            {
                response.RankList = rankComponent.RankSeasonTowers;
            }
            else
            {
                List<KeyValuePairLong> ranklist = rankComponent.DBRankInfo.rankSeasonTower;

                List<long> idlist = new List<long>();
                List<long> idremove = new List<long>();

                for (int i = 0; i < ranklist.Count; i++)
                {
                    if (idlist.Contains(ranklist[i].KeyId))
                    {
                        idremove.Add(ranklist[i].KeyId);
                        continue;
                    }

                    idlist.Add(ranklist[i].KeyId);
                    RoleInfoComponentServer roleInfoComponentServer = await DBHelper.GetComponent<RoleInfoComponentServer>(UnitZoneHelper.GetHomeZone(ranklist[i].KeyId), ranklist[i].KeyId);
                    if (roleInfoComponentServer == null)
                    {
                        continue;
                    }
                    response.RankList.Add(new RankSeasonTowerInfo()
                    {
                        UserId = ranklist[i].KeyId,
                        TotalTime = ranklist[i].Value,        //时间
                        FubenId = (int)(ranklist[i].Value2),  //副本
                        PlayerLv = roleInfoComponentServer.RoleInfo.Lv,
                        PlayerName = roleInfoComponentServer.RoleInfo.Name,
                        Occ = roleInfoComponentServer.RoleInfo.Occ,
                    });
                }
                rankComponent.RankSeasonTowerLastTime = TimeHelper.ServerNow();
                rankComponent.RankSeasonTowers = response.RankList;

                for (int remove = 0; remove < idremove.Count; remove++)
                {
                    for (int i = ranklist.Count - 1; i >= 0; i--)
                    {
                        if (ranklist[i].KeyId == idremove[remove])
                        {
                            ranklist.RemoveAt(i);
                            break;
                        }
                    }
                }
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
