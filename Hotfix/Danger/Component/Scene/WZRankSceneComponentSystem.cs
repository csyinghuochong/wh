using System;
using System.Collections.Generic;

namespace ET
{
    [Timer(TimerType.WZRankTimer)]
    public class WZRankTimer : ATimer<WZRankSceneComponent>
    {
        public override void Run(WZRankSceneComponent self)
        {
            try
            {
                self.SaveDB().Coroutine();
            }
            catch (Exception e)
            {
                Log.Error($"WZRank timer error: {self.Id}\n{e}");
            }
        }
    }

    [ObjectSystem]
    public class WZRankSceneComponentAwakeSystem : AwakeSystem<WZRankSceneComponent>
    {
        public override void Awake(WZRankSceneComponent self)
        {
            self.DBRankInfo = new DBRankInfo { Id = self.DomainZone() };
            self.InitDBRankInfo().Coroutine();

            long dbTime = TimeHelper.Minute * 30 + RandomHelper.RandomNumber(1000, 10000);
            if(CommonHelper.IsInnerNet())
            {
                dbTime = TimeHelper.Minute;
            }

            self.Timer = TimerComponent.Instance.NewRepeatedTimer(
                dbTime,  TimerType.WZRankTimer,
                self);
            Log.Console($"[WZRank] Awake zone={self.DomainZone()}");
        }
    }

    [ObjectSystem]
    public class WZRankSceneComponentDestroySystem : DestroySystem<WZRankSceneComponent>
    {
        public override void Destroy(WZRankSceneComponent self)
        {
            TimerComponent.Instance.Remove(ref self.Timer);
        }
    }

    public static class WZRankSceneComponentSystem
    {
        public static async ETTask InitDBRankInfo(this WZRankSceneComponent self)
        {
            await TimerComponent.Instance.WaitAsync(TimeHelper.Second);
            DBRankInfo dbRankInfo = await DBHelper.GetComponent<DBRankInfo>(self.DomainZone(), self.DomainZone());
            if (dbRankInfo == null)
            {
                self.DBRankInfo = new DBRankInfo { Id = self.DomainZone() };
            }
            else
            {
                self.DBRankInfo = dbRankInfo;
            }
        }

        public static async ETTask SaveDB(this WZRankSceneComponent self)
        {
            if (self.DBRankInfo == null)
            {
                return;
            }
            await DBHelper.SaveComponent(self.DomainZone(), self.DBRankInfo.Id, self.DBRankInfo);
        }

        public static void OnRecvRankUpdate(this WZRankSceneComponent self, RankingInfo rankingInfo)
        {
            if (rankingInfo == null || rankingInfo.UserId == 0)
            {
                return;
            }
            self.UpdateRankList(rankingInfo);
        }

        public static void UpdateRankList(this WZRankSceneComponent self, RankingInfo rankingInfo)
        {
            int oldRankIndex = -1;
            for (int i = 0; i < self.DBRankInfo.rankingInfos.Count; i++)
            {
                if (self.DBRankInfo.rankingInfos[i].UserId == rankingInfo.UserId)
                {
                    oldRankIndex = i;
                    break;
                }
            }

            if (oldRankIndex == -1)
            {
                self.DBRankInfo.rankingInfos.Add(rankingInfo);
            }
            else
            {
                self.DBRankInfo.rankingInfos[oldRankIndex] = rankingInfo;
            }

            self.DBRankInfo.rankingInfos.Sort((a, b) => (int)b.Combat - (int)a.Combat);

            if (self.DBRankInfo.rankingInfos.Count > 500)
            {
                self.DBRankInfo.rankingInfos.RemoveAt(self.DBRankInfo.rankingInfos.Count - 1);
            }
        }

        public static int GetCombatRank(this WZRankSceneComponent self, long userId)
        {
            for (int i = 0; i < self.DBRankInfo.rankingInfos.Count; i++)
            {
                if (self.DBRankInfo.rankingInfos[i].UserId == userId)
                {
                    return i + 1;
                }
            }
            return 0;
        }

        public static int GetOccCombatRank(this WZRankSceneComponent self, long userId, int occ)
        {
            int occRank = 0;
            for (int i = 0; i < self.DBRankInfo.rankingInfos.Count; i++)
            {
                RankingInfo info = self.DBRankInfo.rankingInfos[i];
                if (info.Occ == occ)
                {
                    occRank++;
                }
                if (info.UserId == userId)
                {
                    return occRank;
                }
            }
            return 0;
        }

        public static List<RankingInfo> GetRankList(this WZRankSceneComponent self)
        {
            List<RankingInfo> all = self.DBRankInfo.rankingInfos;
            int count = all.Count > CommonConfig.RankNumber ? CommonConfig.RankNumber : all.Count;
            return all.GetRange(0, count);
        }
    }
}
