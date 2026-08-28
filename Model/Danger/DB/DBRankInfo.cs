using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace ET
{
    [BsonIgnoreExtraElements]
    public class DBRankInfo : Entity, IAwake
    {
        public List<RankPetInfo> rankingPets = new List<RankPetInfo>();     //宠物天梯
        public List<RankingInfo> rankingInfos = new List<RankingInfo>();    //战力排行


        public List<RankingInfo> rankSoloInfo = new List<RankingInfo>();    //solo

        public List<LongLongPair> rankingTrial = new List<LongLongPair>();   //试炼副本伤害排行
        public List<LongLongPair> rankSeasonTower = new List<LongLongPair>();   //试炼副本伤害排行  id/层数/时间
    }
}
